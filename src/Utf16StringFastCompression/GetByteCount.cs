using System.Runtime.Intrinsics.X86;

namespace Utf16StringFastCompression;

partial class Utf16CompressionEncoding
{
    public static int GetByteCount(ReadOnlySpan<char> source) => GetByteCount(ref MemoryMarshal.GetReference(source), source.Length).ToInt32();

    public static int GetByteCount(char[] source, int sourceLength) => GetByteCount(ref MemoryMarshal.GetArrayDataReference(source), sourceLength).ToInt32();

/*
ec4mask = uint.MaxValue >>> 16;
ec4Accum += popcnt(ec4 & ec4mask);
difmask = uint.MaxValue >>> 13;
difAccum += popcnt(dif & dif)
ByteCount = (length << 1) - ec4Accum + (1 + difAccum * 3 >>> 1)

一番最初に連続ASCIIがある場合
        | ←     Lower         Upper     →
  LAST  | bits
0 0 0 0 | 1 1 1 1 0 0 0 0 0 0 0 0 1 1 1 1 _c
        | c4 = bits & (bits >>> 1) & (bits >>> 2) & (bits >>> 3)
0 0 0 0 | 1 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 _c
        | ec4 = c4 | (c4 << 1) | (c4 << 2) | (c4 << 3)
0 0 0 0 | 1 1 1 1 0 0 0 0 0 0 0 0 1 1 1 1 _c
        | tec4 = (uint)((int)(~ec4) >> 1)
1 1 1 0 | 0 0 0 1 1 1 1 1 1 1 1 0 0 0 0 1 !c
        | dif = ~(ec4 ^ tex4)
0 0 0 1 | 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 1 _c


*/

    public static nint GetByteCount(scoped ref char source, nint sourceLength)
    {
        if (sourceLength < 4)
        {
            return sourceLength << 1;
        }

        scoped ref char sourceEnd = ref Unsafe.Add(ref source, sourceLength);
        if (Vector256.IsHardwareAccelerated)
        {
            var filter = Vector256.Create((ushort)0xff80);
            nint asciiAccum = 0, changeAccum = 0;
            uint bitCarrier = 0;
            nint loopCount = (sourceLength - 1) >>> 4;
            var restLength = (int)(sourceLength - (loopCount << 4));
            // if (Bmi2.IsSupported)
            // {
            //     while (loopCount >= 3)
            //     {
            //         ulong bits = (ulong)bitCarrier | ((ulong)(Bmi2.ParallelBitExtract(Vector256.Equals(Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source)) & filter, Vector256<ushort>.Zero).AsByte().ExtractMostSignificantBits(), 0x55555555U)) << 4)
            //             | ((ulong)(Bmi2.ParallelBitExtract(Vector256.Equals(Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref Unsafe.AddByteOffset(ref source, 32))) & filter, Vector256<ushort>.Zero).AsByte().ExtractMostSignificantBits(), 0x55555555U)) << 20)
            //             | ((ulong)(Bmi2.ParallelBitExtract(Vector256.Equals(Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref Unsafe.AddByteOffset(ref source, 64))) & filter, Vector256<ushort>.Zero).AsByte().ExtractMostSignificantBits(), 0x55555555U)) << 36);
            //         bitCarrier = (uint)((bits >>> 48) & 0b1111UL);
            //         var c4 = bits & (bits >>> 1) & (bits >>> 2) & (bits >>> 3);
            //         c4  |= (c4 << 1) | (c4 << 2) | (c4 << 3);
            //         asciiAccum += BitOperations.PopCount(c4) - ((bitCarrier == 0b1111U ? 1 : 0) << 2);
            //         changeAccum += BitOperations.PopCount((c4 ^ (c4 >>> 1)) & ((1UL << 51) - 1UL));
            //         source = ref Unsafe.AddByteOffset(ref source, 96);
            //         loopCount -= 3;
            //     }
            //     bitCarrier = GetBitCarrierConversionTable()[(int)bitCarrier];
            // }
            while (--loopCount >= 0)
            {
                ulong bits = (((ulong)Vector256.Equals(Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source)) & filter, Vector256<ushort>.Zero).AsByte().ExtractMostSignificantBits()) << 8) | (ulong)bitCarrier;
                bitCarrier = ((uint)(bits >>> 32)) & 0xFFU;
                var c4 = bits & (bits >>> 2) & (bits >>> 4) & (bits >>> 6);
                c4 |= (c4 << 2) | (c4 << 4) | (c4 << 6);
                asciiAccum += (BitOperations.PopCount(c4) >>> 1) - ((bitCarrier == 0xFFU ? 1 : 0) << 2);
                changeAccum += BitOperations.PopCount((c4 ^ (c4 >>> 2)) & 0x3F_FFFFFFFFUL) >>> 1;
                source = ref Unsafe.AddByteOffset(ref source, 32);
            }
            {
                ulong bitsMask = (1UL << ((restLength + 4) << 1)) - 1UL;
                ulong bits = ((((ulong)Vector256.Equals(Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source)) & filter, Vector256<ushort>.Zero).AsByte().ExtractMostSignificantBits()) << 8) | bitCarrier) & bitsMask;
                if (bits != 0UL)
                {
                    bits = bits & (bits >>> 2) & (bits >>> 4) & (bits >>> 6);
                    bits = bits | (bits << 2) | (bits << 4) | (bits << 6);
                    asciiAccum += BitOperations.PopCount(bits) >>> 1;
                    changeAccum += BitOperations.PopCount((bits ^ (bits >>> 2)) & (bitsMask >>> 2)) >>> 1;
                }
                return (sourceLength << 1) - asciiAccum + ((changeAccum * 3 + 1) >>> 1);
            }
        }

        var status = 0U;
        nint answer = 0;
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
    private static ReadOnlySpan<byte> GetBitCarrierConversionTable() => new byte[] { 0, 0b11, 0b1100, 0b1111, 0b11_0000, 0b11_0011, 0b11_1100, 0b11_1111, 0b1100_0000, 0b1100_0011, 0b1100_1100, 0b1100_1111, 0b1111_0000, 0b1111_0011, 0b1111_1100, 0b1111_1111, };
}
