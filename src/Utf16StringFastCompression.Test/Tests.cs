using System.Runtime.InteropServices;

namespace Utf16StringFastCompression.Test;

public class Tests
{
    [Theory]
    [InlineData("abcdefgh")]
    [InlineData("abcdefghi")]
    [InlineData("abcdefghij")]
    [InlineData("abcdefghijk")]
    [InlineData("abcdefghijkl")]
    [InlineData("abcdefghijklm")]
    [InlineData("abcdefghijklmn")]
    [InlineData("abcdefghijklmno")]
    public void AllAsciiTest(string value)
    {
        Assert.True(value.Length >= 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length + 2, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("あいうえおかきく")]
    [InlineData("あいうえおかきくけ")]
    [InlineData("あいうえおかきくけこ")]
    public void AllUtf16Test(string value)
    {
        Assert.True(value.Length >= 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length << 1, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("あいうabcdefghijklmえおかきく")]
    [InlineData("あいうabcdefghijklmnえおかきくけ")]
    [InlineData("あいうabcdefghijklmnoえおかきくけこ")]
    [InlineData("ほるもんはdazydazyほうるもamazingんあいうabcdefghijklmえおかきく")]
    [InlineData("▓▓tinkle tinkle！！1！14→514↑1919810▓▓▓▓▓▓")]
    public void MixTest(string value)
    {
        Assert.True(value.Length >= 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.InRange(byteCount, 0, value.Length << 1);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("abcde")]
    [InlineData("abcdef")]
    [InlineData("abcdefg")]
    public void LessThan8AllAsciiTest(string value)
    {
        Assert.InRange(value.Length, 0, 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length + 2, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("あいうえ")]
    [InlineData("あいうえお")]
    [InlineData("あいうえおか")]
    [InlineData("あいうえおかき")]
    public void LessThan8NotAsciiTest(string value)
    {
        Assert.True(value.Length < 8);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length << 1, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("あ")]
    [InlineData("あい")]
    [InlineData("あいう")]
    public void LessThan4Test(string value)
    {
        Assert.True(value.Length < 4);
        Span<byte> bytes = stackalloc byte[Utf16CompressionEncoding.GetMaxByteCount(value.Length)];
        var byteCount = (int)Utf16CompressionEncoding.GetBytes(ref MemoryMarshal.GetReference(value.AsSpan()), value.Length, ref MemoryMarshal.GetReference(bytes));
        Assert.Equal(value.Length << 1, byteCount);
        bytes = bytes[..byteCount];
        Span<char> chars = stackalloc char[Utf16CompressionEncoding.GetMaxCharCount(byteCount)];
        var charCount = (int)Utf16CompressionEncoding.GetChars(ref MemoryMarshal.GetReference(bytes), byteCount, ref MemoryMarshal.GetReference(chars));
        Assert.Equal(value, new string(chars[..charCount]));
    }
}