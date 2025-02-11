namespace GBemulator.MemoryManagementUnit;

// Memory Map can be read here: https://gbdev.io/pandocs/Memory_Map.html
public class MMU
{
    private readonly byte[] _memory;
    private readonly byte[] _bootROM;
    private bool _bootMode;

    public MMU(byte[]? memory = null, bool bootMode = false, byte[]? bootROM = null)
    {
        // Memory array should be 2^16
        _memory = new byte[0xFFFF + 1];

        // Boot ROM array should be 2^8
        _bootROM = new byte[0x00FF + 1];
        _bootMode = bootMode;

        if (memory is not null)
        {
            memory.CopyTo(_memory, 0);
        }

        if (bootROM is not null)
        {
            bootROM.CopyTo(_bootROM, 0);
        }
    }

    public byte Read8Mapped(ushort address) => address switch
    {
        <= 0x00FF when _bootMode == false => _bootROM[address], // During boot the boot ROM is initially mapped over cartridge ROM
        <= 0x3FFF => Read8(address),                            // ROM Bank 00
        <= 0x7FFF => Read8(address),                            // ROM Bank 01-NN (switchable ROM Bank)
        <= 0x9FFF => Read8(address),                            // VRAM (Switchable in CGB mode)
        <= 0xBFFF => Read8(address),                            // External RAM
        <= 0xCFFF => Read8(address),                            // Work RAM
        <= 0xDFFF => Read8(address),                            // Work RAM (Switchable in CGB mode)
        <= 0xFDFF => Read8((ushort)(address - 0x2000)),         // Echo RAM (mirror of C000–DDFF)
        <= 0xFE9F => Read8(address),                            // Object attribute memory (OAM)
        <= 0xFEFF => 0xFF,                                      // Not Usable, see https://gbdev.io/pandocs/Memory_Map.html#fea0feff-range
        <= 0xFF7F => Read8(address),                            // I/O Registers
        <= 0xFFFE => Read8(address),                            // High RAM
        <= 0xFFFF => Read8(address),                            // Interrupt Enable Register (IE)
    };

    public byte Read8(ushort address)
    {
        return _memory[address];
    }

    public ushort Read16(ushort address)
    {
        byte leastSignificant = _memory[address];
        byte mostSignificant = _memory[(ushort)(address + 1)];

        ushort result = Helpers.BytesToUshort(mostSignificant, leastSignificant);

        return result;
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