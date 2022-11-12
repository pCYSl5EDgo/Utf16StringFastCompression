using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Utf16StringFastCompression;

public static class Utf16CompressionEncoding
{
    public static T GetMaxByteCount<T>(T charCount) where T : IShiftOperators<T, int, T> => charCount << 1;

    public static T GetMaxCharCount<T>(T byteCount) => byteCount;

    public static nint GetBytes(scoped ref char source, nint sourceLength, scoped ref byte destination)
    {
        unchecked
        {
            if (sourceLength < 4)
            {
                sourceLength <<= 1;
                if (sourceLength <= 0)
                {
                    return 0;
                }

                Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.As<char, byte>(ref source), unchecked((uint)sourceLength));
                return sourceLength;
            }

            scoped ref byte initialDestination = ref destination;
            scoped ref char sourceEnd = ref Unsafe.Add(ref source, sourceLength);
            var status = uint.MaxValue;
            if (sourceLength >= 8 && Vector128.IsHardwareAccelerated)
            {
                var filter = Vector128.Create<ushort>(0x80);
                var shuffle = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(Shuffle()), 32);
                var isAsciiMode = false;
                while (sourceLength >>> 3 != 0)
                {
                    var vector = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                    var msbs = Vector128.ExtractMostSignificantBits(Vector128.GreaterThanOrEqual(vector, filter));
                    if (msbs == 0)
                    {
                        if (!isAsciiMode)
                        {
                            isAsciiMode = true;
                            Unsafe.WriteUnaligned(ref destination, ushort.MaxValue);
                            destination = ref Unsafe.AddByteOffset(ref destination, 2);
                        }

                        Unsafe.WriteUnaligned(ref destination, Vector128.Shuffle(vector.AsByte(), shuffle).AsUInt64().GetElement(0));
                        destination = ref Unsafe.AddByteOffset(ref destination, 8);
                        source = ref Unsafe.AddByteOffset(ref source, 16);
                        sourceLength -= 8;
                    }
                    else if ((ushort)(msbs + 1) == 0)
                    {
                        if (isAsciiMode)
                        {
                            isAsciiMode = false;
                            destination = byte.MaxValue;
                            destination = ref Unsafe.AddByteOffset(ref destination, 1);
                        }

                        vector.AsByte().StoreUnsafe(ref destination);
                        destination = ref Unsafe.AddByteOffset(ref destination, 16);
                        source = ref Unsafe.AddByteOffset(ref source, 16);
                        sourceLength -= 8;
                    }
                    else if (isAsciiMode)
                    {
                        isAsciiMode = false;
                        var trailingZeroCount = BitOperations.TrailingZeroCount(msbs) >>> 1;
                        if (trailingZeroCount != 0)
                        {
                            source = ref Unsafe.Add(ref source, trailingZeroCount);
                            sourceLength -= trailingZeroCount;
                            int index = 0;
                            do
                            {
                                destination = (byte)vector.GetElement(index++);
                                destination = ref Unsafe.AddByteOffset(ref destination, 1);
                            } while (--trailingZeroCount != 0);
                        }

                        destination = byte.MaxValue;
                        destination = ref Unsafe.AddByteOffset(ref destination, 1);
                    }
                    else
                    {
                        var tzc = BitOperations.TrailingZeroCount(msbs);
                        var end = tzc >>> 1;
                        if (end >= 4)
                        {
                            Unsafe.WriteUnaligned(ref destination, ushort.MaxValue);
                            destination = ref Unsafe.AddByteOffset(ref destination, 2);
                            for (int i = 0; i < end; ++i)
                            {
                                destination = (byte)vector.GetElement(i);
                                destination = ref Unsafe.AddByteOffset(ref destination, 1);
                            }
                            destination = byte.MaxValue;
                            destination = ref Unsafe.AddByteOffset(ref destination, 1);
                            Unsafe.WriteUnaligned(ref destination, vector.GetElement(end));
                            source = ref Unsafe.Add(ref source, ++end);
                            sourceLength -= end;
                        }
                        else
                        {
                            ++end;
                            var byteCount = end << 1;
                            Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.As<char, byte>(ref source), (uint)byteCount);
                            destination = ref Unsafe.AddByteOffset(ref destination, byteCount);
                            source = ref Unsafe.Add(ref source, end);
                            sourceLength -= end;
                        }
                    }
                }

                if (isAsciiMode)
                {
                    status = 3;
                }
            }

            for (; Unsafe.IsAddressLessThan(ref source, ref sourceEnd); source = ref Unsafe.Add(ref source, 1))
            {
                if (source < 0x80)
                {
                    if (++status == 0U)
                    {
                        Unsafe.WriteUnaligned(ref destination, (ushort)0xffff);
                        destination = ref Unsafe.AddByteOffset(ref destination, 2);
                    }

                    if ((int)status < 0)
                    {
                        status = 3U;
                    }

                    destination = unchecked((byte)(ushort)source);
                    destination = ref Unsafe.AddByteOffset(ref destination, 1);
                    continue;
                }

                switch (status + 1U)
                {
                    case 0U: break;
                    case 1U:
                        // 0xff, 0xff, _st0, dest
                        // _st0, 0x00, dest
                        destination = ref Unsafe.SubtractByteOffset(ref destination, 1); // position of _st0
                        Unsafe.WriteUnaligned(ref Unsafe.SubtractByteOffset(ref destination, 2), (ushort)destination);
                        goto RESET_STATUS;
                    case 2U:
                        // 0xff, 0xff, _st0, _st1, dest
                        // _st0, 0x00, _st1, 0x00, dest
                        Unsafe.WriteUnaligned(ref Unsafe.SubtractByteOffset(ref destination, 4), (ushort)Unsafe.SubtractByteOffset(ref destination, 2));
                        Unsafe.WriteUnaligned(ref Unsafe.SubtractByteOffset(ref destination, 2), (ushort)Unsafe.SubtractByteOffset(ref destination, 1));
                        goto RESET_STATUS;
                    case 3U:
                        // 0xff, 0xff, _st0, _st1, _st2, dest
                        // _st0, 0x00, _st1, 0x00, _st2, 0x00, dest
                        destination = ref Unsafe.AddByteOffset(ref destination, 1);
                        ref var sub4 = ref Unsafe.SubtractByteOffset(ref destination, 4);
                        Unsafe.WriteUnaligned(ref Unsafe.SubtractByteOffset(ref sub4, 2), (ushort)sub4);
                        Unsafe.WriteUnaligned(ref sub4, (ushort)Unsafe.AddByteOffset(ref sub4, 1));
                        ref var sub2 = ref Unsafe.AddByteOffset(ref destination, 2);
                        Unsafe.WriteUnaligned(ref sub2, (ushort)sub2);
                        goto RESET_STATUS;
                    default:
                        destination = byte.MaxValue;
                        destination = ref Unsafe.AddByteOffset(ref destination, 1);
                    RESET_STATUS:
                        status = uint.MaxValue;
                        break;
                }

                Unsafe.WriteUnaligned(ref destination, source);
                destination = ref Unsafe.AddByteOffset(ref destination, 2);
            }

            return Unsafe.ByteOffset(ref initialDestination, ref destination);
        }
    }

    private static ReadOnlySpan<byte> Shuffle() => new byte[]
    {
        0, 16, 1, 16, 2, 16, 3, 16, 4, 16, 5, 16, 6, 16, 7, 16,
        8, 16, 9, 16, 10, 16, 11, 16, 12, 16, 13, 16, 14, 16, 15, 16,
        0, 2, 4, 6, 8, 10, 12, 14, 16, 16, 16, 16, 16, 16, 16, 16,
    };

    public static nint GetChars(scoped ref byte source, int sourceLength, scoped ref char destination)
    {
        scoped ref var initialDestination = ref destination;
        while (sourceLength > 0)
        {
            var index = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, ushort>(ref source), sourceLength >>> 1).IndexOf(ushort.MaxValue) << 1;
            if (index < 0)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref destination), ref source, unchecked((uint)sourceLength));
                destination = ref Unsafe.AddByteOffset(ref destination, sourceLength);
                goto RETURN;
            }

            if (index != 0)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref destination), ref source, unchecked((uint)index));
                destination = ref Unsafe.AddByteOffset(ref destination, index);
            }

            index += 2;
            source = ref Unsafe.AddByteOffset(ref source, index);
            sourceLength -= index;

            index = MemoryMarshal.CreateSpan(ref source, sourceLength).IndexOf(byte.MaxValue);
            if (index < 0)
            {
                destination = ref ToUtf16(ref source, ref Unsafe.AddByteOffset(ref source, sourceLength), ref destination);
                goto RETURN;
            }

            if (index == 0)
            {
                source = ref Unsafe.AddByteOffset(ref source, 1);
            }
            else
            {
                destination = ref ToUtf16(ref source, ref Unsafe.AddByteOffset(ref source, index), ref destination);
            }

            ++index;
            source = ref Unsafe.AddByteOffset(ref source, index);
            sourceLength -= index;
        }

    RETURN:
        return Unsafe.ByteOffset(ref initialDestination, ref destination) >>> 1;
    }

    private static ref char ToUtf16(scoped ref byte source, scoped ref byte sourceEnd, ref char destination)
    {
        if (Vector128.IsHardwareAccelerated)
        {
            var count = Unsafe.ByteOffset(ref source, ref sourceEnd) >>> 4;
            if (count > 0)
            {
                ref var end = ref Unsafe.AddByteOffset(ref source, (uint)(count - 1) << 4);
                var shuffle0 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(Shuffle()));
                var shuffle1 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(Shuffle()), 16);
                do
                {
                    var vector = Vector128.LoadUnsafe(ref source);
                    Vector128.Shuffle(vector, shuffle0).StoreUnsafe(ref Unsafe.As<char, byte>(ref destination));
                    Vector128.Shuffle(vector, shuffle1).StoreUnsafe(ref Unsafe.As<char, byte>(ref destination), 16);
                    destination = ref Unsafe.AddByteOffset(ref destination, 32);
                    source = ref Unsafe.AddByteOffset(ref source, 16);
                } while (Unsafe.AreSame(ref source, ref end));
            }
        }

        for (; Unsafe.IsAddressLessThan(ref source, ref sourceEnd); source = ref Unsafe.AddByteOffset(ref source, 1), destination = ref Unsafe.Add(ref destination, 1))
        {
            destination = (char)(ushort)source;
        }

        return ref destination;
    }
}
