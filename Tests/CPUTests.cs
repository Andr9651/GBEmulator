using GBemulator.CentralProcessingUnit;
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
            0x10, 0x00
        };

        var mmu = new MMU(program);

        var registers = Registers.EmptyRegisters();

        var cpu = new CPU(mmu, registers);

        cpu.Run();

        Assert.Equal(0xf0, cpu.Registers.B);
        Assert.Equal(0x10, cpu.Registers.C);
        Assert.Equal(7, cpu.Registers.ProgramCounter);
    }
}