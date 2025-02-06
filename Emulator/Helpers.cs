namespace GBemulator;

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

    public static bool GetBit(this byte bits, byte place)
    {
        if (place > 7)
        {
            throw new ArgumentException("Place must be between 0-7", nameof(place));
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

    public static byte SetBit(this byte bits, byte place, bool value)
    {
        if (place > 7)
        {
            throw new ArgumentException("Place must be between 0-7", nameof(place));
        }

        byte mask = (byte)(1 << place);
        mask ^= byte.MaxValue;
        bits &= mask;

        if (value == true)
        {
            bits += (byte)(1 << place);
        }

        return bits;
    }

    public static ushort BytesToUshort(byte mostSignificant, byte leastSignificant)
    {
        return (ushort)((mostSignificant << 8) + leastSignificant);
    }

    public static (byte, byte) UshortToBytes(ushort value)
    {
        byte leastSignificant = (byte)(value & byte.MaxValue);
        byte mostSignificant = (byte)(value >> 8);

        return (mostSignificant, leastSignificant);
    }

    public static byte RotateByteLeft(this byte value)
    {
        bool bit7 = GetBit(value, 7);
        value <<= 1;
        value = SetBit(value, 0, bit7);

        return value;
    }

    public static byte RotateByteRight(this byte value)
    {
        bool bit0 = GetBit(value, 0);
        value >>= 1;
        value = SetBit(value, 7, bit0);

        return value;
    }
}
