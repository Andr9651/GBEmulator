using GBemulator.MemoryManagementUnit;


namespace GBemulator.CentalProcessingUnit;
public partial class CPU
{
    public ushort StackPointer { get; set; }
    public ushort ProgramCounter { get; set; }
    private byte _accumulator;
    private byte _flags;

    public byte Flags
    {
        get { return _flags; }
        set { _flags = value; }
    }

    public ushort AccumulatorFlags { get; set; }

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

    public byte B;
    public byte C;
    public byte D;
    public byte E;
    public byte H;
    public byte L;
    public ushort BC => (ushort)((B << 8) + C);
    public ushort DE => (ushort)((D << 8) + E);
    public ushort HL => (ushort)((H << 8) + L);


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