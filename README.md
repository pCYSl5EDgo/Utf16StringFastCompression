# Utf16StringFastCompression

Very Fast UTF16 compression library for .NET7

# Usage

```
dotnet add package Utf16StringFastCompression
```

All API is under Utf16StringFastCompression.Utf16CompressionEncoding static class.

## Full API List

### From char to byte

```csharp
int GetBytes(ReadOnlySpan<char> source, Span<byte> destination)
int GetBytes(char[] source, int sourceLength, byte[] destination)
nint GetBytes(scoped ref char source, nint sourceLength, scoped ref byte destination)

int GetByteCount(ReadOnlySpan<char> source)
int GetByteCount(char[] source, int sourceLength)
nint GetByteCount(scoped ref char source, nint sourceLength)

T GetMaxByteCount<T>(T charCount) where T : IShiftOperators<T, int, T> => charCount << 1;
```

### From byte to char

```csharp
int GetChars(ReadOnlySpan<byte> source, Span<char> destination)
int GetChars(byte[] source, int sourceLength, char[] destination)
nint GetChars(scoped ref byte source, nint sourceLength, scoped ref char destination)
nint GetCharsStateful(scoped ref byte source, nint sourceLength, scoped ref char destination, scoped ref ToCharState state)

int GetCharCount(ReadOnlySpan<byte> source)
int GetCharCount(byte[] source, int sourceLength)
long GetCharCount(in ReadOnlySequence<byte> source)
nint GetCharCount(scoped ref byte source, nint sourceLength)
nint GetCharCountStateful(scoped ref byte source, nint sourceLength, scoped ref ToCharState state)

string GetString(ReadOnlySpan<byte> source)
string GetString(ref byte source, nint sourceLength)

T GetMaxCharCount<T>(T byteCount) => byteCount;
```

If you want to process byte stream, you can use GetChar(Count|s)Stateful apis.

```csharp
public struct ToCharState
{
    public bool IsAsciiMode;
    public bool HasRemainingByte;
    public byte RemainingByte;
}
```

## Detect input text kind

```csharp
TextKind DetectTextKind(ReadOnlySpan<char> source)
TextKind DetectTextKind(char[] source, int sourceLength)
TextKind DetectTextKind(ref char source, nint sourceLength)

public enum TextKind
{
    Mixed,
    AllNotInAsciiRange,
    AllInAsciiRange,
}
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

|          Method |                 Text |       Mean |      Error |     StdDev |     Median |
|---------------- |--------------------- |-----------:|-----------:|-----------:|-----------:|
| SerializeFast | |       5.912 ns |     0.0850 ns |     0.1191 ns |       5.885 ns |
| SerializeUtf8 | |      13.139 ns |     0.1501 ns |     0.2153 ns |      13.064 ns |
| SerializeFast | P(...)iki [174742] | 143,324.921 ns | 4,975.5960 ns | 6,810.6519 ns | 139,827.893 ns |
| SerializeUtf8 | P(...)iki [174742] | 133,992.946 ns |   772.3141 ns | 1,107.6298 ns | 133,662.439 ns |
| SerializeFast | very (...) text [21] |      19.239 ns |     0.7261 ns |     1.0868 ns |      19.482 ns |
| SerializeUtf8 | very (...) text [21] |      19.612 ns |     0.4811 ns |     0.6256 ns |      19.253 ns |
| SerializeFast |  走れメ(...)カード [10610] |   2,837.155 ns |    57.6724 ns |    76.9910 ns |   2,803.810 ns |
| SerializeUtf8 |  走れメ(...)カード [10610] |  21,684.962 ns |   247.6680 ns |   330.6296 ns |  21,599.664 ns |

## ByteCount Performance

|          Method |                 Text |       Mean |      Error |     StdDev |     Median |
|---------------- |--------------------- |-----------:|-----------:|-----------:|-----------:|
| ByteCountFast | |       1.898 ns |     0.0257 ns |     0.0368 ns |       1.881 ns |
| ByteCountUtf8 | |       6.887 ns |     0.0798 ns |     0.1169 ns |       6.847 ns |
| ByteCountFast | P(...)iki [174742] |  24,466.736 ns |   338.5406 ns |   463.3982 ns |  24,240.948 ns |
| ByteCountUtf8 | P(...)iki [174742] |  28,463.019 ns |   361.9417 ns |   507.3917 ns |  28,173.419 ns |
| ByteCountFast | very (...) text [21] |      10.327 ns |     0.1098 ns |     0.1540 ns |      10.275 ns |
| ByteCountUtf8 | very (...) text [21] |       9.887 ns |     0.0260 ns |     0.0365 ns |       9.886 ns |
| ByteCountFast |  走れメ(...)カード [10610] |   1,330.227 ns |    16.6922 ns |    24.9841 ns |   1,336.935 ns |
| ByteCountUtf8 |  走れメ(...)カード [10610] |   1,394.888 ns |    24.4221 ns |    35.7976 ns |   1,388.348 ns |

## Deserialize Performance byte → char

|          Method |                 Text |       Mean |      Error |     StdDev |     Median |
|---------------- |--------------------- |-----------:|-----------:|-----------:|-----------:|
| DeserializeFast | |   1.114 μs |  0.0821 μs |  0.1177 μs |   1.100 μs |
| DeserializeUtf8 | |   2.593 μs |  0.8732 μs |  1.3070 μs |   1.750 μs |
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