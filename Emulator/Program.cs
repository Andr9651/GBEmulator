using GBemulator.CentalProcessingUnit;
using GBemulator.MemoryManagementUnit;

byte[] program = {
    0x00,
    0x00,
    0x00,
    0x00,
    0x00,
    0x00,
    0x00,
    0x00,
    0x00,
    0x00,
    0x00,
    0x00,
    0x10,
};

var mmu = new MMU(program);

var cpu = new CPU(mmu);

cpu.Run();

Console.WriteLine(cpu);