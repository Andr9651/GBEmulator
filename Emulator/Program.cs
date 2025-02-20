using GBemulator;
using GBemulator.CentralProcessingUnit;
using GBemulator.MemoryManagementUnit;

bool DEBUG = true;

string path = args[0] ?? "../ROMs";

byte[] romBytes = Helpers.LoadROM(path);

var mmu = new MMU(romBytes);

var registers = Registers.PostBootRegistersDMG();


var cpu = new CPU(mmu, registers);

int debugRun = 0;

while (cpu.Running == true)
{
    cpu.Cycle();

    if (DEBUG)
    {
        Console.WriteLine(cpu.Registers);

        if (debugRun == 0)
        {
            File.WriteAllBytes("../Memory.bin", mmu.Memory);
            debugRun += Helpers.ConsoleReadNumber();
        }
        else
        {
            debugRun--;
        }
    }
}


Console.WriteLine($"{cpu.Registers}");