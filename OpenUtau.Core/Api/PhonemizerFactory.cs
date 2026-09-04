using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenUtau.Api {
    public class PhonemizerFactory {
        public Type type;
        public string name;
        public string tag;
        public string author;
        public string language;

        public Phonemizer Create() {
            var phonemizer = Activator.CreateInstance(type) as Phonemizer;
            phonemizer.Name = name;
            phonemizer.Tag = tag;
            phonemizer.Language = language;
            return phonemizer;
        }

        public override string ToString() => string.IsNullOrEmpty(author)
            ? $"[{tag}] {name}"
            : $"[{tag}] {name} (Contributed by {author})";

        // Reached from UTrack's constructor, so it is hit from whatever thread happens to be
        // building tracks - project load and rendering both do this off the UI thread. One gate
        // covers registration, name lookup and list publication: a ConcurrentDictionary alone
        // keeps the map intact but still lets BuildList() snapshot before an in-flight Get(Type)
        // write lands (orderedFactories then misses the factory and UPart falls back to
        // pIndex 0), and lets Get(string) observe the map mid-registration.
        private static readonly object registryGate = new object();
        private static readonly Dictionary<Type, PhonemizerFactory> factories = new Dictionary<Type, PhonemizerFactory>();
        private static PhonemizerFactory[] orderedFactories = [];
        public static PhonemizerFactory Get(Type type) {
            lock (registryGate) {
                if (!factories.TryGetValue(type, out var factory)) {
                    var attr = type.GetCustomAttribute<PhonemizerAttribute>();
                    if (attr == null || string.IsNullOrEmpty(attr.Name) || string.IsNullOrEmpty(attr.Tag)) {
                        return null;
                    }
                    factory = new PhonemizerFactory() {
                        type = type,
                        name = attr.Name,
                        tag = attr.Tag,
                        author = attr.Author,
                        language = attr.Language,
                    };
                    factories[type] = factory;
                }
                return factory;
            }
        }

        public static PhonemizerFactory? Get(string typeFullName) {
            lock (registryGate) {
                foreach (var factory in factories.Values) {
                    if (factory.type.FullName == typeFullName) {
                        return factory;
                    }
                }
            }
            return null;
        }

        public static void BuildList() {
            lock (registryGate) {
                orderedFactories = factories.Values.OrderBy(f => f.tag).ToArray();
            }
        }

        public static PhonemizerFactory[] GetAll() => orderedFactories;
    }
}
