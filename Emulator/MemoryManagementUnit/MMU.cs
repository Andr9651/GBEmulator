namespace GBemulator.MemoryManagementUnit;

// Memory Map can be read here: https://gbdev.io/pandocs/Memory_Map.html
public class MMU
{
    public byte[] Memory => _memory;

    private readonly byte[] _memory;
    private readonly byte[] _bootROM;
    private bool _bootMode;

    private byte _dividerRegister
    {
        get => _memory[0xFF04];
        set => _memory[0xFF04] = value;
    }

    private byte _timerCounter
    {
        get => _memory[0xFF05];
        set => _memory[0xFF05] = value;
    }

    private byte _timerModulo
    {
        get => _memory[0xFF06];
        set => _memory[0xFF06] = value;
    }

    private byte _interruptEnable
    {
        get => _memory[0xFFFF];
        set => _memory[0xFFFF] = value;
    }

    private byte _interruptFlags
    {
        get => _memory[0xFF0F];
        set => _memory[0xFF0F] = value;
    }

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

    public byte Read8(ushort address) => address switch
    {
        <= 0x00FF when _bootMode == true => _bootROM[address],      // During boot the boot ROM is initially mapped over cartridge ROM
        <= 0x3FFF => Read8Direct(address),                          // ROM Bank 00
        <= 0x7FFF => Read8Direct(address),                          // ROM Bank 01-NN (switchable ROM Bank)
        <= 0x9FFF => Read8Direct(address),                          // VRAM (Switchable in CGB mode)
        <= 0xBFFF => Read8Direct(address),                          // External RAM
        <= 0xCFFF => Read8Direct(address),                          // Work RAM
        <= 0xDFFF => Read8Direct(address),                          // Work RAM (Switchable in CGB mode)
        <= 0xFDFF => Read8Direct((ushort)(address - 0x2000)),       // Echo RAM (mirror of C000–DDFF)
        <= 0xFE9F => Read8Direct(address),                          // Object attribute memory (OAM)
        <= 0xFEFF => 0xFF,                                          // Not Usable, see https://gbdev.io/pandocs/Memory_Map.html#fea0feff-range
        0xFF44 => 0x90,                                             // Hardcoded LCD for Gameboy Doctor
        <= 0xFF7F => Read8Direct(address),                          // I/O Registers
        <= 0xFFFE => Read8Direct(address),                          // High RAM
        <= 0xFFFF => Read8Direct(address),                          // Interrupt Enable Register (IE)
    };

    public void Write8Mapped(ushort address, byte value)
    {
        switch (address)
        {
            case <= 0x3FFF: break;                                              // ROM Bank 00
            case <= 0x7FFF: break;                                              // ROM Bank 01-NN (switchable ROM Bank)
            case <= 0x9FFF: Write8(address, value); break;                      // VRAM (Switchable in CGB mode)
            case <= 0xBFFF: Write8(address, value); break;                      // External RAM
            case <= 0xCFFF: Write8(address, value); break;                      // Work RAM
            case <= 0xDFFF: Write8(address, value); break;                      // Work RAM (Switchable in CGB mode)
            case <= 0xFDFF: Write8((ushort)(address - 0x2000), value); break;   // Echo RAM (mirror of C000–DDFF)
            case <= 0xFE9F: Write8(address, value); break;                      // Object attribute memory (OAM)
            case <= 0xFEFF: break;                                              // Not Usable, see https://gbdev.io/pandocs/Memory_Map.html#fea0feff-range
            case 0xFF04: Write8(address, 0x00); break;                          // DIB: Divider register  
            case 0xFF05: Write8(address, 0x00); break;                          // TIMA: Timer counter  
            case 0xFF06: Write8(address, 0x00); break;                          // TMA: Timer modulo  
            case 0xFF07: Write8(address, 0x00); break;                          // TCA: Timer control  
            case <= 0xFF7F: Write8(address, value); break;                      // I/O Registers
            case <= 0xFFFE: Write8(address, value); break;                      // High RAM
            case <= 0xFFFF: Write8(address, value); break;                      // Interrupt Enable Register (IE)
        }
    }

    public byte Read8Direct(ushort address)
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
        if (address == 0xFF01) // Serial buffer address
        {
            Console.Write((char)value);
        }

        if (address == 0xFFFF) // Serial buffer address
        {
            Console.WriteLine($"Interrupt Enable: {value:b8}");
        }
        _memory[address] = value;
    }

    public void Write16(ushort address, ushort value)
    {
        var (mostSignificant, leastSignificant) = Helpers.UshortToBytes(value);

        _memory[address] = leastSignificant;
        _memory[(ushort)(address + 1)] = mostSignificant;
    }

    public Interrupt GetNextInterrupt()
    {
        byte interruptEnable = _interruptEnable;
        byte interruptFlag = _interruptFlags;

        // The order of the patterns matter as the lower interrupt flags take precident over the higher ones (e.g. LCD is over Timer, but under VBlank)
        var interrupt = (interruptEnable, interruptFlag) switch
        {
            var (enable, flag) when enable.GetBit(0) && flag.GetBit(0) => Interrupt.VBlank,
            var (enable, flag) when enable.GetBit(1) && flag.GetBit(1) => Interrupt.LCD,
            var (enable, flag) when enable.GetBit(2) && flag.GetBit(2) => Interrupt.Timer,
            var (enable, flag) when enable.GetBit(3) && flag.GetBit(3) => Interrupt.Serial,
            var (enable, flag) when enable.GetBit(4) && flag.GetBit(4) => Interrupt.Joypad,
            _ => Interrupt.None
        };

        switch (interrupt)
        {
            case Interrupt.VBlank: _interruptFlags = Helpers.SetBit(_interruptFlags, 0, false); break;
            case Interrupt.LCD: _interruptFlags = Helpers.SetBit(_interruptFlags, 1, false); break;
            case Interrupt.Timer: _interruptFlags = Helpers.SetBit(_interruptFlags, 2, false); break;
            case Interrupt.Serial: _interruptFlags = Helpers.SetBit(_interruptFlags, 3, false); break;
            case Interrupt.Joypad: _interruptFlags = Helpers.SetBit(_interruptFlags, 4, false); break;
            default: break;
        }

        return interrupt;
    }

    // https://gbdev.io/pandocs/Timer_and_Divider_Registers.html
    public void IncrementTimersByMCycles(int value)
    {
        _dividerRegister = (byte)(_dividerRegister + value * 4);

        int newCounter = _timerCounter + value * 4;
        if (newCounter > 0xFF)
        {
            _timerCounter = (byte)(_timerModulo + (newCounter - 0xFF));
            _interruptFlags = _interruptFlags.SetBit(2, true);
        }
    }
}