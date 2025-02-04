namespace GBemulator.CentalProcessingUnit;

public partial class CPU
{
    private Action[] _instructions8Bit = new Action[0xff];
    private Action[] _instructions16Bit = new Action[0xff];

    private void LoadInstructions()
    {
        _instructions8Bit[0x00] = NoOp;
        _instructions8Bit[0x10] = Stop;

        _instructions8Bit[0x01] = Load16BitRegister((value) => BC = value);
        _instructions8Bit[0x11] = Load16BitRegister((value) => DE = value);
        _instructions8Bit[0x21] = Load16BitRegister((value) => HL = value);
        _instructions8Bit[0x31] = Load16BitRegister((value) => StackPointer = value);

        _instructions8Bit[0x03] = Increment16BitRegister(() => BC++);
        _instructions8Bit[0x13] = Increment16BitRegister(() => DE++);
        _instructions8Bit[0x23] = Increment16BitRegister(() => HL++);
        _instructions8Bit[0x33] = Increment16BitRegister(() => StackPointer++);
    }

    public void ExecuteInstruction(byte instructionCode)
    {
        Action instruction = _instructions8Bit[instructionCode];

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

    public Action Load16BitRegister(Action<ushort> targetRegister)
    {
        return () =>
        {
            ushort value = _mmu.Read16(ProgramCounter);
            ProgramCounter += 2;
            targetRegister(value);
        };
    }

    public Action Increment16BitRegister(Action targetRegister)
    {
        return targetRegister;
    }
}