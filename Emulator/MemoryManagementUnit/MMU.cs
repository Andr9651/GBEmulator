namespace GBemulator.MemoryManagementUnit;

public class MMU
{
    private readonly byte[] _memory = new byte[0xffff];

    public MMU()
    {

    }

    public MMU(byte[] memory)
    {
        memory.CopyTo(_memory, 0);
    }

    public byte Read8(ushort address)
    {
        return _memory[address];
    }

    public ushort Read16(ushort address)
    {
        byte leastSignificantByte = _memory[address];
        byte mostSignificantByte = _memory[address + 1];

        ushort value = (ushort)((mostSignificantByte << 8) + leastSignificantByte);

        return value;
    }

    public void Write8(ushort address, byte value)
    {
        _memory[address] = value;
    }

    public void Write16(ushort address, ushort value)
    {
        byte leastSignificantByte = (byte)(value & byte.MaxValue);
        byte mostSignificantByte = (byte)(value >> 8);

        _memory[address] = leastSignificantByte;
        _memory[address + 1] = mostSignificantByte;
    }
}