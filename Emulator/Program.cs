using GBemulator;
using GBemulator.CentralProcessingUnit;
using GBemulator.MemoryManagementUnit;

bool DEBUG = true;

string path = args[0] ?? "../ROMs";
int cycleCount = int.TryParse(args[1], out int number) ? number : 50000;

byte[] romBytes = Helpers.LoadROM(path);

var mmu = new MMU(romBytes);

var registers = Registers.PostBootRegistersGameboyDoctor();

var cpu = new CPU(mmu, registers);

int debugRun = -1;

cpu.Running = true;

StreamWriter logFile = new StreamWriter("log.txt");
int linecount = 0;

while (cpu.Running == true)
{
    if (linecount > cycleCount) break;
    if (DEBUG)
    {
        string logString = cpu.Registers.ToString() + " PCMEM:";

        ushort programCounter = cpu.Registers.ProgramCounter;

        logString += $"{mmu.Read8(programCounter):X2},";
        logString += $"{mmu.Read8(++programCounter):X2},";
        logString += $"{mmu.Read8(++programCounter):X2},";
        logString += $"{mmu.Read8(++programCounter):X2}";

        logFile.WriteLine(logString);

        if (debugRun == 0)
        {
            // File.WriteAllBytes("../Memory.bin", mmu.Memory);
            debugRun += Helpers.ConsoleReadNumber();
        }
        else
        {
            debugRun--;
        }
    }

    cpu.Cycle();
    linecount++;
}

logFile.Close();