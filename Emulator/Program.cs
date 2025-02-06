using GBemulator.CentalProcessingUnit;
using GBemulator.MemoryManagementUnit;

// for (int i = 0; i <= 0xFF; i++)
// {
//     Console.WriteLine($"case 0x{i:X2}: ");
//     Console.WriteLine("break;");
// }

byte[] program = {
    0x00,

    0x01,
    0x0f,
    0xf0,

    0x03,
    0x10,
};

var mmu = new MMU(program);

var cpu = new CPU(mmu);

cpu.Run();

Console.WriteLine($"{cpu.BC:X4}");
Console.WriteLine(cpu);