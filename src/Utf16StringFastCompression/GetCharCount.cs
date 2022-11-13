﻿namespace Utf16StringFastCompression;

partial class Utf16CompressionEncoding
{
    public static int GetCharCount(ReadOnlySpan<byte> source) => GetCharCount(ref MemoryMarshal.GetReference(source), source.Length).ToInt32();

    public static int GetCharCount(byte[] source, int sourceLength) => GetCharCount(ref MemoryMarshal.GetArrayDataReference(source), sourceLength).ToInt32();

    public static nint GetCharCount(scoped ref byte source, nint sourceLength)
    {
        var state = new ToCharState();
        return GetCharCountStateful(ref state, ref source, sourceLength);
    }

    public static nint GetCharCountStateful(scoped ref ToCharState state, scoped ref byte source, nint sourceLength)
    {
        if (Unsafe.IsNullRef(ref source) || sourceLength <= 0)
        {
            return 0;
        }

        nint answer = 0;
        scoped ref var sourceEnd = ref Unsafe.AddByteOffset(ref source, sourceLength);
        var isAscii = state.IsAsciiMode;
        if (state.HasRemainingByte)
        {
            state.HasRemainingByte = false;
            var value = (ushort)((source << 8) | state.RemainingByte);
            if (value == ushort.MaxValue)
            {
                isAscii = true;
            }
            else
            {
                answer += 1;
            }
            source = ref Unsafe.AddByteOffset(ref source, 1);
        }

        if (Vector256.IsHardwareAccelerated && sourceLength >= Unsafe.SizeOf<Vector256<ushort>>())
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<ushort>>());
            var filter = Vector256<byte>.AllBitsSet;
            while (Unsafe.IsAddressLessThan(ref source, ref sourceEnd))
            {
                var vector = Vector256.LoadUnsafe(ref source);
                if (isAscii)
                {
                    if (Vector256.EqualsAny(vector, filter))
                    {
                        isAscii = false;
                        var tzc = BitOperations.TrailingZeroCount(Vector256.Equals(vector, filter).ExtractMostSignificantBits());
                        answer += tzc;
                        source = ref Unsafe.AddByteOffset(ref source, tzc + 1);
                        continue;
                    }

                    answer += Unsafe.SizeOf<Vector256<ushort>>();
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<ushort>>());
                }
                else
                {
                    if (Vector256.EqualsAny(vector.AsUInt16(), filter.AsUInt16()))
                    {
                        isAscii = true;
                        var tzc = BitOperations.TrailingZeroCount(Vector256.Equals(vector.AsUInt16(), filter.AsUInt16()).ExtractMostSignificantBits());
                        answer += tzc;
                        source = ref Unsafe.AddByteOffset(ref source, (tzc + 1) << 1);
                        continue;
                    }

                    answer += Unsafe.SizeOf<Vector256<ushort>>() >> 1;
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<ushort>>());
                }
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<ushort>>());
        }

        if (Vector128.IsHardwareAccelerated && sourceLength >= Unsafe.SizeOf<Vector128<ushort>>())
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector128<ushort>>());
            var filter = Vector128<byte>.AllBitsSet;
            while (Unsafe.IsAddressLessThan(ref source, ref sourceEnd))
            {
                var vector = Vector128.LoadUnsafe(ref source);
                if (isAscii)
                {
                    if (Vector128.EqualsAny(vector, filter))
                    {
                        isAscii = false;
                        var tzc = BitOperations.TrailingZeroCount(Vector128.Equals(vector, filter).ExtractMostSignificantBits());
                        answer += tzc;
                        source = ref Unsafe.AddByteOffset(ref source, tzc + 1);
                        continue;
                    }

                    answer += Unsafe.SizeOf<Vector128<ushort>>();
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<ushort>>());
                }
                else
                {
                    if (Vector128.EqualsAny(vector.AsUInt16(), filter.AsUInt16()))
                    {
                        isAscii = true;
                        var tzc = BitOperations.TrailingZeroCount(Vector128.Equals(vector.AsUInt16(), filter.AsUInt16()).ExtractMostSignificantBits());
                        answer += tzc;
                        source = ref Unsafe.AddByteOffset(ref source, (tzc + 1 << 1));
                        continue;
                    }

                    answer += Unsafe.SizeOf<Vector128<ushort>>() >> 1;
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<ushort>>());
                }
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector128<ushort>>());
        }

        while (Unsafe.IsAddressLessThan(ref source, ref sourceEnd))
        {
            if (isAscii)
            {
                if (source == byte.MaxValue)
                {
                    isAscii = false;
                }
                else
                {
                    ++answer;
                }
                source = ref Unsafe.AddByteOffset(ref source, 1);
                continue;
            }
            
            var value = Unsafe.ReadUnaligned<ushort>(ref source);
            if (value == ushort.MaxValue)
            {
                isAscii = true;
            }
            else
            {
                ++answer;
            }

            source = ref Unsafe.AddByteOffset(ref source, 2);
            if (Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
            {
                state.HasRemainingByte = true;
                state.RemainingByte = source;
                break;
            }
            else
            {
                continue;
            }
        }

        state.IsAsciiMode = isAscii;
        return answer;
    }
}
