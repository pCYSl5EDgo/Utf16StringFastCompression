using System;
using System.Text;
using System.Text.Unicode;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Utf16StringFastCompression;
using BenchmarkDotNet.Attributes;

[MediumRunJob]
public class ContinuousTest
{
    private uint[] array = new uint[4096];
    [IterationSetup]
    public void Setup()
    {
        System.Random.Shared.NextBytes(MemoryMarshal.Cast<uint, byte>(array.AsSpan()));
    }

    [Benchmark]
    public uint Continuous4_2stepLoop()
    {
        uint answer = 0; for (nint i = 0; i < array.Length; ++i) answer += Continuous4_2step(array[i]); return answer;
    }
    [Benchmark]
    public uint Continuous4_4stepLoop()
    {
        uint answer = 0; for (nint i = 0; i < array.Length; ++i) answer += Continuous4_4step(array[i]); return answer;
    }
    [Benchmark]
    public uint Continuous4_Pext_2stepLoop()
    {
        uint answer = 0; for (nint i = 0; i < array.Length; ++i) answer += Continuous4_Pext_2step(array[i]); return answer;
    }
    public static uint Continuous4_Pext_2step(uint bits)
    {
        if (!Bmi2.IsSupported) { return 0; }
        bits = Bmi2.ParallelBitExtract(bits, 0x55555555U);
        bits |= bits >>> 2;
        return bits | (bits >>> 4);
    }
    public static uint Continuous4_4step(uint bits) => bits | (bits >>> 2) | (bits >>> 4) | (bits >>> 6);
    public static uint Continuous4_2step(uint bits)
    {
        bits |= (bits >>> 2);
        return bits | (bits >>> 4);
    }
}

[MediumRunJob]
public class NarrowTest
{
    private byte[] array = new byte[4096 * 32];
    [IterationSetup]
    public void Setup()
    {
        System.Random.Shared.NextBytes(array.AsSpan());
    }

    [Benchmark]
    public void Loop128()
    {
        var destination = new byte[array.Length >>> 1];
        ref var destItr = ref MemoryMarshal.GetArrayDataReference(destination);
        ref var itr = ref MemoryMarshal.GetArrayDataReference(array);
        ref var itrEnd = ref Unsafe.Add(ref itr, array.Length);
        do
        {
            Vector256<ushort> vec = Vector256.LoadUnsafe(ref itr).AsUInt16();
            Vector128<byte> narrow = Vector128.Narrow(vec.AsUInt16().GetLower(), vec.AsUInt16().GetUpper());
            narrow.StoreUnsafe(ref destItr);
            itr = ref Unsafe.Add(ref itr, 32);
            destItr = ref Unsafe.Add(ref destItr, 16);
        } while (!Unsafe.AreSame(ref itr, ref itrEnd));
    }

    [Benchmark]
    public void Loop256Self()
    {
        var destination = new byte[array.Length >>> 1];
        ref var destItr = ref MemoryMarshal.GetArrayDataReference(destination);
        ref var itr = ref MemoryMarshal.GetArrayDataReference(array);
        ref var itrEnd = ref Unsafe.Add(ref itr, array.Length);
        do
        {
            Vector256<ushort> vec = Vector256.LoadUnsafe(ref itr).AsUInt16();
            var lower = Vector256.Narrow(vec, vec).GetLower();
            lower.StoreUnsafe(ref destItr);
            itr = ref Unsafe.Add(ref itr, 32);
            destItr = ref Unsafe.Add(ref destItr, 16);
        } while (!Unsafe.AreSame(ref itr, ref itrEnd));
    }

    [Benchmark]
    public void Loop256()
    {
        var destination = new byte[array.Length >>> 1];
        ref var destItr = ref MemoryMarshal.GetArrayDataReference(destination);
        ref var itr = ref MemoryMarshal.GetArrayDataReference(array);
        ref var itrEnd = ref Unsafe.Add(ref itr, array.Length);
        do
        {
            Vector256<byte> narrow = Vector256.Narrow(Vector256.LoadUnsafe(ref itr).AsUInt16(), Vector256.LoadUnsafe(ref Unsafe.Add(ref itr, 32)).AsUInt16());
            narrow.StoreUnsafe(ref destItr);
            itr = ref Unsafe.Add(ref itr, 64);
            destItr = ref Unsafe.Add(ref destItr, 32);
        } while (!Unsafe.AreSame(ref itr, ref itrEnd));
    }
}

[MediumRunJob]
public class PextTest
{
    private Vector256<ushort>[] array = new Vector256<ushort>[4096];
    [IterationSetup]
    public void Setup()
    {
        System.Random.Shared.NextBytes(MemoryMarshal.Cast<Vector256<ushort>, byte>(array.AsSpan()));
    }

    [Benchmark]
    public ulong MSBLoop()
    {
        ulong answer = 0;
        for (nint i = 0; i < array.Length; ++i)
        {
            ref var vector = ref array[i];
            answer += vector.ExtractMostSignificantBits();
        }
        return answer;
    }
    [Benchmark]
    public ulong PextLoop()
    {
        ulong answer = 0;
        for (nint i = 0; i < array.Length; ++i)
        {
            ref var vector = ref array[i];
            answer += Pext(vector.AsByte().ExtractMostSignificantBits());
        }
        return answer;
    }
    [Benchmark]
    public ulong NarrowLoop()
    {
        ulong answer = 0;
        for (nint i = 0; i < array.Length; ++i)
        {
            ref var vector = ref array[i];
            answer += Narrow(vector.AsByte().ExtractMostSignificantBits());
        }
        return answer;
    }

    public static ulong Pext(ulong value)
    {
        if (Bmi2.X64.IsSupported)
        {
            return Bmi2.X64.ParallelBitExtract(value, 0x55555555_55555555UL);
        }
        return 0;
    }

    public static ulong Narrow(ulong value)
    {
        return
           (value & 0x0000_0000_0000_0001UL)
        | ((value & 0x0000_0000_0000_0004UL) >>> 1)
        | ((value & 0x0000_0000_0000_0010UL) >>> 2)
        | ((value & 0x0000_0000_0000_0040UL) >>> 3)
        | ((value & 0x0000_0000_0000_0100UL) >>> 4)
        | ((value & 0x0000_0000_0000_0400UL) >>> 5)
        | ((value & 0x0000_0000_0000_1000UL) >>> 6)
        | ((value & 0x0000_0000_0000_4000UL) >>> 7)
        | ((value & 0x0000_0000_0001_0000UL) >>> 8)
        | ((value & 0x0000_0000_0004_0000UL) >>> 9)
        | ((value & 0x0000_0000_0010_0000UL) >>> 10)
        | ((value & 0x0000_0000_0040_0000UL) >>> 11)
        | ((value & 0x0000_0000_0100_0000UL) >>> 12)
        | ((value & 0x0000_0000_0400_0000UL) >>> 13)
        | ((value & 0x0000_0000_1000_0000UL) >>> 14)
        | ((value & 0x0000_0000_4000_0000UL) >>> 15)
        | ((value & 0x0000_0001_0000_0000UL) >>> 0x10)
        | ((value & 0x0000_0004_0000_0000UL) >>> 0x11)
        | ((value & 0x0000_0010_0000_0000UL) >>> 0x12)
        | ((value & 0x0000_0040_0000_0000UL) >>> 0x13)
        | ((value & 0x0000_0100_0000_0000UL) >>> 0x14)
        | ((value & 0x0000_0400_0000_0000UL) >>> 0x15)
        | ((value & 0x0000_1000_0000_0000UL) >>> 0x16)
        | ((value & 0x0000_4000_0000_0000UL) >>> 0x17)
        | ((value & 0x0001_0000_0000_0000UL) >>> 0x18)
        | ((value & 0x0004_0000_0000_0000UL) >>> 0x19)
        | ((value & 0x0010_0000_0000_0000UL) >>> 0x1b)
        | ((value & 0x0040_0000_0000_0000UL) >>> 0x1a)
        | ((value & 0x0100_0000_0000_0000UL) >>> 0x1c)
        | ((value & 0x0400_0000_0000_0000UL) >>> 0x1d)
        | ((value & 0x1000_0000_0000_0000UL) >>> 0x1e)
        | ((value & 0x4000_0000_0000_0000UL) >>> 0x1f);
    }
}
