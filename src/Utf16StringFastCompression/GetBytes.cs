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
                        destination = ref Write(ref destination, byte.MaxValue);
                    }

                    v0.AsByte().StoreUnsafe(ref destination);
                    destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<byte>>());
                    continue;
                }

                if (!isAscii)
                {
                    destination = ref Write(ref destination, ushort.MaxValue);
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
                destination = ref Write(ref destination, byte.MaxValue);
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
                        destination = ref Write(ref destination, byte.MaxValue);
                    }

                    v0.AsByte().StoreUnsafe(ref destination);
                    destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<byte>>());
                    continue;
                }

                if (!isAscii)
                {
                    destination = ref Write(ref destination, ushort.MaxValue);
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
                destination = ref Write(ref destination, byte.MaxValue);
                v1.AsByte().StoreUnsafe(ref destination);
                destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<byte>>());
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
                if (status >= 4)
                {
                    destination = ref Write(ref destination, byte.MaxValue);
                }
                status = 0;
                destination = ref Write(ref destination, value);
                continue;
            }

            if (++status < 4)
            {
                destination = ref Write(ref destination, value);
                continue;
            }

            if (status == 4)
            {
                destination = ref TransitFromUnicodeToAscii(ref destination);
            }
            
            status = 4;
            destination = ref Write(ref destination, (byte)value);
        }

        return Unsafe.ByteOffset(ref initialDestination, ref destination);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static nint GetCompactBytesSlowPath(scoped ref char source, nint sourceLength, scoped ref byte destination)
    {
        if (sourceLength < 4)
        {
            sourceLength <<= 1;
            if (sourceLength <= 0)
            {
                return 0;
            }

            Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.As<char, byte>(ref source), (uint)sourceLength);
            return sourceLength;
        }

        scoped ref byte initialDestination = ref destination;
        scoped ref char sourceEnd = ref Unsafe.Add(ref source, sourceLength);
        var status = 0U;
        while (!Unsafe.AreSame(ref source, ref sourceEnd))
        {
            var value = (ushort)source;
            source = ref Unsafe.Add(ref source, 1);
            if (value >= 0x80)
            {
                if (status >= 4)
                {
                    destination = ref Write(ref destination, byte.MaxValue);
                }
                status = 0;
                destination = ref Write(ref destination, value);
                continue;
            }

            if (++status < 4)
            {
                destination = ref Write(ref destination, value);
                continue;
            }

            if (status == 4)
            {
                destination = ref TransitFromUnicodeToAscii(ref destination);
            }
            
            status = 4;
            destination = ref Write(ref destination, (byte)value);
        }

        return Unsafe.ByteOffset(ref initialDestination, ref destination);
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
    private static ref byte TransitFromUnicodeToAscii(ref byte destination)
    {
        // ST0L, ST0U, ST1L, ST1U, ST2L, ST2U, destination
        // 0xff, 0xff, ST0L, ST1L, ST2L, destination
        ref var sub4 = ref Unsafe.SubtractByteOffset(ref destination, 4);
        ref var sub6 = ref Unsafe.SubtractByteOffset(ref destination, 6);
        Unsafe.WriteUnaligned(ref sub4, (ushort)((sub4 << 8) | sub6));
        Unsafe.WriteUnaligned(ref sub6, ushort.MaxValue);
        return ref Unsafe.SubtractByteOffset(ref destination, 1);
    }
}
