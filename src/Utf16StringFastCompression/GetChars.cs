namespace Utf16StringFastCompression;

partial class Utf16CompressionEncoding
{
    /// <summary>
    /// Deserialize to span
    /// </summary>
    /// <param name="source">Source byte span</param>
    /// <param name="destination">Destination char span</param>
    /// <returns>The number of decoded chars.</returns>
    public static int GetChars(ReadOnlySpan<byte> source, Span<char> destination) => GetChars(ref MemoryMarshal.GetReference(source), source.Length, ref MemoryMarshal.GetReference(destination)).ToInt32();

    /// <summary>
    /// Deserialize to array
    /// </summary>
    /// <param name="source">Source byte array</param>
    /// <param name="sourceLength">Source byte length</param>
    /// <param name="destination">Destination char array</param>
    /// <returns>The number of decoded chars.</returns>
    public static int GetChars(byte[] source, int sourceLength, char[] destination) => GetChars(ref MemoryMarshal.GetArrayDataReference(source), sourceLength, ref MemoryMarshal.GetArrayDataReference(destination)).ToInt32();

    /// <summary>
    /// Deserialize to reference
    /// </summary>
    /// <param name="source">Source byte reference</param>
    /// <param name="sourceLength">Source byte length</param>
    /// <param name="destination">Destination char reference</param>
    /// <returns>The number of decoded chars.</returns>
    public static nint GetChars(scoped ref byte source, nint sourceLength, scoped ref char destination)
    {
        var state = new ToCharState();
        return GetCharsStateful(ref source, sourceLength, ref destination, ref state);
    }
    
    public static nint GetCharsStateful(scoped ref byte source, nint sourceLength, scoped ref char destination, scoped ref ToCharState state)
    {
        if (Unsafe.IsNullRef(ref source) || sourceLength <= 0 || Unsafe.IsNullRef(ref destination))
        {
            return 0;
        }

        unchecked
        {
            scoped ref var initialDestination = ref destination;
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
                    destination = (char)value;
                    destination = ref Unsafe.Add(ref destination, 1);
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
                            for (; --tzc >= 0; source = ref Unsafe.AddByteOffset(ref source, 1), destination = ref Unsafe.Add(ref destination, 1))
                            {
                                destination = (char)source;
                            }

                            source = ref Unsafe.AddByteOffset(ref source, 1);
                            continue;
                        }

                        var (v0, v1) = Vector256.Widen(vector);
                        v0.StoreUnsafe(ref Unsafe.As<char, ushort>(ref destination));
                        v1.StoreUnsafe(ref Unsafe.As<char, ushort>(ref destination), (uint)Unsafe.SizeOf<Vector256<ushort>>() >>> 1);
                        destination = ref Unsafe.Add(ref destination, Unsafe.SizeOf<Vector256<ushort>>());
                        source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<ushort>>());
                    }
                    else
                    {
                        if (Vector256.EqualsAny(vector.AsUInt16(), filter.AsUInt16()))
                        {
                            isAscii = true;
                            var tzc = BitOperations.TrailingZeroCount(Vector256.Equals(vector.AsUInt16(), filter.AsUInt16()).ExtractMostSignificantBits());
                            tzc <<= 1;
                            if (tzc != 0)
                            {
                                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref destination), ref source, (uint)tzc);
                            }

                            destination = ref Unsafe.AddByteOffset(ref destination, tzc);
                            source = ref Unsafe.AddByteOffset(ref source, tzc + 2);
                            continue;
                        }

                        vector.StoreUnsafe(ref Unsafe.As<char, byte>(ref destination));
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector256<ushort>>());
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
                            for (; --tzc >= 0; source = ref Unsafe.AddByteOffset(ref source, 1), destination = ref Unsafe.Add(ref destination, 1))
                            {
                                destination = (char)source;
                            }

                            source = ref Unsafe.AddByteOffset(ref source, 1);
                            continue;
                        }

                        var (v0, v1) = Vector128.Widen(vector);
                        v0.StoreUnsafe(ref Unsafe.As<char, ushort>(ref destination));
                        v1.StoreUnsafe(ref Unsafe.As<char, ushort>(ref destination), (uint)Unsafe.SizeOf<Vector128<ushort>>() >>> 1);
                        destination = ref Unsafe.Add(ref destination, Unsafe.SizeOf<Vector128<ushort>>());
                        source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<ushort>>());
                    }
                    else
                    {
                        if (Vector128.EqualsAny(vector.AsUInt16(), filter.AsUInt16()))
                        {
                            isAscii = true;
                            var tzc = BitOperations.TrailingZeroCount(Vector128.Equals(vector.AsUInt16(), filter.AsUInt16()).ExtractMostSignificantBits());
                            tzc <<= 1;
                            if (tzc != 0)
                            {
                                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref destination), ref source, (uint)tzc);
                            }

                            destination = ref Unsafe.AddByteOffset(ref destination, tzc);
                            source = ref Unsafe.AddByteOffset(ref source, tzc + 2);
                            continue;
                        }

                        vector.StoreUnsafe(ref Unsafe.As<char, byte>(ref destination));
                        destination = ref Unsafe.AddByteOffset(ref destination, Unsafe.SizeOf<Vector128<ushort>>());
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
                if (Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
                {
                    state.HasRemainingByte = true;
                    state.RemainingByte = source;
                    break;
                }
            }

            state.IsAsciiMode = isAscii;
            return Unsafe.ByteOffset(ref initialDestination, ref destination) >>> 1;
        }
    }
}
