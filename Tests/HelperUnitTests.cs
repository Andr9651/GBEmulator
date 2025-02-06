using GBemulator;

namespace Tests;
public class HelperUnitTests
{
    [Theory]
    [InlineData(0xff00, 0xff)]
    [InlineData(0xffff, 0xff)]
    [InlineData(0x00ff, 0x00)]
    [InlineData(0x0000, 0x00)]
    public void HighByte(ushort value, byte expected)
    {
        Assert.Equal(expected, value.GetHigh());
    }

    [Theory]
    [InlineData(0xff00, 0x00)]
    [InlineData(0xffff, 0xff)]
    [InlineData(0x00ff, 0xff)]
    [InlineData(0x0000, 0x00)]
    public void GetLow(ushort value, byte expected)
    {
        Assert.Equal(expected, value.GetLow());
    }

    [Theory]
    [InlineData(0xffff, 0x00, 0x00ff)]
    [InlineData(0xff00, 0x00, 0x0000)]
    [InlineData(0x00ff, 0xff, 0xffff)]
    [InlineData(0x0000, 0xff, 0xff00)]
    public void SetHigh(ushort value, byte highValue, ushort expected)
    {
        ushort result = value.SetHigh(highValue);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0xffff, 0x00, 0xff00)]
    [InlineData(0x00ff, 0x00, 0x0000)]
    [InlineData(0xff00, 0xff, 0xffff)]
    [InlineData(0x0000, 0xff, 0x00ff)]
    public void SetLow(ushort value, byte lowValue, ushort expected)
    {
        ushort result = value.SetLow(lowValue);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0xffff, 7, true)]
    [InlineData(0xf000, 15, true)]
    public void GetBit(ushort bits, byte place, bool expected)
    {
        bool result = bits.GetBit(place);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(255)]
    public void GetBitError(byte place)
    {
        ushort value = 0xffff;

        Assert.Throws<ArgumentException>(() => value.GetBit(place));
    }

    [Theory]
    [InlineData(0b1111_1111_1111_1111,
     15, false, 0b0111_1111_1111_1111)]

    [InlineData(0b0111_1111_1111_1111,
     15, false, 0b0111_1111_1111_1111)]

    [InlineData(0b1001_1011_0111_1001,
     07, false, 0b1001_1011_0111_1001)]

    [InlineData(0b1001_1011_0111_1001,
     007, true, 0b1001_1011_1111_1001)]

    [InlineData(0b1001_1011_0111_1001,
     000, true, 0b1001_1011_0111_1001)]

    [InlineData(0b1001_1011_0111_1000,
     000, true, 0b1001_1011_0111_1001)]
    public void SetBit(ushort bits, byte place, bool value, ushort expected)
    {
        ushort result = bits.SetBit(place, value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(255)]
    public void SetBitError(byte place)
    {
        ushort value = 0xffff;

        Assert.Throws<ArgumentException>(() => value.SetBit(place, true));
    }

    [Fact]
    public void BytesToUshort()
    {
        byte leastSignificant = 0x0F;
        byte mostSignificant = 0xF0;

        var result = Helpers.BytesToUshort(mostSignificant, leastSignificant);

        Assert.Equal(0xF00F, result);
    }
}
