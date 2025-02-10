using GBemulator.CentralProcessingUnit;
using GBemulator.MemoryManagementUnit;

// for (int i = 0; i <= 0xFF; i++)
// {
//     Console.WriteLine($"case 0x{i:X2}: ");
//     Console.WriteLine("break;");
//     if (i % 8 == 7)
//     {
//         Console.WriteLine();
//     }
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

var registers = Registers.PostBootRegistersDMG();

var cpu = new CPU(mmu, registers);

cpu.Run();

Console.WriteLine($"{cpu.Registers}");