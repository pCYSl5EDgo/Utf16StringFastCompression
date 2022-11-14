using BenchmarkDotNet.Running;

BenchmarkRunner.Run<SerializeTest>();
BenchmarkRunner.Run<DeserializeTest>();
// BenchmarkRunner.Run<PextTest>();