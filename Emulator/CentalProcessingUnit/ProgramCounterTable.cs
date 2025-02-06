
namespace GBemulator.CentralProcessingUnit;

public partial class CPU
{
    // Data copied from https://meganesu.github.io/generate-gb-opcodes/
    private readonly byte[] ProgramCounterIncrementsPerInstruction = {
    //  x0	x1	x2	x3	x4	x5	x6	x7	x8	x9	xA	xB	xC	xD	xE	xF
        1,  3,  1,  1,  1,  1,  2,  1,  3,  1,  1,  1,  1,  1,  2,  1, // 0x
        2,  3,  1,  1,  1,  1,  2,  1,  2,  1,  1,  1,  1,  1,  2,  1, // 1x
        2,  3,  1,  1,  1,  1,  2,  1,  2,  1,  1,  1,  1,  1,  2,  1, // 2x
        2,  3,  1,  1,  1,  1,  2,  1,  2,  1,  1,  1,  1,  1,  2,  1, // 3x
        1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, // 4x
        1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, // 5x
        1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, // 6x
        1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, // 7x
        1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, // 8x
        1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, // 9x
        1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, // Ax
        1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, // Bx
        1,  1,  3,  3,  3,  1,  2,  1,  1,  1,  3,  0,  3,  3,  2,  1, // Cx
        1,  1,  3,  0,  3,  1,  2,  1,  1,  1,  3,  0,  3,  0,  2,  1, // Dx
        2,  1,  1,  0,  0,  1,  2,  1,  2,  1,  3,  0,  0,  0,  2,  1, // Ex
        2,  1,  1,  1,  0,  1,  2,  1,  2,  1,  3,  1,  0,  0,  2,  1, // Fx
    };

    private void IncrementProgramCounter(ushort instructionCode)
    {
        ProgramCounter += ProgramCounterIncrementsPerInstruction[instructionCode];

        if (instructionCode == 0xCB)
        {
            ProgramCounter += 2;
        }
    }
}