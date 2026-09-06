using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;
using Xunit;

namespace OpenUtau.Core.Api {
    /// <summary>
    /// The factory is reached from UTrack's constructor, so registration happens on whatever
    /// thread builds tracks while BuildList() publication and Get(string) lookup run concurrently.
    /// One gate must cover all three: a snapshot taken mid-registration would leave
    /// orderedFactories without the new factory (UPart then falls back to pIndex 0) and could
    /// return null from Get(string).
    /// </summary>
    public class PhonemizerFactoryTest {
        [Phonemizer("Factory test A", "ZZ TFA", language: "EN")]
        private class DummyA : DummyPhonemizer { }

        [Phonemizer("Factory test B", "ZZ TFB", language: "EN")]
        private class DummyB : DummyPhonemizer { }

        [Phonemizer("Factory test C", "ZZ TFC", language: "EN")]
        private class DummyC : DummyPhonemizer { }

        [Phonemizer("Factory test D", "ZZ TFD", language: "EN")]
        private class DummyD : DummyPhonemizer { }

        private class DummyPhonemizer : Phonemizer {
            public override void SetSinger(USinger singer) { }
            public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevs) {
                return new Result();
            }
        }

        private static readonly Type[] StressTypes = { typeof(DummyB), typeof(DummyC), typeof(DummyD) };

        [Fact]
        public void RegistrationIsVisibleToLookupAndPublication() {
            // DummyA is only touched here, so its registration state is deterministic even with
            // xunit running the stress test's class in parallel (the factory is static).
            Assert.DoesNotContain(PhonemizerFactory.GetAll(), f => f.type == typeof(DummyA));

            var factory = PhonemizerFactory.Get(typeof(DummyA));
            Assert.NotNull(factory);
            // Lookup by full name works immediately, without a BuildList in between.
            Assert.NotNull(PhonemizerFactory.Get(typeof(DummyA).FullName!));
            // The published list does not contain it until publication, and publication picks it up.
            Assert.DoesNotContain(PhonemizerFactory.GetAll(), f => f.type == typeof(DummyA));
            PhonemizerFactory.BuildList();
            Assert.Contains(PhonemizerFactory.GetAll(), f => f.type == typeof(DummyA));
        }

        [Fact]
        public void UnknownTypeReturnsNull() {
            Assert.Null(PhonemizerFactory.Get(typeof(object)));
        }

        [Fact]
        public void ConcurrentRegistrationNeverProducesAnIncompleteSnapshot() {
            // Stress the interleaving Get(Type) registration vs BuildList() publication. The one
            // hard invariant a reader can rely on mid-flight: once a factory is visible in
            // GetAll(), it is registered, so Get(string) by that name must find it. A snapshot
            // racing the write used to violate exactly this via the string-lookup null.
            var types = Enumerable.Range(0, 64)
                .Select(i => StressTypes[i % StressTypes.Length])
                .ToArray();
            var failures = new List<string>();

            var tasks = new List<Task>();
            for (int t = 0; t < 4; t++) {
                tasks.Add(Task.Run(() => {
                    foreach (var type in types) {
                        PhonemizerFactory.Get(type);
                    }
                }));
            }
            tasks.Add(Task.Run(() => {
                for (int i = 0; i < 500; i++) {
                    PhonemizerFactory.BuildList();
                    var snapshot = PhonemizerFactory.GetAll();
                    foreach (var factory in snapshot) {
                        if (PhonemizerFactory.Get(factory.type.FullName!) == null) {
                            lock (failures) {
                                failures.Add($"registered factory '{factory.tag}' not found by name");
                            }
                        }
                    }
                }
            }));
            Task.WaitAll(tasks.ToArray());

            PhonemizerFactory.BuildList();
            Assert.Empty(failures);
            foreach (var type in StressTypes) {
                Assert.NotNull(PhonemizerFactory.Get(type.FullName!));
                Assert.Contains(PhonemizerFactory.GetAll(), f => f.type == type);
            }
        }
    }
}
