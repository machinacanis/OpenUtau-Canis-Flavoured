using Xunit;

// Test collections run serially: the test suite shares process-wide mutable state (the G2P
// prediction cache, SyllableBasedPhonemizer yaml state), and parallel collections raced on it,
// which intermittently garbled phonemizer output (e.g. EnToJa expectations). Serial execution
// keeps the suite deterministic until that shared state is thread-safe.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
