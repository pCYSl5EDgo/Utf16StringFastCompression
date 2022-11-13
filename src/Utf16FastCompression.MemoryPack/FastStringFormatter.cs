using MemoryPack;
using System.Buffers;

namespace Utf16FastCompression.MemoryPack;

public sealed class FastStringFormatter : IMemoryPackFormatter<string>
{
    public void Deserialize(scoped ref MemoryPackReader reader, scoped ref string? value)
    {
        throw new NotImplementedException();
    }

    public void Serialize<TBufferWriter>(scoped ref MemoryPackWriter<TBufferWriter> writer, scoped ref string? value) where TBufferWriter : IBufferWriter<byte>
    {
        throw new NotImplementedException();
    }
}
