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

    public static int GetBytesDeterministic(ReadOnlySpan<char> source, Span<byte> destination) => GetBytesDeterministic(ref MemoryMarshal.GetReference(source), source.Length, ref MemoryMarshal.GetReference(destination)).ToInt32();

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

                    destination = ref Write(ref destination, ref v0);
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
                    destination = ref Write(ref destination, ref packed);
                    continue;
                }
                
                var lower = packed.GetLower();
                destination = ref Write(ref destination, ref lower);
                destination = ref Write(ref destination, byte.MaxValue);
                destination = ref Write(ref destination, ref v1);
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

                    destination = ref Write(ref destination, ref v0);
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
                    destination = ref Write(ref destination, ref packed);
                    continue;
                }

                destination = ref Write(ref destination, packed.AsUInt64().GetElement(0));
                // packed.GetLower().StoreUnsafe(ref destination);
                // I don't know which is better.
                destination = ref Write(ref destination, byte.MaxValue);
                destination = ref Write(ref destination, ref v1);
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
                destination = ref TransitFromAsciiToUnicode(ref destination, ref status);
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

    public static nint GetBytesDeterministic(scoped ref char source, nint sourceLength, scoped ref byte destination)
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
        if (Vector256.IsHardwareAccelerated && Unsafe.ByteOffset(ref source, ref sourceEnd) >= Unsafe.SizeOf<Vector256<byte>>())
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<byte>>());
            var filter = Vector256.Create((ushort)0xff80);
            while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
            {
                var vec = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                // 00 → unicode, 11 → ascii
                var bits = Vector256.Equals(vec & filter, Vector256<ushort>.Zero).AsByte().ExtractMostSignificantBits();
                if (bits == 0U)
                {
                    destination = ref TransitFromAsciiToUnicode(ref destination, ref status);
                    destination = ref Write(ref destination, ref vec);
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>());
                    continue;
                }
                if (bits == uint.MaxValue)
                {
                    destination = ref TransitFromUnicodeToAscii(ref destination, ref status);
                    var narrow = Vector128.Narrow(vec.GetLower(), vec.GetUpper());
                    destination = ref Write(ref destination, ref narrow);
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>());
                    continue;
                }
                if ((bits & 1U) == 0U)
                {
                    destination = ref TransitFromAsciiToUnicode(ref destination, ref status);
                    var shift = bits & (bits >>> 2) & (bits >>> 4) & (bits >>> 6);
                    if (shift == 0U)
                    {
                        destination = ref Write(ref destination, ref vec);
                        source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<byte>>());
                        status = (uint)(BitOperations.LeadingZeroCount(~bits) >>> 1);
                        continue;
                    }

                    var copySize = BitOperations.TrailingZeroCount(shift);
                    destination = ref Copy(ref destination, ref vec, (uint)copySize);
                    source = ref Unsafe.AddByteOffset(ref source, copySize);
                    destination = ref Write(ref destination, ushort.MaxValue);
                    status = 4;
                    continue;
                }

                var asciiLength = BitOperations.TrailingZeroCount(~bits) >>> 1;
                if (asciiLength + status < 4)
                {
                    status = 0;
                    var copySize = (uint)(asciiLength + 1) << 1;
                    destination = ref Copy(ref destination, ref vec, copySize);
                    source = ref Unsafe.AddByteOffset(ref source, copySize);
                }
                else
                {
                    destination = ref TransitFromUnicodeToAscii(ref destination, ref status);
                    var narrow = Vector128.Narrow(vec.GetLower(), vec.GetUpper());
                    destination = ref Copy(ref destination, ref narrow, (uint)asciiLength);
                    destination = ref Write(ref destination, byte.MaxValue);
                    source = ref Unsafe.Add(ref source, asciiLength);
                    status = 0;
                }
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<byte>>());
        }
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
                    destination = ref TransitFromAsciiToUnicode(ref destination, ref status);
                    destination = ref Write(ref destination, ref vec);
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                    continue;
                }
                if (bits == ushort.MaxValue)
                {
                    destination = ref TransitFromUnicodeToAscii(ref destination, ref status);
                    destination = ref Write(ref destination, Vector128.Narrow(vec, vec).AsUInt64().GetElement(0));
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                    continue;
                }
                if ((bits & 1U) == 0U)
                {
                    destination = ref TransitFromAsciiToUnicode(ref destination, ref status);
                    var shift = bits & (bits >>> 2) & (bits >>> 4) & (bits >>> 6);
                    if (shift == 0U)
                    {
                        destination = ref Write(ref destination, ref vec);
                        source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<byte>>());
                        status = (uint)(BitOperations.LeadingZeroCount((ushort)~bits) >>> 1) - 8U;
                        continue;
                    }

                    var copySize = BitOperations.TrailingZeroCount(shift);
                    destination = ref Copy(ref destination, ref vec, (uint)copySize);
                    source = ref Unsafe.AddByteOffset(ref source, copySize);
                    destination = ref Write(ref destination, ushort.MaxValue);
                    status = 4;
                    continue;
                }

                var asciiLength = BitOperations.TrailingZeroCount(~bits) >>> 1;
                if (asciiLength + status < 4)
                {
                    status = 0;
                    var copySize = (uint)(asciiLength + 1) << 1;
                    destination = ref Copy(ref destination, ref vec, copySize);
                    source = ref Unsafe.AddByteOffset(ref source, copySize);
                }
                else
                {
                    destination = ref TransitFromUnicodeToAscii(ref destination, ref status);
                    var narrow = Vector128.Narrow(vec, vec);
                    destination = ref Copy(ref destination, ref narrow, (uint)asciiLength);
                    destination = ref Write(ref destination, byte.MaxValue);
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
                destination = ref TransitFromAsciiToUnicode(ref destination, ref status);
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
    private static ref byte Write(ref byte destination, ulong value)
    {
        Unsafe.WriteUnaligned(ref destination, value);
        return ref Unsafe.AddByteOffset(ref destination, 8);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Write(ref byte destination, scoped ref Vector256<ushort> value)
    {
        value.AsByte().StoreUnsafe(ref destination);
        return ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<ushort>>());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Write(ref byte destination, scoped ref Vector256<byte> value)
    {
        value.StoreUnsafe(ref destination);
        return ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<ushort>>());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Write(ref byte destination, scoped ref Vector128<ushort> value)
    {
        value.AsByte().StoreUnsafe(ref destination);
        return ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<ushort>>());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Write(ref byte destination, scoped ref Vector128<byte> value)
    {
        value.StoreUnsafe(ref destination);
        return ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<ushort>>());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Copy<T>(ref byte destination, scoped ref T value, uint size) where T : unmanaged
    {
        Unsafe.CopyBlockUnaligned(ref destination, ref Unsafe.As<T, byte>(ref value), size);
        return ref Unsafe.AddByteOffset(ref destination, size);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte TransitFromUnicodeToAscii(ref byte destination, scoped ref uint status)
    {
        var temp = status;
        status = 4;
        switch (temp)
        {
            case 0: return ref Write(ref destination, ushort.MaxValue);
            case 1:
            {
                ref var sub2 = ref Unsafe.SubtractByteOffset(ref destination, 2);
                var value = sub2;
                Unsafe.WriteUnaligned(ref sub2, ushort.MaxValue);
                return ref Write(ref destination, value);
            }
            case 2:
            {
                ref var sub2 = ref Unsafe.SubtractByteOffset(ref destination, 2);
                ref var sub4 = ref Unsafe.SubtractByteOffset(ref destination, 4);
                Unsafe.WriteUnaligned(ref sub2, (ushort)((sub2 << 8) | sub4));
                Unsafe.WriteUnaligned(ref sub4, ushort.MaxValue);
                break;
            }
            case 3: return ref TransitFromUnicodeToAscii(ref destination);
        }
        return ref destination;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte TransitFromAsciiToUnicode(ref byte destination, ref uint status)
    {
        var temp = status;
        status = 0;
        return ref (temp >= 4 ? ref Write(ref destination, byte.MaxValue) : ref destination);
    }
}
