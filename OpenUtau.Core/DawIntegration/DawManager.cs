using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;
using Serilog;

namespace OpenUtau.Core.DawIntegration {
    /// <summary>What a pending sync covers. Values are ordered as PROTOCOL.md §7 requires them sent.</summary>
    public enum DawSyncKind {
        Ustx = 0,
        Tracks = 1,
        PartLayout = 2,
    }

    /// <summary>
    /// Trailing-edge debounce for the three sync streams (PROTOCOL.md §7): 1 s for
    /// <c>updateUstx</c>/<c>updateTracks</c>, 5 s for <c>updatePartLayout</c> and its audio.
    /// </summary>
    /// <remarks>
    /// Time is passed in rather than read from the clock, so the pump can be a timer in
    /// production and a plain loop in tests without any sleeping.
    /// </remarks>
    public sealed class DawSyncScheduler {
        public static readonly TimeSpan DefaultFastDebounce = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan DefaultSlowDebounce = TimeSpan.FromSeconds(5);

        private readonly TimeSpan fast;
        private readonly TimeSpan slow;
        private readonly object lockObj = new object();
        private readonly Dictionary<DawSyncKind, DateTime> due = new Dictionary<DawSyncKind, DateTime>();

        public DawSyncScheduler(TimeSpan? fastDebounce = null, TimeSpan? slowDebounce = null) {
            fast = fastDebounce ?? DefaultFastDebounce;
            slow = slowDebounce ?? DefaultSlowDebounce;
        }

        public TimeSpan DebounceFor(DawSyncKind kind) =>
            kind == DawSyncKind.PartLayout ? slow : fast;

        public bool HasPending {
            get {
                lock (lockObj) {
                    return due.Count > 0;
                }
            }
        }

        /// <summary>Marks a stream dirty, pushing its due time out to a full debounce from now.</summary>
        public void Touch(DawSyncKind kind, DateTime now) {
            lock (lockObj) {
                due[kind] = now + DebounceFor(kind);
            }
        }

        /// <summary>Makes every pending stream due immediately — the <c>playbackStarted</c> flush (§7).</summary>
        public void FlushPending(DateTime now) {
            lock (lockObj) {
                foreach (var kind in due.Keys.ToList()) {
                    due[kind] = now;
                }
            }
        }

        /// <summary>Marks one stream dirty and immediately due, bypassing its debounce.</summary>
        public void MakeDue(DawSyncKind kind, DateTime now) {
            lock (lockObj) {
                due[kind] = now;
            }
        }

        /// <summary>Marks every stream dirty and immediately due. Used for the post-(re)connect full sync.</summary>
        public void RequestFullSync(DateTime now) {
            lock (lockObj) {
                foreach (DawSyncKind kind in Enum.GetValues<DawSyncKind>()) {
                    due[kind] = now;
                }
            }
        }

        public void Clear() {
            lock (lockObj) {
                due.Clear();
            }
        }

        /// <summary>Takes the streams whose debounce has elapsed, in §7 send order.</summary>
        public DawSyncKind[] TryTake(DateTime now) {
            lock (lockObj) {
                var ready = due
                    .Where(entry => entry.Value <= now)
                    .Select(entry => entry.Key)
                    .OrderBy(kind => (int)kind)
                    .ToArray();
                foreach (var kind in ready) {
                    due.Remove(kind);
                }
                return ready;
            }
        }
    }

    /// <summary>
    /// Bounded hash → PCM store. <c>updatePartLayout</c> advertises hashes and the plugin pulls
    /// them later with <c>getAudio</c> (PROTOCOL.md §6.2), so the bytes have to outlive the
    /// message that named them.
    /// </summary>
    /// <remarks>
    /// A three-minute stereo part is roughly 63 MB of float32, so the store is capped and evicts
    /// least-recently-used entries. A miss is recoverable: <see cref="DawManager"/> re-extracts
    /// the audio from the part that owns the hash.
    /// </remarks>
    public sealed class DawAudioCache {
        public const long DefaultCapacityBytes = 256L * 1024 * 1024;

        private sealed class Entry {
            public byte[] Pcm = Array.Empty<byte>();
            public long Stamp;
        }

        private readonly long capacity;
        private readonly object lockObj = new object();
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
        private long stamp;
        private long sizeBytes;

        public DawAudioCache(long capacityBytes = DefaultCapacityBytes) {
            capacity = Math.Max(1, capacityBytes);
        }

        public long SizeBytes { get { lock (lockObj) { return sizeBytes; } } }
        public int Count { get { lock (lockObj) { return entries.Count; } } }

        public void Put(string hash, byte[] pcm) {
            lock (lockObj) {
                if (entries.TryGetValue(hash, out var previous)) {
                    sizeBytes -= previous.Pcm.Length;
                }
                entries[hash] = new Entry { Pcm = pcm, Stamp = ++stamp };
                sizeBytes += pcm.Length;
                Evict();
            }
        }

        public bool TryGet(string hash, out byte[] pcm) {
            lock (lockObj) {
                if (entries.TryGetValue(hash, out var entry)) {
                    entry.Stamp = ++stamp;
                    pcm = entry.Pcm;
                    return true;
                }
            }
            pcm = Array.Empty<byte>();
            return false;
        }

        /// <summary>Drops every hash the current layout no longer advertises.</summary>
        public void Retain(ICollection<string> keep) {
            lock (lockObj) {
                foreach (string hash in entries.Keys.Where(hash => !keep.Contains(hash)).ToList()) {
                    sizeBytes -= entries[hash].Pcm.Length;
                    entries.Remove(hash);
                }
            }
        }

        public void Clear() {
            lock (lockObj) {
                entries.Clear();
                sizeBytes = 0;
            }
        }

        /// <summary>Keeps one entry whatever its size, so an oversized part can still be served.</summary>
        private void Evict() {
            while (sizeBytes > capacity && entries.Count > 1) {
                string oldest = entries.OrderBy(entry => entry.Value.Stamp).First().Key;
                sizeBytes -= entries[oldest].Pcm.Length;
                entries.Remove(oldest);
            }
        }
    }

    /// <summary>Connection state, surfaced to the UI.</summary>
    public enum DawConnectionState {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
    }

    /// <summary>
    /// Drives one plugin connection: the <see cref="ICmdSubscriber"/> subscription that marks
    /// streams dirty, the debounced sync pump, audio serving and reconnect backoff
    /// (PROTOCOL.md §7, §9).
    /// </summary>
    public sealed class DawManager : SingletonBase<DawManager>, ICmdSubscriber, IDisposable {
        /// <summary>§3: 500 ms, 1 s, 2 s, then give up and tell the user.</summary>
        public static readonly TimeSpan[] DefaultReconnectBackoff = {
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
        };

        /// <summary>§3: the init handshake gets 5 s, not the ordinary 10 s request budget.</summary>
        public static readonly TimeSpan InitTimeout = TimeSpan.FromSeconds(5);

        /// <summary>Pump period. Well under the 1 s fast debounce, so it never dominates latency.</summary>
        public static readonly TimeSpan DefaultPumpInterval = TimeSpan.FromMilliseconds(200);

        private readonly DawSyncScheduler scheduler;
        private readonly DawAudioCache audioCache;
        private readonly TimeSpan[] reconnectBackoff;
        private readonly SemaphoreSlim syncGate = new SemaphoreSlim(1, 1);
        private readonly object stateLock = new object();

        /// <summary>Advertised hash → the part that produced it, so a cache miss can be re-extracted.</summary>
        private readonly Dictionary<string, UVoicePart> hashOwners = new Dictionary<string, UVoicePart>();

        private DawTransport? transport;
        private Timer? pump;
        private DawServer? server;
        private bool subscribed;
        private volatile bool closingLocally;
        private int disposed;

        public DawManager() : this(null) { }

        public DawManager(
            DawSyncScheduler? syncScheduler,
            DawAudioCache? cache = null,
            TimeSpan[]? backoff = null) {
            scheduler = syncScheduler ?? new DawSyncScheduler();
            audioCache = cache ?? new DawAudioCache();
            reconnectBackoff = backoff ?? DefaultReconnectBackoff;
        }

        public DawConnectionState State { get; private set; } = DawConnectionState.Disconnected;
        public bool IsConnected => transport?.IsConnected == true;
        public string ServerName => server?.Name ?? string.Empty;
        public DawSyncScheduler Scheduler => scheduler;
        public DawAudioCache AudioCache => audioCache;

        /// <summary>Injectable clock, so debounce tests never sleep.</summary>
        public Func<DateTime> NowUtc { get; set; } = () => DateTime.UtcNow;

        /// <summary>
        /// The project being synced. Defaults to the open document; injectable so tests can drive
        /// a hand-built project without standing up <see cref="DocManager"/>'s UI thread.
        /// </summary>
        public Func<UProject> ProjectSource { get; set; } = () => DocManager.Inst.Project;

        public DawTransportOptions TransportOptions { get; set; } = DawTransportOptions.Default;

        /// <summary>Set to false in tests, which drive <see cref="PumpOnceAsync"/> by hand.</summary>
        public bool UseTimerPump { get; set; } = true;

        public event Action<DawConnectionState>? StateChanged;

        /// <summary>Raised when reconnection is exhausted. The UI turns this into a visible error.</summary>
        public event Action<string>? ConnectionLost;

        /// <summary>
        /// Connects to a discovered plugin, performs the init handshake and starts syncing.
        /// </summary>
        /// <exception cref="DawProtocolException">
        /// The advertisement, or the plugin's own init answer, is an api major this build cannot speak (§4).
        /// </exception>
        public async Task ConnectAsync(DawServer target, CancellationToken cancellation = default) {
            if (!target.IsCompatible) {
                throw new DawProtocolException(
                    $"Plugin '{target.Name}' speaks api '{target.Info.ApiVersion}', " +
                    $"this build speaks {DawApiVersion.CurrentString}.");
            }
            await DisconnectAsync();
            lock (stateLock) {
                server = target;
            }
            closingLocally = false;
            SetState(DawConnectionState.Connecting);
            try {
                await OpenAsync(target.Port, cancellation);
            } catch {
                // A half-open connection must not trigger the reconnect ladder.
                closingLocally = true;
                await TeardownAsync();
                SetState(DawConnectionState.Disconnected);
                throw;
            }
        }

        private async Task OpenAsync(int port, CancellationToken cancellation) {
            var opened = await DawTransport.ConnectAsync(port, TransportOptions, NowUtc, cancellation);
            opened.Notification += OnPluginNotification;
            opened.Disconnected += OnTransportDisconnected;
            opened.RequestHandler = ServeRequestAsync;
            lock (stateLock) {
                transport = opened;
            }
            // §6.1 as decided: init carries the USTX baseline and the answer is the api version.
            string ustx = await SerializeProjectAsync();
            var response = await opened.SendRequestAsync<InitResponse>(
                DawMessageKind.Init, new InitRequest { Ustx = ustx }, InitTimeout, cancellation);
            if (!DawApiVersion.TryParse(response.ApiVersion, out var version)
                || !version.IsCompatibleWith(DawApiVersion.Current)) {
                throw new DawProtocolException(
                    $"Plugin answered init with api '{response.ApiVersion}', " +
                    $"which this build cannot speak.");
            }
            Subscribe();
            SetState(DawConnectionState.Connected);
            // init already delivered the USTX, so only tracks and layout are outstanding (§7, §9).
            var now = NowUtc();
            scheduler.MakeDue(DawSyncKind.Tracks, now);
            scheduler.MakeDue(DawSyncKind.PartLayout, now);
            StartPump();
            await PumpOnceAsync(cancellation);
        }

        /// <summary>
        /// Serializes the open project to USTX YAML — byte-identical to what <c>Ustx.Save</c>
        /// writes. <see cref="DawUstx.Serialize"/> runs <c>BeforeSave</c>/<c>AfterSave</c>, which
        /// mutate the project, so it has to happen on the document thread.
        /// </summary>
        private Task<string> SerializeProjectAsync() {
            return OnDocumentThreadAsync(() => DawUstx.Serialize(ProjectSource()));
        }

        /// <summary>
        /// Runs work on the document thread. <see cref="DocManager.MainScheduler"/> is null until
        /// the UI calls <c>Initialize</c>, which is the case in tests; then the work runs inline.
        /// </summary>
        private Task<T> OnDocumentThreadAsync<T>(Func<T> work) {
            var docScheduler = DocManager.Inst.MainScheduler;
            if (docScheduler == null) {
                return Task.FromResult(work());
            }
            return Task.Factory.StartNew(work, CancellationToken.None, TaskCreationOptions.None, docScheduler);
        }

        private void Subscribe() {
            lock (stateLock) {
                if (subscribed) {
                    return;
                }
                DocManager.Inst.AddSubscriber(this);
                subscribed = true;
            }
        }

        private void Unsubscribe() {
            lock (stateLock) {
                if (!subscribed) {
                    return;
                }
                DocManager.Inst.RemoveSubscriber(this);
                subscribed = false;
            }
        }

        private void SetState(DawConnectionState next) {
            bool changed;
            lock (stateLock) {
                changed = State != next;
                State = next;
            }
            if (changed) {
                StateChanged?.Invoke(next);
            }
        }

        private void StartPump() {
            if (!UseTimerPump) {
                return;
            }
            lock (stateLock) {
                pump?.Dispose();
                pump = new Timer(
                    _ => PumpOnceAsync(CancellationToken.None).ContinueWith(
                        task => Log.Error(task.Exception!, "DAW: sync pump failed."),
                        TaskContinuationOptions.OnlyOnFaulted),
                    null,
                    DefaultPumpInterval,
                    DefaultPumpInterval);
            }
        }

        private void StopPump() {
            lock (stateLock) {
                pump?.Dispose();
                pump = null;
            }
        }

        /// <summary>
        /// The document command stream. Runs on the document thread for every single edit, so it
        /// only ever sets flags — the pump does the work (§7).
        /// </summary>
        public void OnNext(UCommand cmd, bool isUndo) {
            if (!IsConnected) {
                return;
            }
            var now = NowUtc();
            switch (cmd) {
                case VolumeChangeNotification:
                case PanChangeNotification:
                case SoloTrackNotification:
                    scheduler.Touch(DawSyncKind.Tracks, now);
                    break;
                case PartRenderedNotification:
                    scheduler.Touch(DawSyncKind.PartLayout, now);
                    break;
                case LoadProjectNotification:
                    // A different project: nothing the plugin holds is valid any more.
                    audioCache.Clear();
                    lock (stateLock) {
                        hashOwners.Clear();
                    }
                    scheduler.RequestFullSync(now);
                    break;
                case UNotification:
                    // Transient UI state — play position, selection, progress. Not project data.
                    break;
                case TrackCommand:
                    // Adding, removing or renaming a track moves parts between track numbers too.
                    scheduler.Touch(DawSyncKind.Tracks, now);
                    scheduler.Touch(DawSyncKind.Ustx, now);
                    scheduler.Touch(DawSyncKind.PartLayout, now);
                    break;
                default:
                    // A real edit. The audio follows once the renderer reports back.
                    scheduler.Touch(DawSyncKind.Ustx, now);
                    scheduler.Touch(DawSyncKind.PartLayout, now);
                    break;
            }
        }

        /// <summary>
        /// Sends whatever the debounce has made due, in §7 order. A gate serializes ticks so a
        /// slow layout sync can never overlap the next one.
        /// </summary>
        public async Task PumpOnceAsync(CancellationToken cancellation = default) {
            if (!IsConnected || !scheduler.HasPending) {
                return;
            }
            if (!await syncGate.WaitAsync(0, cancellation)) {
                // Already syncing. Whatever is due simply waits for the next tick.
                return;
            }
            try {
                foreach (var kind in scheduler.TryTake(NowUtc())) {
                    if (!IsConnected) {
                        break;
                    }
                    await SyncAsync(kind, cancellation);
                }
            } catch (OperationCanceledException) {
                // Shutting down.
            } catch (TimeoutException e) {
                // §8: a request timeout means the connection is dead. Hand it to the reconnect path.
                Log.Warning(e, "DAW: sync timed out; dropping the connection.");
                await CloseTransportAsync();
            } catch (Exception e) {
                // A refused envelope or a serialization fault leaves the stream coherent, so the
                // connection survives and the stream is retried on the next edit.
                Log.Error(e, "DAW: sync failed.");
            } finally {
                syncGate.Release();
            }
        }

        /// <summary>Sends one stream. Public so tests and the conformance harness can force a sync.</summary>
        public async Task SyncAsync(DawSyncKind kind, CancellationToken cancellation = default) {
            var live = transport;
            if (live == null || !live.IsConnected) {
                return;
            }
            switch (kind) {
                case DawSyncKind.Ustx:
                    await live.SendNotificationAsync(
                        DawMessageKind.UpdateUstx,
                        new UpdateUstxNotification { Ustx = await SerializeProjectAsync() },
                        cancellation);
                    break;
                case DawSyncKind.Tracks:
                    await live.SendNotificationAsync(
                        DawMessageKind.UpdateTracks, await BuildTracksAsync(), cancellation);
                    break;
                case DawSyncKind.PartLayout:
                    await SyncPartLayoutAsync(live, cancellation);
                    break;
            }
        }

        private Task<UpdateTracksNotification> BuildTracksAsync() {
            return OnDocumentThreadAsync(() => new UpdateTracksNotification {
                Tracks = ProjectSource().tracks
                    .Select(track => new DawTrackInfo {
                        Name = track.TrackName,
                        Volume = track.Volume,
                        Pan = track.Pan,
                    })
                    .ToList(),
            });
        }

        /// <summary>A part's layout plus the signal it was mixed into, captured on the document thread.</summary>
        private sealed class PartSnapshot {
            public UProject Project = null!;
            public UVoicePart Part = null!;
            public int TrackNo;
            public double StartMs;
            public double EndMs;
        }

        private Task<List<PartSnapshot>> SnapshotPartsAsync() {
            return OnDocumentThreadAsync(() => {
                var project = ProjectSource();
                return project.parts
                    .OfType<UVoicePart>()
                    .Select(part => new PartSnapshot {
                        Project = project,
                        Part = part,
                        TrackNo = part.trackNo,
                        StartMs = project.timeAxis.TickPosToMsPos(part.position),
                        EndMs = project.timeAxis.TickPosToMsPos(part.End),
                    })
                    .ToList();
            });
        }

        /// <summary>
        /// Reports the layout, hashing each part's rendered audio on the way.
        /// </summary>
        /// <remarks>
        /// Parts whose render is unfinished are left out entirely rather than advertised with a
        /// placeholder hash, because <see cref="DawAudio.TryExtractPart"/> can only produce a
        /// correct hash for finished audio. <c>PartRenderedNotification</c> marks the stream dirty
        /// again, so they appear in a later sync.
        /// </remarks>
        private async Task SyncPartLayoutAsync(DawTransport live, CancellationToken cancellation) {
            var snapshot = await SnapshotPartsAsync();
            var layout = new List<DawPartLayout>(snapshot.Count);
            var owners = new Dictionary<string, UVoicePart>();
            foreach (var entry in snapshot) {
                if (!TryHashPart(entry, out string hash, out byte[] pcm)) {
                    continue;
                }
                owners[hash] = entry.Part;
                audioCache.Put(hash, pcm);
                layout.Add(new DawPartLayout {
                    TrackNo = entry.TrackNo,
                    StartMs = entry.StartMs,
                    EndMs = entry.EndMs,
                    AudioHash = hash,
                });
            }
            lock (stateLock) {
                hashOwners.Clear();
                foreach (var pair in owners) {
                    hashOwners[pair.Key] = pair.Value;
                }
            }
            audioCache.Retain(owners.Keys);
            var response = await live.SendRequestAsync<UpdatePartLayoutResponse>(
                DawMessageKind.UpdatePartLayout,
                new UpdatePartLayoutRequest { Parts = layout },
                cancellation: cancellation);
            if (response.MissingAudios.Count > 0) {
                // The plugin pulls each one itself with getAudio (§6.2); we just keep them warm.
                Log.Information(
                    $"DAW: plugin is missing {response.MissingAudios.Count} of {layout.Count} part audios.");
            }
        }

        /// <summary>
        /// Extracts and hashes one part. Runs off the document thread on purpose: a part can be
        /// tens of megabytes, and concurrent reads of a part's mix are what playback already does.
        /// </summary>
        private static bool TryHashPart(PartSnapshot entry, out string hash, out byte[] pcm) {
            hash = string.Empty;
            pcm = Array.Empty<byte>();
            if (!DawAudio.TryExtractPart(entry.Project, entry.Part, out var samples)) {
                return false;
            }
            pcm = DawAudio.ToPcmBytes(samples);
            hash = DawAudio.FormatHash(DawAudio.Hash(pcm));
            return true;
        }

        /// <summary>
        /// Serves the plugin's requests. Only <c>getAudio</c> is inbound in v1 (§6.2), and it is
        /// answered with a data-plane frame rather than an envelope.
        /// </summary>
        private async Task ServeRequestAsync(DawInboundRequest request) {
            if (request.Kind != DawMessageKind.GetAudio) {
                await request.RespondAsync(DawResult.Fail($"Unsupported request '{request.Kind}'."));
                return;
            }
            string hash;
            try {
                hash = request.ReadPayload<GetAudioRequest>().Hash;
            } catch (Exception e) {
                await request.RespondAsync(DawResult.Fail(e.Message));
                return;
            }
            if (!TryResolveAudio(hash, out byte[] pcm)) {
                await request.RespondAsync(DawResult.Fail($"No audio for hash {hash}."));
                return;
            }
            await request.RespondWithAudioAsync(hash, pcm);
        }

        /// <summary>
        /// Finds the audio behind an advertised hash: the cache first, then a re-extraction from
        /// the part that produced it. The re-extracted bytes still have to hash to the requested
        /// value — if they do not, the part was re-rendered and the plugin is asking about audio
        /// that no longer exists, which the next layout sync will correct.
        /// </summary>
        private bool TryResolveAudio(string hash, out byte[] pcm) {
            if (audioCache.TryGet(hash, out pcm)) {
                return true;
            }
            UVoicePart? part;
            lock (stateLock) {
                hashOwners.TryGetValue(hash, out part);
            }
            if (part == null) {
                return false;
            }
            var entry = new PartSnapshot { Project = ProjectSource(), Part = part };
            if (!TryHashPart(entry, out string actual, out var bytes) || actual != hash) {
                return false;
            }
            audioCache.Put(hash, bytes);
            pcm = bytes;
            return true;
        }

        private void OnPluginNotification(string kind, JsonElement? payload) {
            switch (kind) {
                case DawMessageKind.Ping:
                    // Liveness only — the transport already refreshed its heartbeat clock (§3).
                    break;
                case DawMessageKind.PlaybackStarted:
                    // §7: the DAW is about to play, so everything pending goes out now.
                    scheduler.FlushPending(NowUtc());
                    FireAndForget(PumpOnceAsync(), "playbackStarted flush");
                    break;
                default:
                    Log.Information($"DAW: ignoring unknown notification '{kind}'.");
                    break;
            }
        }

        /// <summary>
        /// End of connection. A close we asked for stays closed; anything else climbs the §3
        /// backoff ladder.
        /// </summary>
        private void OnTransportDisconnected(DawDisconnectReason reason, string detail) {
            StopPump();
            Unsubscribe();
            scheduler.Clear();
            if (closingLocally) {
                SetState(DawConnectionState.Disconnected);
                return;
            }
            SetState(DawConnectionState.Reconnecting);
            FireAndForget(ReconnectAsync(reason, detail), "reconnect");
        }

        /// <summary>
        /// Retries the same port on the §3 ladder — 500 ms, 1 s, 2 s. A plugin that really exited
        /// never answers, so the ladder ends and the user is told once.
        /// </summary>
        private async Task ReconnectAsync(DawDisconnectReason reason, string detail) {
            var target = server;
            if (target == null) {
                SetState(DawConnectionState.Disconnected);
                return;
            }
            for (int attempt = 0; attempt < reconnectBackoff.Length; attempt++) {
                await Task.Delay(reconnectBackoff[attempt]);
                if (closingLocally || Volatile.Read(ref disposed) != 0) {
                    SetState(DawConnectionState.Disconnected);
                    return;
                }
                try {
                    await OpenAsync(target.Port, CancellationToken.None);
                    Log.Information($"DAW: reconnected to '{target.Name}' on attempt {attempt + 1}.");
                    return;
                } catch (Exception e) {
                    Log.Warning(e, $"DAW: reconnect attempt {attempt + 1} of {reconnectBackoff.Length} failed.");
                }
            }
            SetState(DawConnectionState.Disconnected);
            ConnectionLost?.Invoke($"{reason}: {detail}");
        }

        /// <summary>
        /// User-initiated teardown: flush what is pending so the DAW is left holding the final
        /// state, then send the bare <c>close</c> (§9).
        /// </summary>
        public async Task DisconnectAsync() {
            var live = transport;
            if (live == null) {
                return;
            }
            closingLocally = true;
            StopPump();
            if (live.IsConnected) {
                scheduler.FlushPending(NowUtc());
                await PumpOnceAsync();
            }
            await TeardownAsync();
        }

        private async Task TeardownAsync() {
            await CloseTransportAsync();
            StopPump();
            Unsubscribe();
            scheduler.Clear();
            audioCache.Clear();
            lock (stateLock) {
                hashOwners.Clear();
                transport?.Dispose();
                transport = null;
                server = null;
            }
            SetState(DawConnectionState.Disconnected);
        }

        private async Task CloseTransportAsync() {
            var live = transport;
            if (live == null || !live.IsConnected) {
                return;
            }
            try {
                await live.CloseAsync();
            } catch (Exception e) {
                Log.Warning(e, "DAW: closing the transport failed.");
            }
        }

        private static void FireAndForget(Task task, string what) {
            task.ContinueWith(
                faulted => Log.Error(faulted.Exception!, $"DAW: {what} failed."),
                TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Drops the connection without a final sync. Deliberately does no blocking wait: dispose
        /// runs on application shutdown, and a final sync needs the document thread, which would
        /// deadlock if that thread is the one disposing.
        /// </summary>
        public void Dispose() {
            if (Interlocked.Exchange(ref disposed, 1) != 0) {
                return;
            }
            closingLocally = true;
            StopPump();
            Unsubscribe();
            scheduler.Clear();
            audioCache.Clear();
            lock (stateLock) {
                hashOwners.Clear();
                transport?.Dispose();
                transport = null;
                server = null;
            }
            syncGate.Dispose();
            SetState(DawConnectionState.Disconnected);
        }
    }
}
