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
                            Unsafe.WriteUnaligned(ref destination, ushort.MaxValue);
                            destination = ref Unsafe.AddByteOffset(ref destination, 2);
                        }
                        Vector256.Narrow(v0, v1).StoreUnsafe(ref destination);
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<byte>>());
                    }
                    else
                    {
                        if (isAscii)
                        {
                            isAscii = false;
                            destination = byte.MaxValue;
                            destination = ref Unsafe.AddByteOffset(ref destination, 1);
                        }

                        v0.AsByte().StoreUnsafe(ref destination);
                        v1.AsByte().StoreUnsafe(ref destination, (uint)Unsafe.SizeOf<Vector256<byte>>());
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<byte>>() << 1);
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
                            Unsafe.WriteUnaligned(ref destination, ushort.MaxValue);
                            destination = ref Unsafe.AddByteOffset(ref destination, 2);
                        }
                        Vector128.Narrow(v0, v1).StoreUnsafe(ref destination);
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<byte>>());
                    }
                    else
                    {
                        if (isAscii)
                        {
                            isAscii = false;
                            destination = byte.MaxValue;
                            destination = ref Unsafe.AddByteOffset(ref destination, 1);
                        }

                        v0.AsByte().StoreUnsafe(ref destination);
                        v1.AsByte().StoreUnsafe(ref destination, (uint)Unsafe.SizeOf<Vector128<byte>>());
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<byte>>() << 1);
                    }
                }
                status = isAscii ? 4U : 0U;
            }

            for (; Unsafe.IsAddressLessThan(ref source, ref sourceEnd); source = ref Unsafe.Add(ref source, 1))
            {
                if (source < 0x80)
                {
                    if (status++ == 0U)
                    {
                        Unsafe.WriteUnaligned(ref destination, (ushort)0xffff);
                        destination = ref Unsafe.AddByteOffset(ref destination, 2);
                    }

                    if (status == 0U)
                    {
                        status = 4U;
                    }

                    destination = unchecked((byte)(ushort)source);
                    destination = ref Unsafe.AddByteOffset(ref destination, 1);
                    continue;
                }

                switch (status)
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
                        status = 0;
                        break;
                }

                Unsafe.WriteUnaligned(ref destination, source);
                destination = ref Unsafe.AddByteOffset(ref destination, 2);
            }

            switch (status)
            {
                case 1:
                    // 0xff, 0xff, _st0, dest
                    // _st0, 0x00, dest
                    destination = ref Unsafe.SubtractByteOffset(ref destination, 1);
                    Unsafe.WriteUnaligned(ref Unsafe.SubtractByteOffset(ref destination, 2), (ushort)destination);
                    break;
                case 2:
                    // 0xff, 0xff, _st0, _st1, dest
                    // _st0, 0x00, _st1, 0x00, dest
                    ref var sub2 = ref Unsafe.SubtractByteOffset(ref destination, 2);
                    Unsafe.WriteUnaligned(ref Unsafe.SubtractByteOffset(ref sub2, 2), (ushort)sub2);
                    Unsafe.WriteUnaligned(ref sub2, (ushort)Unsafe.SubtractByteOffset(ref destination, 1));
                    break;
            }

            return Unsafe.ByteOffset(ref initialDestination, ref destination);
        }
    }

    public static nint GetChars(scoped ref byte source, int sourceLength, scoped ref char destination)
    {
        unchecked
        {
            scoped ref var initialDestination = ref destination;
            scoped ref var sourceEnd = ref Unsafe.AddByteOffset(ref source, (uint)sourceLength);
            var isAscii = false;
            while (!Unsafe.AreSame(ref source, ref sourceEnd))
            {
                if (isAscii)
                {
                    if (source == byte.MaxValue)
                    {
                        isAscii = false;
                    }
                    else
                    {
                        destination = (char)(ushort)source;
                        destination = ref Unsafe.Add(ref destination, 1);
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
                    destination = (char)value;
                    destination = ref Unsafe.Add(ref destination, 1);
                }
                source = ref Unsafe.AddByteOffset(ref source, 2);
            }

            return Unsafe.ByteOffset(ref initialDestination, ref destination) >>> 1;
        }
    }
}
