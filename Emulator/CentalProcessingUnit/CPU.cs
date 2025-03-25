using GBemulator.MemoryManagementUnit;

namespace GBemulator.CentralProcessingUnit;

public partial class CPU
{
    private MMU _mmu;
    private Registers _registers;
    public Registers Registers => _registers;
    public bool Running { get; set; }
    public bool Halting { get; set; }

    private bool InterruptMasterEnable
    {
        get => _registers.InterruptMasterEnable;
        set => _registers.InterruptMasterEnable = value;
    }

    private bool QueueInterruptMasterEnableSet
    {
        get => _registers.QueueInterruptMasterEnableSet;
        set => _registers.QueueInterruptMasterEnableSet = value;
    }
    private ushort StackPointer
    {
        get => _registers.StackPointer;
        set => _registers.StackPointer = value;
    }
    private ushort ProgramCounter
    {
        get => _registers.ProgramCounter;
        set => _registers.ProgramCounter = value;
    }
    private byte Accumulator
    {
        get => _registers.Accumulator;
        set => _registers.Accumulator = value;
    }

    // Only the upper 4 bits can be used, the lower 4 should always be 0
    // However certain instructions will try to assign the lower bits so a mask is used to handle that
    private byte Flags
    {
        get => (byte)(_registers.Flags & 0xF0);
        set => _registers.Flags = (byte)(value & 0xF0);
    }
    private byte B
    {
        get => _registers.B;
        set => _registers.B = value;
    }
    private byte C
    {
        get => _registers.C;
        set => _registers.C = value;
    }
    private byte D
    {
        get => _registers.D;
        set => _registers.D = value;
    }
    private byte E
    {
        get => _registers.E;
        set => _registers.E = value;
    }
    private byte H
    {
        get => _registers.H;
        set => _registers.H = value;
    }
    private byte L
    {
        get => _registers.L;
        set => _registers.L = value;
    }

    /// <summary>
    /// 7 bit in the AccumulatorFlags register (AF) <br />
    /// Called z
    /// </summary>
    private bool ZeroFlag
    {
        get => Flags.GetBit(7);
        set => Flags = Flags.SetBit(7, value);
    }

    /// <summary>
    /// 6 bit in the AccumulatorFlags register (AF) <br />
    /// Called n
    /// </summary>
    private bool SubtractionFlag
    {
        get => Flags.GetBit(6);
        set => Flags = Flags.SetBit(6, value);
    }

    /// <summary>
    /// 5 bit in the AccumulatorFlags register (AF) <br />
    /// Called h
    /// </summary>
    private bool HalfCarryFlag
    {
        get => Flags.GetBit(5);
        set => Flags = Flags.SetBit(5, value);
    }

    /// <summary>
    /// 4 bit in the AccumulatorFlags register (AF) <br />
    /// Called c
    /// </summary>
    private bool CarryFlag
    {
        get => Flags.GetBit(4);
        set => Flags = Flags.SetBit(4, value);
    }

    private ushort AccumulatorFlags
    {
        get => Helpers.BytesToUshort(Accumulator, Flags);
        set => (Accumulator, Flags) = Helpers.UshortToBytes(value);
    }

    private ushort BC
    {
        get => Helpers.BytesToUshort(B, C);
        set => (B, C) = Helpers.UshortToBytes(value);
    }

    private ushort DE
    {
        get => Helpers.BytesToUshort(D, E);
        set => (D, E) = Helpers.UshortToBytes(value);
    }

    private ushort HL
    {
        get => Helpers.BytesToUshort(H, L);
        set => (H, L) = Helpers.UshortToBytes(value);
    }

    public CPU(MMU mmu, Registers registers)
    {
        _mmu = mmu;
        _registers = registers;
    }


    private byte GetNext8Bits()
    {
        byte result = _mmu.Read8(ProgramCounter);
        ProgramCounter++;
        return result;

    }

    private ushort GetNext16Bits()
    {
        ushort result = _mmu.Read16(ProgramCounter);
        ProgramCounter += 2;
        return result;
    }

    public void Cycle()
    {
        HandleInterrupts();

        if (Halting != true)
        {
            byte instructionCode = GetNext8Bits();

            ExecuteInstruction(instructionCode);
            IncrementCycles(instructionCode);
        }
        else
        {
            IncrementCycles(0x00);
        }
    }

    // https://gbdev.io/pandocs/Interrupts.html#interrupt-handling
    public bool HandleInterrupts()
    {
        if (QueueInterruptMasterEnableSet == true)
        {
            InterruptMasterEnable = true;
            QueueInterruptMasterEnableSet = false;
        }

        System.Console.WriteLine("halt stopped");
        Halting = false;

        if (InterruptMasterEnable == false)
        {
            return false;
        }

        ushort interruptAddress;

        switch (_mmu.GetNextInterrupt())
        {
            case Interrupt.VBlank:
                interruptAddress = 0x0040;
                break;
            case Interrupt.LCD:
                interruptAddress = 0x0048;
                break;
            case Interrupt.Timer:
                interruptAddress = 0x0050;
                break;
            case Interrupt.Serial:
                interruptAddress = 0x0058;
                break;
            case Interrupt.Joypad:
                interruptAddress = 0x0060;
                break;
            default:
                return false;
        }

        InterruptMasterEnable = false;
        _mmu.IncrementTimersByMCycles(2);

        PushStack(ProgramCounter);
        _mmu.IncrementTimersByMCycles(2);

        ProgramCounter = interruptAddress;
        _mmu.IncrementTimersByMCycles(1);

        return true;
    }
}