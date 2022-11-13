namespace Utf16StringFastCompression;

partial class Utf16CompressionEncoding
{
    public static TextKind DetectTextKind(ref char source, nint sourceLength)
    {
        if (Unsafe.IsNullRef(ref source) || sourceLength <= 0)
        {
            return default;
        }
        
        var isAscii = ((ushort)source) < 0x80;
        ref var sourceEnd = ref Unsafe.Add(ref source, sourceLength);
        source = ref Unsafe.Add(ref source, 1);
        if (Vector256.IsHardwareAccelerated && Unsafe.ByteOffset(ref source, ref sourceEnd) >= Unsafe.SizeOf<Vector256<ushort>>())
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<ushort>>());
            var filter = Vector256.Create<ushort>(0xff80);
            if (isAscii)
            {
                while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
                {
                    var vector = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<ushort>>());
                    if ((vector & filter) != Vector256<ushort>.Zero)
                    {
                        return default;
                    }
                }
            }
            else
            {
                while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
                {
                    var vector = Vector256.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector256<ushort>>());
                    if (Vector256.EqualsAny(vector & filter, Vector256<ushort>.Zero))
                    {
                        return default;
                    }
                }
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector256<ushort>>());
        }

        if (Vector128.IsHardwareAccelerated && Unsafe.ByteOffset(ref source, ref sourceEnd) >= Unsafe.SizeOf<Vector128<ushort>>())
        {
            sourceEnd = ref Unsafe.SubtractByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector128<ushort>>());
            var filter = Vector128.Create<ushort>(0xff80);
            if (isAscii)
            {
                while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
                {
                    var vector = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<ushort>>());
                    if ((vector & filter) != Vector128<ushort>.Zero)
                    {
                        return default;
                    }
                }
            }
            else
            {
                while (!Unsafe.IsAddressGreaterThan(ref source, ref sourceEnd))
                {
                    var vector = Vector128.LoadUnsafe(ref Unsafe.As<char, ushort>(ref source));
                    source = ref Unsafe.AddByteOffset(ref source, Unsafe.SizeOf<Vector128<ushort>>());
                    if (Vector128.EqualsAny(vector & filter, Vector128<ushort>.Zero))
                    {
                        return default;
                    }
                }
            }
            sourceEnd = ref Unsafe.AddByteOffset(ref sourceEnd, Unsafe.SizeOf<Vector128<ushort>>());
        }

        while (Unsafe.IsAddressLessThan(ref source, ref sourceEnd))
        {
            if (((ushort)source < 0x80) ^ isAscii)
            {
                return default;
            }
        }

        return (TextKind)(1 + (isAscii ? 1 : 0));
    }
}

public enum TextKind
{
    Mixed,
    AllNotInAsciiRange,
    AllInAsciiRange,
}
