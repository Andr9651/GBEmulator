using GBemulator.CentalProcessingUnit;
using GBemulator.MemoryManagementUnit;

namespace Tests;
public class CPUTests
{

    [Fact]
    public void LoadAndIncrementBC()
    {
        byte[] program =
        {
            0x00,
            0x01, 0x0f, 0xf0,
            0x03,
            0x10,
        };

        var mmu = new MMU(program);

        var cpu = new CPU(mmu);

        cpu.Run();

        Assert.Equal(0xf010, cpu.BC);
        Assert.Equal(6, cpu.ProgramCounter);
    }
}