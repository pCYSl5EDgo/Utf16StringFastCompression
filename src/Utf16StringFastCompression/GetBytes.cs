namespace Utf16StringFastCompression;

partial class Utf16CompressionEncoding
{
    /// <summary>
    /// Serialize to span.
    /// </summary>
    /// <param name="source">Source Char Span</param>
    /// <param name="destination">Destination Byte Span</param>
    /// <returns>The number of encoded bytes.</returns>
    public static int GetBytes(ReadOnlySpan<char> source, Span<byte> destination) => GetBytes(ref MemoryMarshal.GetReference(source), source.Length, ref MemoryMarshal.GetReference(destination)).ToInt32();

    public static int GetBytes(char[] source, int sourceLength, byte[] destination) => GetBytes(ref MemoryMarshal.GetArrayDataReference(source), sourceLength, ref MemoryMarshal.GetArrayDataReference(destination)).ToInt32();

    /// <summary>
    /// Serialize to reference.
    /// </summary>
    /// <param name="source">Source Char Reference</param>
    /// <param name="sourceLength">Source Char Length</param>
    /// <param name="destination">Destination Byte Reference</param>
    /// <returns>The number of encoded bytes.</returns>
    public static nint GetBytes(scoped ref char source, nint sourceLength, scoped ref byte destination)
    {
        unchecked
        {
            if (sourceLength < 4)
            {
                if (sourceLength <= 0)
                {
                    return 0;
                }

                sourceLength <<= 1;
                Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.As<char, byte>(ref source), (uint)sourceLength);
                return sourceLength;
            }

            scoped ref byte initialDestination = ref destination;
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
                            destination = ref WriteMarkerByte(ref destination);
                        }

                        v0.AsByte().StoreUnsafe(ref destination);
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<byte>>());
                        continue;
                    }

                    if (!isAscii)
                    {
                        destination = ref WriteMarkerUInt16(ref destination);
                    }

                    var v1 = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>());
                    var packed = Vector256.Narrow(v0, v1);
                    isAscii = (v1 & filter) == Vector256<ushort>.Zero;
                    if (isAscii)
                    {
                        packed.StoreUnsafe(ref destination);
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<byte>>());
                        continue;
                    }

                    packed.GetLower().StoreUnsafe(ref destination);
                    destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<byte>>() >>> 1);
                    destination = byte.MaxValue;
                    destination = ref Unsafe.AddByteOffset(ref destination, 1);
                    v1.AsByte().StoreUnsafe(ref destination);
                    destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<byte>>());
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
                            destination = ref WriteMarkerByte(ref destination);
                        }

                        v0.AsByte().StoreUnsafe(ref destination);
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<byte>>());
                        continue;
                    }

                    if (!isAscii)
                    {
                        destination = ref WriteMarkerUInt16(ref destination);
                    }

                    var v1 = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                    var packed = Vector128.Narrow(v0, v1);
                    isAscii = (v1 & filter) == Vector128<ushort>.Zero;
                    if (isAscii)
                    {
                        packed.StoreUnsafe(ref destination);
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<byte>>());
                        continue;
                    }

                    Unsafe.WriteUnaligned(ref destination, packed.AsUInt64().GetElement(0));
                    // packed.GetLower().StoreUnsafe(ref destination);
                    // I don't know which is better.
                    destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<byte>>() >>> 1);
                    destination = byte.MaxValue;
                    destination = ref Unsafe.AddByteOffset(ref destination, 1);
                    v1.AsByte().StoreUnsafe(ref destination);
                    destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<byte>>());
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
                            Unsafe.WriteUnaligned(ref destination, value);
                            break;
                        }

                        Unsafe.WriteUnaligned(ref destination, (ushort)0xffff);
                        destination = ref Unsafe.AddByteOffset(ref destination, 2);
                    }

                    if (status == 0U)
                    {
                        status = 4U;
                    }

                    destination = (byte)value;
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
                        destination = 0;
                        destination = ref Unsafe.AddByteOffset(ref destination, 1);
                        ref var sub4 = ref Unsafe.SubtractByteOffset(ref destination, 4);
                        Unsafe.WriteUnaligned(ref Unsafe.SubtractByteOffset(ref sub4, 2), (ushort)sub4);
                        Unsafe.WriteUnaligned(ref sub4, (ushort)Unsafe.AddByteOffset(ref sub4, 1));
                        goto RESET_STATUS;
                    default:
                        destination = byte.MaxValue;
                        destination = ref Unsafe.AddByteOffset(ref destination, 1);
                    RESET_STATUS:
                        status = 0;
                        break;
                }

                destination = ref Write(ref destination, value);
            }

            if (status == 2)
            {
                // 0xff, 0xff, _st0, _st1, dest
                // _st0, 0x00, _st1, 0x00, dest
                ref var sub2 = ref Unsafe.SubtractByteOffset(ref destination, 2);
                Unsafe.WriteUnaligned(ref Unsafe.SubtractByteOffset(ref destination, 4), (ushort)sub2);
                Unsafe.WriteUnaligned(ref sub2, (ushort)Unsafe.SubtractByteOffset(ref destination, 1));
            }

            return Unsafe.ByteOffset(ref initialDestination, ref destination);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static nint GetCompactBytesSlowPath(scoped ref char source, nint sourceLength, scoped ref byte destination)
    {
        scoped ref byte initialDestination = ref destination;
        scoped ref char sourceEnd = ref Unsafe.Add(ref source, sourceLength);
        var status = 0U;
        if (Vector256.IsHardwareAccelerated && Unsafe.ByteOffset(ref source, ref sourceEnd) <= Unsafe.SizeOf<Vector256<ushort>>())
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<ushort>>());
            Vector256<byte> narrow;
            var filter = Vector256.Create<ushort>(0xff80);
            while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
            {
                var v0 = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>());
                // 0b00 ← utf16, 0b11 ← ascii
                uint b0 = Vector256.Equals(v0 & filter, Vector256<ushort>.Zero).AsByte().ExtractMostSignificantBits();
                if (b0 == 0U)
                {
                    destination = ref TryWriteMarkerByte(ref destination, ref status);
                    v0.AsByte().StoreUnsafe(ref destination);
                    destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<byte>>());
                    continue;
                }

                if (Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
                {
                    // no additional Vector256
                    narrow = Vector256.Narrow(v0, Vector256<ushort>.Zero);
                    break;
                }
                
                var v1 = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>());
                ulong rbits = Vector256.Equals(v1 & filter, Vector256<ushort>.Zero).AsByte().ExtractMostSignificantBits();
                rbits <<= 32;
                rbits |= b0;
                // 0b11 ← utf16, 0b00 ← ascii
                rbits = ~rbits;
                narrow = Vector256.Narrow(v0, v1);
                var tzc = BitOperations.TrailingZeroCount(rbits);
                if ((tzc >>> 1) + status >= 4)
                {
                    destination = ref TransitFromUnicodeToAscii(ref destination, ref status);
                    narrow.StoreUnsafe(ref destination);
                    destination = ref Unsafe.AddByteOffset(ref destination, tzc);
                }

                if (tzc == 64)
                {
                    continue;
                }
                
                destination = ref WriteMarkerByte(ref destination);
                rbits |= (1UL << tzc) - 1UL;
                // 0b00 ← utf16, 0b11 ← ascii
                var continuous = ~(rbits | (rbits >>> 2) | (rbits >>> 4) | (rbits >>> 6));
                while (continuous != 0UL)
                {
                    var tzcNext = BitOperations.TrailingZeroCount(continuous);
                    var copyLengthNext = tzcNext - tzc;
                    Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.SubtractByteOffset(ref Unsafe.As<char, byte>(ref source), 64 - tzc), (uint)copyLengthNext);
                    destination = ref Unsafe.AddByteOffset(ref destination, copyLengthNext);
                    destination = ref WriteMarkerUInt16(ref destination);
                    var copyAsciiLength = (BitOperations.TrailingZeroCount(~(continuous >>> tzcNext)) >>> 1) + 3;
                    Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.AddByteOffset(ref Unsafe.As<Vector256<byte>, byte>(ref narrow), tzcNext >>> 1), (uint)copyAsciiLength);
                    destination = ref Unsafe.AddByteOffset(ref destination, copyAsciiLength);
                    destination = ref WriteMarkerByte(ref destination);
                    continuous &= (ulong.MaxValue << tzcNext);
                    tzc = (copyAsciiLength << 1) + tzcNext;
                }

                // no 4 or more continuous ascii
                status = (uint)BitOperations.LeadingZeroCount(rbits);
                var copyLength = 64 - (uint)tzc;
                if (copyLength != 0)
                {
                    Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.SubtractByteOffset(ref Unsafe.As<char, byte>(ref source), copyLength), copyLength);
                    destination = ref Unsafe.AddByteOffset(ref destination, copyLength);
                }
                continue;
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<ushort>>());
        }

        while (Unsafe.IsAddressLessThan(ref source, ref sourceEnd))
        {
            var value = (ushort)source;
            source = ref Unsafe.Add(ref source, 1);
            if (value >= 0x80)
            {
                destination = ref TryWriteMarkerByte(ref destination, ref status);
                destination = ref Write(ref destination, value);
                continue;
            }

            if (++status == 4)
            {
                destination = ref TransitFromUnicodeToAscii(ref destination);
            }
            if (status == 0)
            {
                status = 4;
            }
            destination = ref Write(ref destination, (byte)value);
        }
        
        return Unsafe.ByteOffset(ref initialDestination, ref destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte WriteMarkerByte(ref byte destination) => ref Write(ref destination, byte.MaxValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte TryWriteMarkerByte(ref byte destination, ref uint status)
    {
        var tmp = status;
        status = 0;
        if (tmp < 4)
        {
            return ref destination;
        }

        return ref WriteMarkerByte(ref destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Write(ref byte destination, byte value)
    {
        destination = value;
        return ref Unsafe.AddByteOffset(ref destination, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Write(ref byte destination, ushort value)
    {
        Unsafe.WriteUnaligned(ref destination, value);
        return ref Unsafe.AddByteOffset(ref destination, 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte WriteMarkerUInt16(ref byte destination) => ref Write(ref destination, ushort.MaxValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte TransitFromUnicodeToAscii(ref byte destination, ref uint status)
    {
        switch (status)
        {
            case 0:
                destination = ref WriteMarkerUInt16(ref destination);
                break;
            case 1:
            {
                ref var sub2 = ref Unsafe.SubtractByteOffset(ref destination, 2);
                destination = sub2;
                WriteMarkerUInt16(ref sub2);
                destination = ref Unsafe.AddByteOffset(ref destination, 1);
                break;
            }
            case 2:
            {
                ref var sub2 = ref Unsafe.SubtractByteOffset(ref destination, 2);
                ref var sub4 = ref Unsafe.SubtractByteOffset(ref destination, 4);
                Unsafe.WriteUnaligned(ref sub2, (ushort)((sub2 << 8) | sub4));
                WriteMarkerUInt16(ref sub4);
                break;
            }
            case 3:
                destination = ref TransitFromUnicodeToAscii(ref destination);
                break;
        }

        status = 4;
        return ref destination;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte TransitFromUnicodeToAscii(ref byte destination)
    {
        ref var sub4 = ref Unsafe.SubtractByteOffset(ref destination, 4);
        ref var sub6 = ref Unsafe.SubtractByteOffset(ref destination, 4);
        Unsafe.WriteUnaligned(ref sub4, (ushort)((sub4 << 8) | sub6));
        WriteMarkerUInt16(ref sub6);
        return ref Unsafe.SubtractByteOffset(ref destination, 1);
    }
}
