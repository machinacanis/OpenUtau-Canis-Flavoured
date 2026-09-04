using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using OpenUtau.Core.DawIntegration;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace OpenUtau.App.ViewModels {
    /// <summary>One discovered plugin, as the connection list shows it.</summary>
    public class DawServerViewModel {
        public DawServer Server { get; }
        public string Name => Server.Name;
        public int Port => Server.Port;
        public string ApiVersion => Server.Info.ApiVersion;

        /// <summary>
        /// Whether this entry can be connected to. Incompatible plugins are still listed, so the
        /// list can explain why one is refused instead of silently hiding it (PROTOCOL.md §4).
        /// </summary>
        public string Compatibility => Server.IsCompatible
            ? ThemeManager.GetString("dawintegration.compatible")
            : ThemeManager.GetString("dawintegration.incompatible");

        public DawServerViewModel(DawServer server) {
            Server = server;
        }
    }

    /// <summary>
    /// The connection entry point: what the discovery directory currently advertises, plus the
    /// state of the single connection <see cref="DawManager"/> owns. The manager outlives this
    /// dialog, so closing the window does not drop the connection.
    /// </summary>
    public class DawIntegrationViewModel : ViewModelBase, IDisposable {
        private readonly DawServerFinder finder = new DawServerFinder(DawServerFinder.DefaultDirectory);
        private bool disposed;

        public ObservableCollection<DawServerViewModel> Servers { get; }
            = new ObservableCollection<DawServerViewModel>();

        [Reactive] public DawServerViewModel? SelectedServer { get; set; }
        [Reactive] public string Status { get; set; } = string.Empty;
        [Reactive] public bool IsConnected { get; set; }
        [Reactive] public bool IsBusy { get; set; }

        public DawIntegrationViewModel() {
            DawManager.Inst.StateChanged += OnStateChanged;
            DawManager.Inst.ConnectionLost += OnConnectionLost;
            ShowState(DawManager.Inst.State);
        }

        public async Task RefreshAsync() {
            // Scan probes every advertised port, so it does blocking socket work even though the
            // directory itself is tiny.
            var found = await Task.Run(() => finder.Scan());
            int selectedPort = SelectedServer?.Port ?? 0;
            Servers.Clear();
            foreach (var server in found) {
                Servers.Add(new DawServerViewModel(server));
            }
            SelectedServer = Servers.FirstOrDefault(item => item.Port == selectedPort)
                ?? Servers.FirstOrDefault(item => item.Server.IsCompatible);
            if (Servers.Count == 0 && !IsConnected) {
                Status = ThemeManager.GetString("dawintegration.none");
            }
        }

        public async Task ConnectAsync() {
            var target = SelectedServer;
            if (target == null || IsBusy) {
                return;
            }
            if (!target.Server.IsCompatible) {
                Status = ThemeManager.GetString("dawintegration.incompatible");
                return;
            }
            IsBusy = true;
            try {
                await DawManager.Inst.ConnectAsync(target.Server);
            } finally {
                IsBusy = false;
            }
        }

        public async Task DisconnectAsync() {
            if (IsBusy) {
                return;
            }
            IsBusy = true;
            try {
                await DawManager.Inst.DisconnectAsync();
            } finally {
                IsBusy = false;
            }
        }

        /// <summary>
        /// <see cref="DawManager.StateChanged"/> fires from the transport read loop and from the
        /// reconnect task, so it has to be marshalled before it touches a binding.
        /// </summary>
        private void OnStateChanged(DawConnectionState state) {
            Dispatcher.UIThread.Post(() => ShowState(state));
        }

        private void OnConnectionLost(string reason) {
            Log.Warning($"DAW: connection lost: {reason}");
            Dispatcher.UIThread.Post(
                () => Status = string.Format(ThemeManager.GetString("dawintegration.lost"), reason));
        }

        private void ShowState(DawConnectionState state) {
            IsConnected = state == DawConnectionState.Connected;
            string name = DawManager.Inst.ServerName;
            Status = state switch {
                DawConnectionState.Connected =>
                    string.Format(ThemeManager.GetString("dawintegration.connected"), name),
                DawConnectionState.Connecting => ThemeManager.GetString("dawintegration.connecting"),
                DawConnectionState.Reconnecting =>
                    string.Format(ThemeManager.GetString("dawintegration.reconnecting"), name),
                _ => ThemeManager.GetString("dawintegration.disconnected"),
            };
        }

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;
            // The manager outlives the dialog, so a leaked handler would keep this view model alive.
            DawManager.Inst.StateChanged -= OnStateChanged;
            DawManager.Inst.ConnectionLost -= OnConnectionLost;
        }
    }
}
