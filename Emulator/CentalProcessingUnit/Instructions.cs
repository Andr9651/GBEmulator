namespace GBemulator.CentalProcessingUnit;

public partial class CPU
{
    private Action[] _instructions = new Action[0xff];
    private Action[] _cbInstructions = new Action[0xff];

    private void LoadInstructions()
    {
        _instructions[0x00] = NoOp;
        _instructions[0x10] = Stop;

        _instructions[0x01] = () => Load16BitRegister(ref B, ref C);
        _instructions[0x11] = () => Load16BitRegister(ref D, ref E);
        _instructions[0x21] = () => Load16BitRegister(ref H, ref L);
        _instructions[0x31] = LoadStackPointer;

        _instructions[0x03] = () => Increment16BitRegister(ref B, ref C);
        _instructions[0x13] = () => Increment16BitRegister(ref D, ref E);
        _instructions[0x23] = () => Increment16BitRegister(ref H, ref L);
        _instructions[0x33] = IncrementStackPointer;
    }

    public void ExecuteInstruction(byte instructionCode)
    {
        Action instruction = _instructions[instructionCode];

        if (instruction == null)
        {
            throw new NotImplementedException($"Instruction: {instructionCode:X4}");
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

    public void Load16BitRegister(ref byte mostSignificant, ref byte leastSignificant)
    {
        leastSignificant = _mmu.Read8(ProgramCounter);
        ProgramCounter++;

        mostSignificant = _mmu.Read8(ProgramCounter);
        ProgramCounter++;
    }

    public void LoadStackPointer()
    {
        byte leastSignificant = _mmu.Read8(ProgramCounter);
        ProgramCounter++;

        byte mostSignificant = _mmu.Read8(ProgramCounter);
        ProgramCounter++;

        StackPointer = Helpers.BytesToUshort(mostSignificant, leastSignificant);
    }

    public void Increment16BitRegister(ref byte mostSignificant, ref byte leastSignificant)
    {
        leastSignificant++;

        if (leastSignificant == 0)
        {
            mostSignificant++;
        }
    }

    public void IncrementStackPointer()
    {
        StackPointer++;
    }
}