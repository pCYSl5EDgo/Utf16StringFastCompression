namespace Utf16StringFastCompression;

public static partial class Utf16CompressionEncoding
{
    public static T GetMaxByteCount<T>(T charCount) where T : IShiftOperators<T, int, T> => charCount << 1;

    public static T GetMaxCharCount<T>(T byteCount) => byteCount;
}
