__**This branch is for only research purpose**__

This has very bad performance issue!
Do not reinvent this!

# Utf16StringFastCompression

Very Fast UTF16 compression library for .NET7

# Usage

```
dotnet add package Utf16StringFastCompression
```

All API is under Utf16StringFastCompression.Utf16CompressionEncoding static class.

# Compression Ratio

If all input text characters are in ASCII range then output bytes length is (text.Length + 2).
If all input text characters are not in ASCII range then output bytes length is (text.Length << 1), which is the very same length of original text.
The output bytes length will never be larger than that of the original.

# Benchmark

## Bad Performance Benchmark

|        Method |                 Text |           Mean |         Error |        StdDev |         Median |
|-------------- |--------------------- |---------------:|--------------:|--------------:|---------------:|
| SerializeFast |                      |       7.077 ns |     0.7572 ns |     1.0615 ns |       6.349 ns |
| SerializeUtf8 |                      |      13.561 ns |     0.3122 ns |     0.4673 ns |      13.464 ns |
| SerializeFast | P(...)iki [174742] | 315,645.705 ns | 1,749.0192 ns | 2,563.6896 ns | 315,768.457 ns |
| SerializeUtf8 | P(...)iki [174742] | 139,469.206 ns | 5,987.4391 ns | 8,587.0062 ns | 135,211.658 ns |
| SerializeFast | very (...) text [21] |      37.408 ns |     2.3530 ns |     3.2985 ns |      35.477 ns |
| SerializeUtf8 | very (...) text [21] |      19.367 ns |     0.2127 ns |     0.3118 ns |      19.287 ns |
| SerializeFast |  走れメ(...)カード [10610] |  18,089.731 ns |   373.1349 ns |   546.9362 ns |  17,790.106 ns |
| SerializeUtf8 |  走れメ(...)カード [10610] |  22,065.688 ns |   684.5503 ns | 1,003.4049 ns |  21,629.352 ns |

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

|          Method |                 Text |       Mean |      Error |     StdDev |     Median |
|---------------- |--------------------- |-----------:|-----------:|-----------:|-----------:|
| DeserializeFast |                      |   1.114 μs |  0.0821 μs |  0.1177 μs |   1.100 μs |
| DeserializeUtf8 |                      |   2.593 μs |  0.8732 μs |  1.3070 μs |   1.750 μs |
| DeserializeFast | P(...)iki [174742] | 155.986 μs | 18.8278 μs | 27.5975 μs | 157.600 μs |
| DeserializeUtf8 | P(...)iki [174742] | 157.247 μs | 17.2575 μs | 25.8301 μs | 158.200 μs |
| DeserializeFast | very (...) text [21] |   2.743 μs |  0.9677 μs |  1.3879 μs |   2.750 μs |
| DeserializeUtf8 | very (...) text [21] |   3.524 μs |  1.0384 μs |  1.5221 μs |   3.500 μs |
| DeserializeFast |  走れメ(...)カード [10610] |   6.092 μs |  0.3951 μs |  0.5275 μs |   5.900 μs |
| DeserializeUtf8 |  走れメ(...)カード [10610] |  33.415 μs |  0.5359 μs |  0.7513 μs |  33.300 μs |

# Format Specification

## Shorter than 4 chars

All inputs whose lengths are less than 4 chars are not serialized, they are just copied to the destination.
"abc" => [0x61, 0x00, 0x62, 0xff, 0x63, 0xff]
"あ" => [0x39, 0x30]

## Longer than 3 chars

This format has 2 modes, ASCII mode and non-ASCII mode. The default mode is non-ASCII mode.

There are byte markers where the mode transitions occur.

* From ASCII mode to non-ASCII mode: 0xff
* From non-ASCII mode to ASCII mode: 0xff, 0xff

**Note:** ASCII mode consists of more than 3 chars.