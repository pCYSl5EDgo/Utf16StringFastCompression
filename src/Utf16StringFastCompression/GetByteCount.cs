namespace Utf16StringFastCompression;

partial class Utf16CompressionEncoding
{
    public static nint GetByteCount(scoped ref char source, nint sourceLength)
    {
        if (sourceLength < 4)
        {
            return sourceLength << 1;
        }

        nint answer = 0;
        scoped ref char sourceEnd = ref Unsafe.Add(ref source, sourceLength);
        var status = 0U;
        if (Vector256.IsHardwareAccelerated && sourceLength >= Unsafe.SizeOf<Vector256<byte>>())
        {
            var isAscii = status != 0U;
            var filter = Vector256.Create<ushort>(0xff80);
            while (sourceLength >>> 5 != 0)
            {
                var v0 = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                var v1 = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source), (uint)Unsafe.SizeOf<Vector256<byte>>() >>> 1);
                source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>() << 1);
                sourceLength -= Unsafe.SizeOf<Vector256<byte>>();
                if (Vector256.EqualsAll((v0 | v1) & filter, Vector256<ushort>.Zero))
                {
                    if (!isAscii)
                    {
                        isAscii = true;
                        answer += 2;
                    }
                    answer += Unsafe.SizeOf<Vector256<byte>>();
                }
                else
                {
                    if (isAscii)
                    {
                        isAscii = false;
                        answer += 1;
                    }

                    answer += Unsafe.SizeOf<Vector256<byte>>() << 1;
                }
            }
            status = isAscii ? 4U : 0U;
        }
        if (Vector128.IsHardwareAccelerated && sourceLength >= Unsafe.SizeOf<Vector128<byte>>())
        {
            var isAscii = status != 0U;
            var filter = Vector128.Create<ushort>(0xff80);
            while (sourceLength >>> 4 != 0)
            {
                var v0 = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                var v1 = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source), (uint)Unsafe.SizeOf<Vector128<byte>>() >>> 1);
                source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>() << 1);
                sourceLength -= Unsafe.SizeOf<Vector128<byte>>();
                if (Vector128.EqualsAll((v0 | v1) & filter, Vector128<ushort>.Zero))
                {
                    if (!isAscii)
                    {
                        isAscii = true;
                        answer += 2;
                    }
                    answer += Unsafe.SizeOf<Vector128<byte>>();
                }
                else
                {
                    if (isAscii)
                    {
                        isAscii = false;
                        answer += 1;
                    }

                    answer += Unsafe.SizeOf<Vector128<byte>>() << 1;
                }
            }
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
