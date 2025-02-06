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
        byte leastSignificant = _memory[address];
        byte mostSignificant = _memory[address + 1];

        ushort value = Helpers.BytesToUshort(mostSignificant, leastSignificant);

        return value;
    }

    public void Write8(ushort address, byte value)
    {
        _memory[address] = value;
    }

    public void Write16(ushort address, ushort value)
    {
        var (mostSignificant, leastSignificant) = Helpers.UshortToBytes(value);

        _memory[address] = leastSignificant;
        _memory[address + 1] = mostSignificant;
    }
}