namespace Utf16StringFastCompression;

public struct ToCharState
{
    public bool IsAsciiMode;
    public bool HasRemainingByte;
    public byte RemainingByte;
}
