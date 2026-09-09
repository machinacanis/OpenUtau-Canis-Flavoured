using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Classic;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;

namespace OpenUtau.Core.Render {
    public static class Renderers {
        public const string CLASSIC = "CLASSIC";
        public const string WORLDLINE_R = "WORLDLINE-R";
        public const string WORLDLINE_R2 = "WORLDLINE-R2";
        public const string ENUNU = "ENUNU";
        public const string VOGEN = "VOGEN";
        public const string DIFFSINGER = "DIFFSINGER";
        public const string VOICEVOX = "VOICEVOX";
        public const string HIFIUTAU = "HIFIUTAU";
        public const string CUSTOM_SERVER = "CUSTOM_SERVER";

        static readonly string[] classicRenderers = new[] { WORLDLINE_R, CLASSIC, HIFIUTAU, CUSTOM_SERVER };
        static readonly string[] enunuRenderers = new[] { ENUNU };
        static readonly string[] vogenRenderers = new[] { VOGEN };
        static readonly string[] diffSingerRenderers = new[] { DIFFSINGER };
        static readonly string[] voicevoxRenderers = new[] { VOICEVOX };
        static readonly string[] noRenderers = new string[0];

        public static string[] GetSupportedRenderers(USingerType singerType) {
            switch (singerType) {
                case USingerType.Classic:
                    return classicRenderers;
                case USingerType.Enunu:
                    return enunuRenderers;
                case USingerType.Vogen:
                    return vogenRenderers;
                case USingerType.DiffSinger:
                    return diffSingerRenderers;
                case USingerType.Voicevox:
                    return voicevoxRenderers;
                default:
                    return noRenderers;
            }
        }

        public static List<string> getRendererOptions() {
            return new List<string> {
                "WORLDLINE-R",
                "Classic",
                "HiFiUTAU",
                "Custom Server"
            };
        }

        public static string GetDefaultRenderer(USingerType singerType) {
            if (singerType == USingerType.Classic) {
                switch (Preferences.Default.DefaultRenderer) {
                    case "Classic":
                        return CLASSIC;
                    case "HiFiUTAU":
                    case "HiFiUTAU Local":
                    case "HiFiUTAU Online":
                        return HIFIUTAU;
                    case "Custom Server":
                        return CUSTOM_SERVER;
                }
            }
            return GetSupportedRenderers(singerType)[0];
        }

        public static IRenderer CreateRenderer(string renderer) {
            if (renderer == CLASSIC) {
                return new ClassicRenderer();
            } else if (renderer == WORLDLINE_R2) {
                return new WorldlineRenderer(version: 2);
            } else if (renderer?.StartsWith(WORLDLINE_R.Substring(0, 9)) ?? false) {
                return new WorldlineRenderer(version: 1);
            } else if (renderer == ENUNU) {
                return new Enunu.EnunuRenderer();
            } else if (renderer == VOGEN) {
                return new Vogen.VogenRenderer();
            } else if (renderer == DIFFSINGER) {
                return new DiffSinger.DiffSingerRenderer();
            } else if (renderer == VOICEVOX) {
                return new Voicevox.VoicevoxRenderer();
            } else if (renderer == HIFIUTAU || renderer == "HIFIUTAU_LOCAL" || renderer == "HIFIUTAU_ONLINE") {
                return new HiFiUtau.HifiUtauRenderer();
            } else if (renderer == CUSTOM_SERVER) {
                return new CustomRender.CustomServerRenderer();
            }
            return null;
        }

        // One instance per renderer id. Renderers are stateless or globally
        // serialized (the static lockObj fields), so sharing an instance across
        // tracks is behaviourally identical to today while an undo/redo that
        // toggles the renderer no longer re-creates the pipeline object.
        static readonly ConcurrentDictionary<string, IRenderer> rendererCache =
            new ConcurrentDictionary<string, IRenderer>();

        public static IRenderer GetOrCreate(string renderer) {
            // CUSTOM_SERVER is the exception: its ServerUrl / Endpoint are
            // per-track settings mutated by URenderSettings.Validate and read
            // back by the track settings dialog, so tracks must not share one
            // instance (fork).
            if (renderer == CUSTOM_SERVER) {
                return CreateRenderer(renderer);
            }
            return rendererCache.GetOrAdd(renderer ?? string.Empty, CreateRenderer);
        }

        readonly static ConcurrentDictionary<string, object> cacheLockMap
            = new ConcurrentDictionary<string, object>();

        public static object GetCacheLock(string key) {
            return cacheLockMap.GetOrAdd(key, _ => new object());
        }

        public static void ApplyDynamics(RenderPhrase phrase, RenderResult result) {
            const int interval = 5;
            if (phrase.dynamics == null) {
                return;
            }
            int startTick = phrase.position - phrase.leading;
            double startMs = result.positionMs - result.leadingMs;
            int startSample = 0;
            for (int i = 0; i < phrase.dynamics.Length; ++i) {
                int endTick = startTick + interval;
                double endMs = phrase.timeAxis.TickPosToMsPos(endTick);
                int endSample = Math.Min((int)((endMs - startMs) / 1000 * 44100), result.samples.Length);
                float a = phrase.dynamics[i];
                float b = (i + 1) == phrase.dynamics.Length ? phrase.dynamics[i] : phrase.dynamics[i + 1];
                for (int j = startSample; j < endSample; ++j) {
                    result.samples[j] *= a + (b - a) * (j - startSample) / (endSample - startSample);
                }
                startTick = endTick;
                startSample = endSample;
            }
        }

        public static IReadOnlyList<IResampler> GetSupportedResamplers(IWavtool? wavtool) {
            if (wavtool is SharpWavtool) {
                return ToolsManager.Inst.Resamplers;
            } else {
                return ToolsManager.Inst.Resamplers
                    .Where(r => !(r is WorldlineResampler))
                    .ToArray();
            }
        }

        public static IReadOnlyList<IWavtool> GetSupportedWavtools(IResampler? resampler) {
            return ToolsManager.Inst.Wavtools;
        }
    }
}
