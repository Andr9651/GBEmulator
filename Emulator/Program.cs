using GBemulator;
using GBemulator.CentralProcessingUnit;
using GBemulator.MemoryManagementUnit;

byte[] romBytes = Helpers.LoadROM("../ROMs");

var mmu = new MMU(romBytes);

var registers = Registers.PostBootRegistersDMG();

var cpu = new CPU(mmu, registers);

cpu.Run();

Console.WriteLine($"{cpu.Registers}");