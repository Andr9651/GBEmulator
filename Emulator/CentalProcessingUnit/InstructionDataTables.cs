
namespace GBemulator.CentralProcessingUnit;

public partial class CPU
{
    // Data copied from https://meganesu.github.io/generate-gb-opcodes/

    // Timing in the gameboy are split into System Clock Ticks (T-cycles) and Machine-cycles (M-cycles)
    // It's split this way since each CPU instruction will always take a multiple of 4 T-cycles per instruction
    // It's therefor (allegedly) simpler in some scenarioes to use M-cycles and multiply by 4 when needed

    // Machine-Cycles for all instructions and conditional instructions where the condition wasn't met
    private readonly byte[] MachineCyclesPerInstruction = [
    //  x0	x1	x2	x3	x4	x5	x6	x7	x8	x9	xA	xB	xC	xD	xE	xF
        1,  3,  2,  2,  1,  1,  2,  1,  5,  2,  2,  2,  1,  1,  2,  1, // 0x
        1,  3,  2,  2,  1,  1,  2,  1,  3,  2,  2,  2,  1,  1,  2,  1, // 1x
        2,  3,  2,  2,  1,  1,  2,  1,  2,  2,  2,  2,  1,  1,  2,  1, // 2x
        2,  3,  2,  2,  3,  3,  3,  1,  2,  2,  2,  2,  1,  1,  2,  1, // 3x
        1,  1,  1,  1,  1,  1,  2,  1,  1,  1,  1,  1,  1,  1,  2,  1, // 4x
        1,  1,  1,  1,  1,  1,  2,  1,  1,  1,  1,  1,  1,  1,  2,  1, // 5x
        1,  1,  1,  1,  1,  1,  2,  1,  1,  1,  1,  1,  1,  1,  2,  1, // 6x
        2,  2,  2,  2,  2,  2,  1,  2,  1,  1,  1,  1,  1,  1,  2,  1, // 7x
        1,  1,  1,  1,  1,  1,  2,  1,  1,  1,  1,  1,  1,  1,  2,  1, // 8x
        1,  1,  1,  1,  1,  1,  2,  1,  1,  1,  1,  1,  1,  1,  2,  1, // 9x
        1,  1,  1,  1,  1,  1,  2,  1,  1,  1,  1,  1,  1,  1,  2,  1, // Ax
        1,  1,  1,  1,  1,  1,  2,  1,  1,  1,  1,  1,  1,  1,  2,  1, // Bx
        2,  3,  3,  4,  3,  4,  2,  4,  2,  4,  3,  0,  3,  6,  2,  4, // Cx
        2,  3,  3,  0,  3,  4,  2,  4,  2,  4,  3,  0,  3,  0,  2,  4, // Dx
        3,  3,  2,  0,  0,  4,  2,  4,  4,  1,  4,  0,  0,  0,  2,  4, // Ex
        3,  3,  2,  1,  0,  4,  2,  4,  3,  2,  4,  1,  0,  0,  2,  4, // Fx
    ];

    // Machine-Cycles for all conditional instructions where the condition was met
    private readonly byte[] MachineCyclesPerConditionalInstruction = [
    //  x0	x1	x2	x3	x4	x5	x6	x7	x8	x9	xA	xB	xC	xD	xE	xF
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // 0x
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // 1x
        3,  0,  0,  0,  0,  0,  0,  0,  3,  0,  0,  0,  0,  0,  0,  0, // 2x
        3,  0,  0,  0,  0,  0,  0,  0,  3,  0,  0,  0,  0,  0,  0,  0, // 3x
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // 4x
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // 5x
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // 6x
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // 7x
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // 8x
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // 9x
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // Ax
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // Bx
        5,  0,  4,  0,  6,  0,  0,  0,  5,  0,  4,  0,  6,  0,  0,  0, // Cx
        5,  0,  4,  0,  6,  0,  0,  0,  5,  0,  4,  0,  6,  0,  0,  0, // Dx
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // Ex
        0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, // Fx
    ];

    // Machine-Cycles for all 16-bit (0xCBxx) instructions 
    private readonly byte[] MachineCyclesPerCBInstruction = [
    //  x0	x1	x2	x3	x4	x5	x6	x7	x8	x9	xA	xB	xC	xD	xE	xF
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // 0x
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // 1x
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // 2x
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // 3x
        2,  2,  2,  2,  2,  2,  3,  2,  2,  2,  2,  2,  2,  2,  3,  2, // 4x
        2,  2,  2,  2,  2,  2,  3,  2,  2,  2,  2,  2,  2,  2,  3,  2, // 5x
        2,  2,  2,  2,  2,  2,  3,  2,  2,  2,  2,  2,  2,  2,  3,  2, // 6x
        2,  2,  2,  2,  2,  2,  3,  2,  2,  2,  2,  2,  2,  2,  3,  2, // 7x
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // 8x
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // 9x
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // Ax
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // Bx
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // Cx
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // Dx
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // Ex
        2,  2,  2,  2,  2,  2,  4,  2,  2,  2,  2,  2,  2,  2,  4,  2, // Fx
    ];

    private bool instructionConditionMet = false;

    private void IncrementCycles(byte InstructionCode)
    {
        byte machineCycles = MachineCyclesPerInstruction[InstructionCode];

        if (instructionConditionMet == true)
        {
            instructionConditionMet = false;
            machineCycles = MachineCyclesPerConditionalInstruction[InstructionCode];
        }

        if (InstructionCode == 0xCB)
        {
            byte CBInstructionCode = _mmu.Read8((ushort)(ProgramCounter + 1));
            machineCycles = MachineCyclesPerCBInstruction[CBInstructionCode];
        }

        _mmu.IncrementTimersByMCycles(machineCycles);
    }
}