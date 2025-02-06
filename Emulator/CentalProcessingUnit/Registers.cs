using GBemulator.MemoryManagementUnit;


namespace GBemulator.CentalProcessingUnit;
public partial class CPU
{
    public ushort StackPointer { get; set; }
    public ushort ProgramCounter { get; set; }
    public byte Accumulator { get; set; }
    public byte Flags { get; set; }

    /// <summary>
    /// 7 bit in the AccumulatorFlags register (AF) <br />
    /// Called z
    /// </summary>
    public bool ZeroFlag
    {
        get => Flags.GetBit(7);
        set => Flags = Flags.SetBit(7, value);
    }

    /// <summary>
    /// 6 bit in the AccumulatorFlags register (AF) <br />
    /// Called n
    /// </summary>
    public bool SubtractionFlag
    {
        get => Flags.GetBit(6);
        set => Flags = Flags.SetBit(6, value);
    }

    /// <summary>
    /// 5 bit in the AccumulatorFlags register (AF) <br />
    /// Called h
    /// </summary>
    public bool HalfCarryflag
    {
        get => Flags.GetBit(5);
        set => Flags = Flags.SetBit(5, value);
    }

    /// <summary>
    /// 4 bit in the AccumulatorFlags register (AF) <br />
    /// Called c
    /// </summary>
    public bool Carryflag
    {
        get => Flags.GetBit(4);
        set => Flags = Flags.SetBit(4, value);
    }

    public byte B;
    public byte C;
    public byte D;
    public byte E;
    public byte H;
    public byte L;

    public ushort AccumulatorFlags
    {
        get => Helpers.BytesToUshort(Accumulator, Flags);
        set
        {
            Accumulator = (byte)(value << 8);
            Flags = (byte)(value & 0x0f);
        }
    }

    public ushort BC
    {
        get => Helpers.BytesToUshort(B, C);
        set
        {
            var (b, c) = Helpers.UshortToBytes(value);
            B = b;
            C = c;
        }
    }

    public ushort DE
    {
        get => Helpers.BytesToUshort(D, E);
        set
        {
            D = (byte)(value << 8);
            E = (byte)(value & 0x0f);
        }
    }

    public ushort HL
    {
        get => Helpers.BytesToUshort(H, L);
        set
        {
            H = (byte)(value << 8);
            L = (byte)(value & 0x0f);
        }
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