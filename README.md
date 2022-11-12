# 

# Benchmark

|        Method |                 Text |           Mean |          Error |         StdDev |
|-------------- |--------------------- |---------------:|---------------:|---------------:|
| SerializeFast |                      |       5.399 ns |       2.657 ns |      0.1456 ns |
| SerializeUtf8 |                      |      13.821 ns |       7.019 ns |      0.3847 ns |
| SerializeFast | 
P(...)iki [174742] | 130,635.087 ns | 380,866.606 ns | 20,876.5839 ns |
| SerializeUtf8 | 
P(...)iki [174742] | 138,089.347 ns |  77,567.207 ns |  4,251.7204 ns |
| SerializeFast | very (...) text [21] |      20.228 ns |      33.854 ns |      1.8557 ns |
| SerializeUtf8 | very (...) text [21] |      20.696 ns |      20.815 ns |      1.1409 ns |
| SerializeFast |  走れメ(...)カード [10610] |   1,223.198 ns |     555.364 ns |     30.4414 ns |
| SerializeUtf8 |  走れメ(...)カード [10610] |  27,204.624 ns |  55,026.016 ns |  3,016.1616 ns |

|          Method |                 Text |       Mean |        Error |      StdDev |     Median |
|---------------- |--------------------- |-----------:|-------------:|------------:|-----------:|
| DeserializeFast |                      |   1.167 μs |     2.787 μs |   0.1528 μs |   1.200 μs |
| DeserializeUtf8 |                      |   1.733 μs |     4.213 μs |   0.2309 μs |   1.600 μs |
| DeserializeFast | 
P(...)iki [174742] | 320.000 μs | 2,216.615 μs | 121.5001 μs | 254.400 μs |
| DeserializeUtf8 | 
P(...)iki [174742] | 134.467 μs |   359.343 μs |  19.6968 μs | 123.700 μs |
| DeserializeFast | very (...) text [21] |   1.233 μs |     2.787 μs |   0.1528 μs |   1.200 μs |
| DeserializeUtf8 | very (...) text [21] |   1.867 μs |     2.107 μs |   0.1155 μs |   1.800 μs |
| DeserializeFast |  走れメ(...)カード [10610] |  24.133 μs |   195.560 μs |  10.7193 μs |  18.400 μs |
| DeserializeUtf8 |  走れメ(...)カード [10610] |  41.567 μs |   247.607 μs |  13.5722 μs |  34.700 μs |