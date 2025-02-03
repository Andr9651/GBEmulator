namespace GB;

public static class Helpers
{
    public static byte GetHigh(this ushort value)
    {
        value >>= 8;
        return (byte)value;
    }

    public static byte GetLow(this ushort value)
    {
        value &= 0x00ff;
        return (byte)value;
    }

    public static ushort SetHigh(this ushort value, byte highValue)
    {
        value &= 0x00ff;
        value += (ushort)(highValue << 8);
        return value;
    }

    public static ushort SetLow(this ushort value, byte lowValue)
    {
        value &= 0xff00;
        value += lowValue;
        return value;
    }

    public static bool GetBit(this ushort bits, byte place)
    {
        if (place > 15)
        {
            throw new ArgumentException("Place must be between 0-15", nameof(place));
        }

        bits >>= place;
        bits &= 1;
        return bits != 0;
    }

    public static ushort SetBit(this ushort bits, byte place, bool value)
    {
        if (place > 15)
        {
            throw new ArgumentException("Place must be between 0-15", nameof(place));
        }

        ushort mask = (ushort)(1 << place);
        mask ^= ushort.MaxValue;
        bits &= mask;

        if (value == true)
        {
            bits += (ushort)(1 << place);
        }

        return bits;
    }
}
