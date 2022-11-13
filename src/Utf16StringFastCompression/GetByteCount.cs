namespace Utf16StringFastCompression;

partial class Utf16CompressionEncoding
{
    public static int GetByteCount(ReadOnlySpan<char> source) => GetByteCount(ref MemoryMarshal.GetReference(source), source.Length).ToInt32();

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

        while (Unsafe.IsAddressLessThan(ref source, ref sourceEnd))
        {
            var value = (ushort)source;
            source = ref Unsafe.Add(ref source, 1);
            if (value < 0x80)
            {
                if (status++ == 0U)
                {
                    if (Unsafe.AreSame(ref source, ref sourceEnd))
                    {
                        break;
                    }

                    answer += 2;
                }

                if (status == 0U)
                {
                    status = 4U;
                }

                answer += 1;
                continue;
            }

            switch (status)
            {
                case 0U: break;
                case 1U:
                    answer -= 1;
                    goto RESET_STATUS;
                case 2U:
                    goto RESET_STATUS;
                default:
                    answer += 1;
                RESET_STATUS:
                    status = 0;
                    break;
            }

            answer += 2;
        }

        return answer;
    }
}
