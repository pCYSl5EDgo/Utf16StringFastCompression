# Utf16StringFastCompression

Very Fast UTF16 compression library for .NET7

# Usage

```
dotnet add package 
```

# Compression Ratio

If all input text characters are in ASCII range then output bytes length is (text.Length + 2).
If all input text characters are not in ASCII range then output bytes length is (text.Length << 1), which is the very same length of original text.
The output bytes length will never be larger than that of the original.

# Benchmark

## Serialize Performance char → byte

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2251/21H2/November2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.100
  [Host]    : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  MediumRun : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

Job=MediumRun  IterationCount=15  LaunchCount=2  
WarmupCount=10  

|        Method |                 Text |           Mean |         Error |         StdDev |
|-------------- |--------------------- |---------------:|--------------:|---------------:|
| SerializeFast |                      |       5.274 ns |     0.2364 ns |      0.3390 ns |
| SerializeUtf8 |                      |      14.430 ns |     0.4815 ns |      0.7207 ns |
| SerializeFast | P(...)iki [174742] | 120,477.397 ns | 3,027.2693 ns |  4,243.8087 ns |
| SerializeUtf8 | P(...)iki [174742] | 146,119.897 ns | 7,151.2220 ns | 10,482.1685 ns |
| SerializeFast | very (...) text [21] |      19.981 ns |     0.7421 ns |      1.1108 ns |
| SerializeUtf8 | very (...) text [21] |      20.400 ns |     0.6168 ns |      0.8846 ns |
| SerializeFast |  走れメ(...)カード [10610] |   1,628.195 ns |    61.6571 ns |     92.2854 ns |
| SerializeUtf8 |  走れメ(...)カード [10610] |  22,770.599 ns |   507.5291 ns |    759.6457 ns |


## Deserialize Performance byte → char

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2251/21H2/November2021Update)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.100
  [Host]    : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  MediumRun : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

Job=MediumRun  InvocationCount=1  IterationCount=15  
LaunchCount=2  UnrollFactor=1  WarmupCount=10  

|          Method |                 Text |       Mean |      Error |     StdDev |
|---------------- |--------------------- |-----------:|-----------:|-----------:|
| DeserializeFast |                      |   1.693 μs |  0.4474 μs |  0.6417 μs |
| DeserializeUtf8 |                      |   1.928 μs |  0.5460 μs |  0.7289 μs |
| DeserializeFast | P(...)iki [174742] | 137.342 μs | 14.2179 μs | 21.2806 μs |
| DeserializeUtf8 | P(...)iki [174742] | 151.607 μs | 14.3747 μs | 21.5154 μs |
| DeserializeFast | very (...) text [21] |   2.557 μs |  0.7679 μs |  1.1494 μs |
| DeserializeUtf8 | very (...) text [21] |   1.811 μs |  0.3124 μs |  0.4379 μs |
| DeserializeFast |  走れメ(...)カード [10610] |   7.075 μs |  1.2623 μs |  1.8104 μs |
| DeserializeUtf8 |  走れメ(...)カード [10610] |  37.179 μs |  7.1154 μs | 10.2047 μs |