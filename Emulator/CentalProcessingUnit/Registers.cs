using GBemulator.MemoryManagementUnit;


namespace GBemulator.CentalProcessingUnit;
public partial class CPU
{
    public ushort StackPointer { get; set; }
    public ushort ProgramCounter { get; set; }

    public ushort AccumulatorFlags { get; set; }
    public byte Accumulator
    {
        get => AccumulatorFlags.GetHigh();
        set => AccumulatorFlags = AccumulatorFlags.SetHigh(value);
    }

    /// <summary>
    /// 7 bit in the AccumulatorFlags register (AF) <br />
    /// Called z
    /// </summary>
    public bool ZeroFlag
    {
        get => AccumulatorFlags.GetBit(7);
        set => AccumulatorFlags = AccumulatorFlags.SetBit(7, value);
    }

    /// <summary>
    /// 6 bit in the AccumulatorFlags register (AF) <br />
    /// Called n
    /// </summary>
    public bool SubtractionFlag
    {
        get => AccumulatorFlags.GetBit(6);
        set => AccumulatorFlags = AccumulatorFlags.SetBit(6, value);
    }

    /// <summary>
    /// 5 bit in the AccumulatorFlags register (AF) <br />
    /// Called h
    /// </summary>
    public bool HalfCarryflag
    {
        get => AccumulatorFlags.GetBit(5);
        set => AccumulatorFlags = AccumulatorFlags.SetBit(5, value);
    }

    /// <summary>
    /// 4 bit in the AccumulatorFlags register (AF) <br />
    /// Called c
    /// </summary>
    public bool Carryflag
    {
        get => AccumulatorFlags.GetBit(4);
        set => AccumulatorFlags = AccumulatorFlags.SetBit(4, value);
    }

    public ushort BC { get; set; }
    public byte B
    {
        get => BC.GetHigh();
        set => BC = BC.SetHigh(value);
    }
    public byte C
    {
        get => BC.GetLow();
        set => BC = BC.SetLow(value);
    }

    public ushort DE { get; set; }
    public byte D
    {
        get => DE.GetHigh();
        set => DE = DE.SetHigh(value);
    }
    public byte E
    {
        get => DE.GetLow();
        set => DE = DE.SetLow(value);
    }

    public ushort HL { get; set; }
    public byte H
    {
        get => HL.GetHigh();
        set => HL = HL.SetHigh(value);
    }
    public byte L
    {
        get => HL.GetLow();
        set => HL = HL.SetLow(value);
    }

    public override string ToString()
    {
        return $@"
AF: {AccumulatorFlags:b16}
BC: {BC:b16}
DE: {DE:b16}
HL: {HL:b16}
StackPointer: {StackPointer:b16}
ProgramCounter: {ProgramCounter:b16} {ProgramCounter}
";
    }
}