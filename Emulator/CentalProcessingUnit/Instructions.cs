namespace GBemulator.CentalProcessingUnit;

public partial class CPU
{
    private Action[] _instructions8Bit = new Action[0xff];
    private Action[] _instructions16Bit = new Action[0xff];

    private void LoadInstructions()
    {
        _instructions8Bit[0x00] = NoOp;
        _instructions8Bit[0x10] = Stop;
    }

    public void ExecuteInstruction(byte instructionCode)
    {
        Action instruction = _instructions8Bit[instructionCode];

        if (instruction == null)
        {
            throw new NotImplementedException($"Instruction: {instructionCode}");
        }

        instruction();
    }

    public void NoOp()
    {
    }

    public void Stop()
    {
        running = false;
    }
}