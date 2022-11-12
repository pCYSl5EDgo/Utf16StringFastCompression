# Utf16StringFastCompression

Very Fast UTF16 compression library for .NET7

# Compression Ratio

If all input text characters are in ASCII range then output bytes length is (text.Length + 2).
If all input text characters are not in ASCII range then output bytes length is (text.Length << 1), which is the very same length of original text.
The output bytes length will never be larger than that of the original.

# Benchmark

|        Method |                 Text |           Mean |          Error |         StdDev |
|-------------- |--------------------- |---------------:|---------------:|---------------:|
| SerializeFast |                      |       5.537 ns |       1.364 ns |      0.0747 ns |
| SerializeUtf8 |                      |      17.210 ns |      79.835 ns |      4.3760 ns |
| SerializeFast | P(...)iki [174742] | 136,464.551 ns | 200,057.478 ns | 10,965.8254 ns |
| SerializeUtf8 | P(...)iki [174742] | 134,777.368 ns |   9,242.667 ns |    506.6218 ns |
| SerializeFast | very (...) text [21] |      19.168 ns |      26.126 ns |      1.4320 ns |
| SerializeUtf8 | very (...) text [21] |      20.116 ns |       1.492 ns |      0.0818 ns |
| SerializeFast |  走れメ(...)カード [10610] |   1,224.884 ns |     283.876 ns |     15.5602 ns |
| SerializeUtf8 |  走れメ(...)カード [10610] |  21,884.395 ns |   4,786.721 ns |    262.3763 ns |

|          Method |                 Text |       Mean |      Error |     StdDev |     Median |
|---------------- |--------------------- |-----------:|-----------:|-----------:|-----------:|
| DeserializeFast |                      |   1.467 μs |   2.787 μs |  0.1528 μs |   1.500 μs |
| DeserializeUtf8 |                      |   1.717 μs |   2.787 μs |  0.1528 μs |   1.750 μs |
| DeserializeFast | P(...)iki [174742] | 142.100 μs | 363.737 μs | 19.9377 μs | 132.700 μs |
| DeserializeUtf8 | P(...)iki [174742] | 144.767 μs | 396.952 μs | 21.7583 μs | 134.100 μs |
| DeserializeFast | very (...) text [21] |   1.500 μs |   1.824 μs |  0.1000 μs |   1.500 μs |
| DeserializeUtf8 | very (...) text [21] |   1.667 μs |   1.053 μs |  0.0577 μs |   1.700 μs |
| DeserializeFast |  走れメ(...)カード [10610] |  10.800 μs | 177.190 μs |  9.7124 μs |   5.700 μs |
| DeserializeUtf8 |  走れメ(...)カード [10610] |  42.033 μs | 265.597 μs | 14.5583 μs |  34.700 μs |