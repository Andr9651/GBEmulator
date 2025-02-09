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
    // The instruction to set the InterruptMasterEnable is delayed by one instruction so this is here to enable that
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
}