using GBemulator.MemoryManagementUnit;

namespace GBemulator.CentralProcessingUnit;

public partial class CPU
{
    private MMU _mmu;
    private bool _running;
    private Registers _registers;
    public Registers Registers => _registers;

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
    private byte Flags
    {
        get => _registers.Flags;
        set => _registers.Flags = value;
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

    public void Run()
    {
        _running = true;

        while (_running == true)
        {
            byte instructionCode = _mmu.Read8(ProgramCounter);
            ExecuteInstruction(instructionCode);
            IncrementProgramCounter(instructionCode);
        }
    }
}