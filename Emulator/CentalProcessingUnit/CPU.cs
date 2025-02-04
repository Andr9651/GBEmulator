using GBemulator.MemoryManagementUnit;

namespace GBemulator.CentalProcessingUnit;

public partial class CPU
{
    private MMU _mmu;
    private bool running;

    public CPU(MMU mmu)
    {
        _mmu = mmu;
    }

    public void Run()
    {
        LoadInstructions();
        running = true;

        while (running == true)
        {
            byte instructionCode = _mmu.Read8(ProgramCounter);
            ProgramCounter++;
            ExecuteInstruction(instructionCode);
        }
    }
}