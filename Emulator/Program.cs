using GBemulator;
using GBemulator.CentralProcessingUnit;
using GBemulator.MemoryManagementUnit;

string path = args[0] ?? "../ROMs";

byte[] romBytes = Helpers.LoadROM(path);

var mmu = new MMU(romBytes);

var registers = Registers.PostBootRegistersDMG();

var cpu = new CPU(mmu, registers);

cpu.Run();

Console.WriteLine($"{cpu.Registers}");