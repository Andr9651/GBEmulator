namespace GBemulator.CentralProcessingUnit;

public struct Registers
{
    public ushort StackPointer;
    public ushort ProgramCounter;
    public byte Accumulator;
    public byte Flags;
    public byte B;
    public byte C;
    public byte D;
    public byte E;
    public byte H;
    public byte L;

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