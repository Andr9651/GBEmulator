using GBemulator;
using GBemulator.CentralProcessingUnit;
using GBemulator.MemoryManagementUnit;

bool DEBUG = true;

string path = args[0] ?? "../ROMs";

byte[] romBytes = Helpers.LoadROM(path);

var mmu = new MMU(romBytes);

var registers = Registers.PostBootRegistersGameboyDoctor();

var cpu = new CPU(mmu, registers);

int debugRun = -1;

cpu.Running = true;

File.WriteAllText("log.txt", null);

while (cpu.Running == true)
{
    if (DEBUG)
    {
        string logString = cpu.Registers.ToString() + " PCMEM:";

        ushort programCounter = cpu.Registers.ProgramCounter;

        logString += $"{mmu.Read8Mapped(programCounter):X2},";
        logString += $"{mmu.Read8Mapped(++programCounter):X2},";
        logString += $"{mmu.Read8Mapped(++programCounter):X2},";
        logString += $"{mmu.Read8Mapped(++programCounter):X2}\n";

        File.AppendAllText("log.txt", logString);
        // Console.WriteLine(logString);

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

    cpu.Cycle();
}