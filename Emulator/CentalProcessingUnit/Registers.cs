namespace GBemulator.CentralProcessingUnit;

public struct Registers
{
    public required ushort StackPointer;
    public required ushort ProgramCounter;
    public required byte Accumulator;
    public required byte Flags;
    public required byte B;
    public required byte C;
    public required byte D;
    public required byte E;
    public required byte H;
    public required byte L;
    public required bool InterruptMasterEnable;
    // The instruction (0xFB) to set the InterruptMasterEnable is delayed by one instruction so this is here to enable that
    public required bool QueueInterruptMasterEnableSet;

    public override string ToString()
    {
        return $@"
AF: {Helpers.BytesToUshort(Accumulator, Flags):b16}
BC: {Helpers.BytesToUshort(B, C):b16}
DE: {Helpers.BytesToUshort(D, E):b16}
HL: {Helpers.BytesToUshort(H, L):b16}
StackPointer: {StackPointer:b16}
ProgramCounter: {ProgramCounter:b16} {ProgramCounter}
";
    }

    public static Registers EmptyRegisters()
    {
        return new Registers
        {
            Accumulator = 0,
            Flags = 0,
            B = 0,
            C = 0,
            D = 0,
            E = 0,
            H = 0,
            L = 0,
            ProgramCounter = 0,
            StackPointer = 0,
            InterruptMasterEnable = false,
            QueueInterruptMasterEnableSet = false,
        };
    }

    // The CPU registers aren't cleared after successfully running the boot ROM.
    // So the initial state need to be set for ROMs to work correctly
    // The state has been taken from here https://gbdev.io/pandocs/Power_Up_Sequence.html#cpu-registers
    public static Registers PostBootRegistersDMG()
    {
        return new Registers
        {
            Accumulator = 0x01,
            Flags = 0b1000_0000, // this assumes the header checksum is 0x00
            B = 0x00,
            C = 0x13,
            D = 0x00,
            E = 0xD8,
            H = 0x01,
            L = 0x4D,
            ProgramCounter = 0x100,
            StackPointer = 0xFFFE,
            InterruptMasterEnable = false,
            QueueInterruptMasterEnableSet = false,
        };
    }
}