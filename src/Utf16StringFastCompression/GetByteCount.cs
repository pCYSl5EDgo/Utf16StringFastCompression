namespace Utf16StringFastCompression;

partial class Utf16CompressionEncoding
{
    public static int GetByteCount(ReadOnlySpan<char> source) => GetByteCount(ref MemoryMarshal.GetReference(source), source.Length).ToInt32();

    public static int GetByteCountDeterministic(ReadOnlySpan<char> source) => GetByteCountDeterministic(ref MemoryMarshal.GetReference(source), source.Length).ToInt32();

    public static int GetByteCount(char[] source, int sourceLength) => GetByteCount(ref MemoryMarshal.GetArrayDataReference(source), sourceLength).ToInt32();

    public static nint GetByteCount(scoped ref char source, nint sourceLength)
    {
        if (sourceLength < 4)
        {
            return sourceLength << 1;
        }

        nint answer = 0;
        scoped ref char sourceEnd = ref Unsafe.Add(ref source, sourceLength);
        var status = 0U;
        if (Vector256.IsHardwareAccelerated && Unsafe.ByteOffset(ref source, ref sourceEnd) >= (Unsafe.SizeOf<Vector256<byte>>() << 1))
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<byte>>() << 1);
            var isAscii = status != 0U;
            var filter = Vector256.Create<ushort>(0xff80);
            while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
            {
                var v0 = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>());
                if ((v0 & filter) != Vector256<ushort>.Zero)
                {
                    if (isAscii)
                    {
                        isAscii = false;
                        answer += 1;
                    }

                    answer += Unsafe.SizeOf<Vector256<byte>>();
                    continue;
                }

                if (!isAscii)
                {
                    answer += 2;
                }

                var v1 = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>());
                isAscii = (v1 & filter) == Vector256<ushort>.Zero;
                answer += (!isAscii ? (Unsafe.SizeOf<Vector256<byte>>() >> 1) + 1 : 0) + Unsafe.SizeOf<Vector256<byte>>();
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<byte>>() << 1);
            status = isAscii ? 4U : 0U;
        }
        if (Vector128.IsHardwareAccelerated && Unsafe.ByteOffset(ref source, ref sourceEnd) >= (Unsafe.SizeOf<Vector128<byte>>() << 1))
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector128<byte>>() << 1);
            var isAscii = status != 0U;
            var filter = Vector128.Create<ushort>(0xff80);
            while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
            {
                var v0 = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                if ((v0 & filter) != Vector128<ushort>.Zero)
                {
                    if (isAscii)
                    {
                        isAscii = false;
                        answer += 1;
                    }

                    answer += Unsafe.SizeOf<Vector128<byte>>();
                    continue;
                }

                if (!isAscii)
                {
                    answer += 2;
                }

                var v1 = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                isAscii = (v1 & filter) == Vector128<ushort>.Zero;
                answer += (!isAscii ? (Unsafe.SizeOf<Vector128<byte>>() >> 1) + 1 : 0) + Unsafe.SizeOf<Vector128<byte>>();
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector128<byte>>() << 1);
            status = isAscii ? 4U : 0U;
        }

        while (!Unsafe.AreSame(ref source, ref sourceEnd))
        {
            var value = (ushort)source;
            source = ref Unsafe.Add(ref source, 1);
            if (value >= 0x80)
            {
                answer += (status >= 4 ? 1 : 0) + 2;
                status = 0;
                continue;
            }
            if (++status < 4)
            {
                answer += 2;
                continue;
            }
            answer += status != 4 ? 1 : 0;
            status = 4;
        }

        return answer;
    }

    public static nint GetByteCountDeterministic(scoped ref char source, nint sourceLength)
    {
        if (sourceLength < 4)
        {
            return sourceLength << 1;
        }

        nint answer = 0;
        scoped ref char sourceEnd = ref Unsafe.Add(ref source, sourceLength);
        var status = 0U;
        if (Vector128.IsHardwareAccelerated && Unsafe.ByteOffset(ref source, ref sourceEnd) >= Unsafe.SizeOf<Vector128<byte>>())
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector128<byte>>());
            var filter = Vector128.Create((ushort)0xff80);
            while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
            {
                var vec = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                // 00 → unicode, 11 → ascii
                var bits = Vector128.Equals(vec & filter, Vector128<ushort>.Zero).AsByte().ExtractMostSignificantBits();
                if (bits == 0U)
                {
                    answer += ByteCountTransitFromAsciiToUnicode(ref status) + 16;
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                    continue;
                }
                if (bits == ushort.MaxValue)
                {
                    answer += ByteCountTransitFromUnicodeToAscii(ref status) + 8;
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                    continue;
                }
                if ((bits & 1U) == 0U)
                {
                    answer += ByteCountTransitFromAsciiToUnicode(ref status);
                    var shift = bits & (bits >>> 2) & (bits >>> 4) & (bits >>> 6);
                    if (shift == 0U)
                    {
                        answer += 16;
                        source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                        status = (uint)(BitOperations.LeadingZeroCount((ushort)~bits) >>> 1) - 8U;
                        continue;
                    }

                    var copySize = BitOperations.TrailingZeroCount(shift);
                    answer += copySize + 2;
                    source = ref Unsafe.AddByteOffset(ref source, copySize);
                    status = 4;
                    continue;
                }
                var asciiLength = BitOperations.TrailingZeroCount(~bits) >>> 1;
                if (asciiLength + status < 4)
                {
                    status = 0;
                    var copySize = (asciiLength + 1) << 1;
                    answer += copySize;
                    source = ref Unsafe.AddByteOffset(ref source, copySize);
                }
                else
                {
                    answer += ByteCountTransitFromUnicodeToAscii(ref status) + asciiLength + 1;
                    source = ref Unsafe.Add(ref source, asciiLength);
                    status = 0;
                }
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector128<byte>>());
        }
        while (!Unsafe.AreSame(ref source, ref sourceEnd))
        {
            var value = (ushort)source;
            source = ref Unsafe.Add(ref source, 1);
            if (value >= 0x80)
            {
                answer += (status >= 4 ? 1 : 0) + 2;
                status = 0;
                continue;
            }
            if (++status < 4)
            {
                answer += 2;
                continue;
            }
            answer += status != 4 ? 1 : 0;
            status = 4;
        }

        return answer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint ByteCountTransitFromAsciiToUnicode(scoped ref uint status)
    {
        var temp = status;
        status = 0;
        return temp >= 4 ? 1 : 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint ByteCountTransitFromUnicodeToAscii(scoped ref uint status)
    {
        var temp = status;
        status = 4;
        return temp < 4 ? (2 - (int)temp) : 0;
    }
}
