using GBemulator;
using GBemulator.CentralProcessingUnit;
using GBemulator.MemoryManagementUnit;

var tokens = Helpers.TokenizeArgs(args);

bool writeLogs = Helpers.GetArgBool(tokens, "--log");
string romPath = Helpers.GetArgString(tokens, "--path") ?? "../ROMs";
int cycleCount = Helpers.GetArgInt(tokens, "--max-cycles") ?? (writeLogs == true ? 500_000 : 5_000_000);
int nextCyclePause = Helpers.GetArgInt(tokens, "--pause-at-cycle") ?? -1;
bool pauseForDebugger = Helpers.GetArgBool(tokens, "--pause");

if (pauseForDebugger)
{
    Console.WriteLine("Pausing to wait for debugger. Press any key to continue");
    Console.Read();
}

byte[] romBytes = Helpers.LoadROM(romPath);
var mmu = new MMU(romBytes);
var registers = Registers.PostBootRegistersGameboyDoctor();
var cpu = new CPU(mmu, registers);

var logFile = new StreamWriter("log.txt");
int linecount = 0;

cpu.Running = true;
while (cpu.Running == true)
{
    ushort programCounter = cpu.Registers.ProgramCounter;

    string logString = cpu.Registers.ToString() + " PCMEM:";
    logString += $"{mmu.Read8(programCounter):X2},";
    logString += $"{mmu.Read8(++programCounter):X2},";
    logString += $"{mmu.Read8(++programCounter):X2},";
    logString += $"{mmu.Read8(++programCounter):X2}";

    if (writeLogs == true)
    {
        logFile.WriteLine(logString);
    }

    if (nextCyclePause == 0)
    {
        Console.WriteLine(logString);
        nextCyclePause += Helpers.ConsoleReadNumber();
    }
    else
    {
        nextCyclePause--;
    }

    cpu.Cycle();

    linecount++;
    if (linecount > cycleCount) break;
}

logFile.Close();