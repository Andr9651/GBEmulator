using System.IO.Pipelines;

namespace GBemulator.CentralProcessingUnit;

public partial class CPU
{
    private bool CheckHalfCarryAddition(byte value1, byte value2, bool CarryBit = false)
    {
        value1 &= 0xF;
        value2 &= 0xF;
        int result = value1 + value2 + (CarryBit ? 1 : 0);

        return result > 0xF;
    }

    private bool CheckHalfCarryAddition(ushort value1, ushort value2, bool CarryBit = false)
    {
        value1 &= 0xFFF;
        value2 &= 0xFFf;
        int result = value1 + value2 + (CarryBit ? 1 : 0);

        return result > 0xFFF;
    }

    private bool CheckHalfCarrySubtraction(byte value1, byte value2, bool CarryBit = false)
    {
        value1 &= 0xf;
        value2 &= 0xf;
        int result = value1 - value2 - (CarryBit ? 1 : 0);

        return result < 0;
    }

    private void Add16bitRegisterToHL(ushort value)
    {
        int result = HL + value;

        SubtractionFlag = false;
        CarryFlag = result > 0xFFFF;
        HalfCarryFlag = CheckHalfCarryAddition(HL, value);

        HL = (ushort)result;
    }

    private byte Increment8bitRegister(byte value)
    {
        byte result = (byte)(value + 1);

        ZeroFlag = result == 0;
        SubtractionFlag = false;
        HalfCarryFlag = CheckHalfCarryAddition(value, 1);

        return result;
    }

    private byte Decrement8bitRegister(byte value)
    {
        byte result = (byte)(value - 1);

        ZeroFlag = result == 0;
        SubtractionFlag = true;
        HalfCarryFlag = CheckHalfCarrySubtraction(value, 1);

        return result;
    }

    private void WriteToAddressInHL(byte value)
    {
        _mmu.Write8(HL, value);
    }

    private void Add8bitRegisterToAccumulator(byte value)
    {
        int result = Accumulator + value;

        ZeroFlag = result == 0;
        SubtractionFlag = false;
        CarryFlag = result > 0xFF;
        HalfCarryFlag = CheckHalfCarryAddition(Accumulator, value);

        Accumulator = (byte)result;
    }

    private void Add8bitRegisterAndCarryToAccumulator(byte value)
    {
        int result = Accumulator + value + (CarryFlag ? 1 : 0);

        ZeroFlag = result == 0;
        SubtractionFlag = false;
        CarryFlag = result > 0xFF;
        HalfCarryFlag = CheckHalfCarryAddition(Accumulator, value, CarryFlag);

        Accumulator = (byte)result; // numbers greater than 255 overflow when cast to byte
    }

    private void Sub8bitRegisterFromAccumulator(byte value)
    {
        int result = Accumulator - value;

        ZeroFlag = result == 0;
        SubtractionFlag = false;
        CarryFlag = result < 0;
        HalfCarryFlag = CheckHalfCarrySubtraction(Accumulator, value);

        Accumulator = (byte)result; // negative numbers underflow when cast to byte
    }

    private void Sub8bitRegisterAndCarryFromAccumulator(byte value)
    {

    }

    private void And8bitRegisterWithAccumulator(byte value)
    {
        Accumulator = (byte)(Accumulator & value);

        ZeroFlag = Accumulator == 0;
        SubtractionFlag = false;
        CarryFlag = false;
        HalfCarryFlag = true;
    }

    private void Xor8bitRegisterWithAccumulator(byte value)
    {
        Accumulator = (byte)(Accumulator ^ value);

        ZeroFlag = Accumulator == 0;
        SubtractionFlag = false;
        CarryFlag = false;
        HalfCarryFlag = false;
    }

    private void Or8bitRegisterWithAccumulator(byte value)
    {
        Accumulator = (byte)(Accumulator & value);

        ZeroFlag = Accumulator == 0;
        SubtractionFlag = false;
        CarryFlag = false;
        HalfCarryFlag = false;
    }

    private byte RotateByteLeft(byte value)
    {
        byte result = Accumulator.RotateByteLeft();

        ZeroFlag = false;
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = result.GetBit(0); // old bit 7, now at bit 0

        return result;
    }

    private byte RotateByteLeftThroughCarry(byte value)
    {
        bool oldCarry = CarryFlag;

        byte result = RotateByteLeft(value);
        result.SetBit(0, oldCarry);

        return result;
    }

    private byte RotateByteRight(byte value)
    {
        byte result = Accumulator.RotateByteRight();

        ZeroFlag = false;
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = result.GetBit(7); // old bit 0, now at bit 7

        return result;
    }

    private byte RotateByteRightThroughCarry(byte value)
    {
        bool oldCarry = CarryFlag;

        byte result = RotateByteRight(value);
        result.SetBit(7, oldCarry);

        return result;
    }

    // This instruction is equal to subtract but discards the result,
    // and instead uses the ZeroFlag to determine if the values are equal.
    private void Compare8bitRegisterWithAccumulator(byte value)
    {
        byte tempAccumulator = Accumulator;
        Sub8bitRegisterFromAccumulator(value);
        Accumulator = tempAccumulator;
    }

    private void JumpRelative(ushort programCounter)
    {
        sbyte amount = (sbyte)_mmu.Read8(programCounter);
        ProgramCounter = (ushort)(programCounter + amount);
    }

    private void ExecuteInstruction(byte instructionCode)
    {
        // ProgramCounter increments have been deferred so this takes care of the increment that would have happened when reading the current instruction
        ushort programCounter = (ushort)(ProgramCounter + 1);

        // Many instructions uses the byte stored in the address contained in HL
        byte HLValue = _mmu.Read8(HL);

        switch (instructionCode)
        {
            case 0x00:
                // No Operation
                break;
            case 0x01:
                BC = _mmu.Read16(programCounter);
                break;
            case 0x02:
                _mmu.Write8(BC, Accumulator);
                break;
            case 0x03:
                BC++;
                break;
            case 0x04:
                B = Increment8bitRegister(B);
                break;
            case 0x05:
                B = Decrement8bitRegister(B);
                break;
            case 0x06:
                B = _mmu.Read8(programCounter);
                break;
            case 0x07:
                Accumulator = RotateByteLeft(Accumulator);
                break;
            case 0x08:
                ushort address = _mmu.Read16(programCounter);
                _mmu.Write16(address, StackPointer);
                break;
            case 0x09:
                Add16bitRegisterToHL(BC);
                break;
            case 0x0A:
                Accumulator = _mmu.Read8(BC);
                break;
            case 0x0B:
                BC--;
                break;
            case 0x0C:
                C = Increment8bitRegister(C);
                break;
            case 0x0D:
                C = Decrement8bitRegister(C);
                break;
            case 0x0E:
                C = _mmu.Read8(programCounter);
                break;
            case 0x0F:
                Accumulator = RotateByteRight(Accumulator);
                break;

            case 0x10:
                _running = false;
                break;
            case 0x11:
                DE = _mmu.Read16(programCounter);
                break;
            case 0x12:
                _mmu.Write8(DE, Accumulator);
                break;
            case 0x13:
                DE++;
                break;
            case 0x14:
                D = Increment8bitRegister(D);
                break;
            case 0x15:
                D = Decrement8bitRegister(D);
                break;
            case 0x16:
                D = _mmu.Read8(programCounter);
                break;
            case 0x17:
                Accumulator = RotateByteLeftThroughCarry(Accumulator);
                break;
            case 0x18:
                JumpRelative(programCounter);
                break;
            case 0x19:
                Add16bitRegisterToHL(DE);
                break;
            case 0x1A:
                Accumulator = _mmu.Read8(DE);
                break;
            case 0x1B:
                DE--;
                break;
            case 0x1C:
                E = Increment8bitRegister(E);
                break;
            case 0x1D:
                E = Decrement8bitRegister(E);
                break;
            case 0x1E:
                E = _mmu.Read8(programCounter);
                break;
            case 0x1F:
                Accumulator = RotateByteRightThroughCarry(Accumulator);
                break;

            case 0x20:
                if (ZeroFlag == false)
                {
                    JumpRelative(programCounter);
                }
                break;
            case 0x21:
                HL = _mmu.Read16(programCounter);
                break;
            case 0x22:
                _mmu.Write8(HL, Accumulator);
                HL++;
                break;
            case 0x23:
                HL++;
                break;
            case 0x24:
                H = Increment8bitRegister(H);
                break;
            case 0x25:
                H = Decrement8bitRegister(H);
                break;
            case 0x26:
                H = _mmu.Read8(programCounter);
                break;
            case 0x27:
                throw new NotImplementedException("DAA not implemented");
                break;
            case 0x28:
                if (ZeroFlag == true)
                {
                    JumpRelative(programCounter);
                }
                break;
            case 0x29:
                Add16bitRegisterToHL(HL);
                break;
            case 0x2A:
                Accumulator = _mmu.Read8(HL);
                HL++;
                break;
            case 0x2B:
                HL--;
                break;
            case 0x2C:
                L = Increment8bitRegister(L);
                break;
            case 0x2D:
                L = Decrement8bitRegister(L);
                break;
            case 0x2E:
                L = _mmu.Read8(programCounter);
                break;
            case 0x2F:
                Accumulator = (byte)~Accumulator;

                SubtractionFlag = true;
                HalfCarryFlag = true;
                break;

            case 0x30:
                if (CarryFlag == false)
                {
                    JumpRelative(programCounter);
                }
                break;
            case 0x31:
                StackPointer = _mmu.Read16(programCounter);
                break;
            case 0x32:
                Accumulator = _mmu.Read8(HL);
                HL--;
                break;
            case 0x33:
                StackPointer++;
                break;
            case 0x34:
                byte value = _mmu.Read8(HL);
                value = Increment8bitRegister(value);
                _mmu.Write8(HL, value);
                break;
            case 0x35:
                byte value2 = _mmu.Read8(HL);
                value = Decrement8bitRegister(value2);
                _mmu.Write8(HL, value);
                break;
            case 0x36:
                byte value3 = _mmu.Read8(HL);
                _mmu.Write8(HL, value3);
                break;
            case 0x37:
                SubtractionFlag = false;
                HalfCarryFlag = false;
                CarryFlag = true;
                break;
            case 0x38:
                if (CarryFlag == true)
                {
                    JumpRelative(programCounter);
                }
                break;
            case 0x39:
                Add16bitRegisterToHL(StackPointer);
                break;
            case 0x3A:
                Accumulator = _mmu.Read8(HL);
                HL--;
                break;
            case 0x3B:
                StackPointer--;
                break;
            case 0x3C:
                Accumulator = Increment8bitRegister(Accumulator);
                break;
            case 0x3D:
                Accumulator = Decrement8bitRegister(Accumulator);
                break;
            case 0x3E:
                Accumulator = _mmu.Read8(programCounter);
                break;
            case 0x3F:
                SubtractionFlag = false;
                HalfCarryFlag = false;
                CarryFlag = !CarryFlag;
                break;

            case 0x40:
                B = B; // This instruction seems useless
                break;
            case 0x41:
                B = C;
                break;
            case 0x42:
                B = D;
                break;
            case 0x43:
                B = E;
                break;
            case 0x44:
                B = H;
                break;
            case 0x45:
                B = L;
                break;
            case 0x46:
                B = HLValue;
                break;
            case 0x47:
                B = Accumulator;
                break;
            case 0x48:
                C = B;
                break;
            case 0x49:
                C = C; // This instruction seems useless
                break;
            case 0x4A:
                C = D;
                break;
            case 0x4B:
                C = E;
                break;
            case 0x4C:
                C = H;
                break;
            case 0x4D:
                C = L;
                break;
            case 0x4E:
                C = HLValue;
                break;
            case 0x4F:
                C = Accumulator;
                break;

            case 0x50:
                D = B;
                break;
            case 0x51:
                D = C;
                break;
            case 0x52:
                D = D; // This instruction seems useless
                break;
            case 0x53:
                D = E;
                break;
            case 0x54:
                D = H;
                break;
            case 0x55:
                D = L;
                break;
            case 0x56:
                D = HLValue;
                break;
            case 0x57:
                D = Accumulator;
                break;
            case 0x58:
                E = B;
                break;
            case 0x59:
                E = C;
                break;
            case 0x5A:
                E = D;
                break;
            case 0x5B:
                E = E; // This instruction seems useless
                break;
            case 0x5C:
                E = H;
                break;
            case 0x5D:
                E = L;
                break;
            case 0x5E:
                E = HLValue;
                break;
            case 0x5F:
                E = Accumulator;
                break;

            case 0x60:
                H = B;
                break;
            case 0x61:
                H = C;
                break;
            case 0x62:
                H = D;
                break;
            case 0x63:
                H = E;
                break;
            case 0x64:
                H = H; // This instruction seems useless
                break;
            case 0x65:
                H = L;
                break;
            case 0x66:
                H = HLValue;
                break;
            case 0x67:
                H = Accumulator;
                break;
            case 0x68:
                L = B;
                break;
            case 0x69:
                L = C;
                break;
            case 0x6A:
                L = D;
                break;
            case 0x6B:
                L = E;
                break;
            case 0x6C:
                L = H;
                break;
            case 0x6D:
                L = L; // This instruction seems useless
                break;
            case 0x6E:
                L = HLValue;
                break;
            case 0x6F:
                L = Accumulator;
                break;

            case 0x70:
                WriteToAddressInHL(B);
                break;
            case 0x71:
                WriteToAddressInHL(C);
                break;
            case 0x72:
                WriteToAddressInHL(D);
                break;
            case 0x73:
                WriteToAddressInHL(E);
                break;
            case 0x74:
                WriteToAddressInHL(H);
                break;
            case 0x75:
                WriteToAddressInHL(L);
                break;
            case 0x76:
                throw new NotImplementedException("HALT not implemented");
                break;
            case 0x77:
                WriteToAddressInHL(Accumulator);
                break;
            case 0x78:
                Accumulator = B;
                break;
            case 0x79:
                Accumulator = C;
                break;
            case 0x7A:
                Accumulator = D;
                break;
            case 0x7B:
                Accumulator = E;
                break;
            case 0x7C:
                Accumulator = H;
                break;
            case 0x7D:
                Accumulator = L;
                break;
            case 0x7E:
                Accumulator = HLValue;
                break;
            case 0x7F:
                Accumulator = Accumulator; // This instruction seems useless
                break;

            case 0x80:
                Add8bitRegisterToAccumulator(B);
                break;
            case 0x81:
                Add8bitRegisterToAccumulator(C);
                break;
            case 0x82:
                Add8bitRegisterToAccumulator(D);
                break;
            case 0x83:
                Add8bitRegisterToAccumulator(E);
                break;
            case 0x84:
                Add8bitRegisterToAccumulator(H);
                break;
            case 0x85:
                Add8bitRegisterToAccumulator(L);
                break;
            case 0x86:
                Add8bitRegisterToAccumulator(HLValue);
                break;
            case 0x87:
                Add8bitRegisterToAccumulator(Accumulator);
                break;
            case 0x88:
                Add8bitRegisterAndCarryToAccumulator(B);
                break;
            case 0x89:
                Add8bitRegisterAndCarryToAccumulator(C);
                break;
            case 0x8A:
                Add8bitRegisterAndCarryToAccumulator(D);
                break;
            case 0x8B:
                Add8bitRegisterAndCarryToAccumulator(E);
                break;
            case 0x8C:
                Add8bitRegisterAndCarryToAccumulator(H);
                break;
            case 0x8D:
                Add8bitRegisterAndCarryToAccumulator(L);
                break;
            case 0x8E:
                Add8bitRegisterAndCarryToAccumulator(HLValue);
                break;
            case 0x8F:
                Add8bitRegisterAndCarryToAccumulator(Accumulator);
                break;

            case 0x90:
                Sub8bitRegisterFromAccumulator(B);
                break;
            case 0x91:
                Sub8bitRegisterFromAccumulator(C);
                break;
            case 0x92:
                Sub8bitRegisterFromAccumulator(D);
                break;
            case 0x93:
                Sub8bitRegisterFromAccumulator(E);
                break;
            case 0x94:
                Sub8bitRegisterFromAccumulator(H);
                break;
            case 0x95:
                Sub8bitRegisterFromAccumulator(L);
                break;
            case 0x96:
                Sub8bitRegisterFromAccumulator(HLValue);
                break;
            case 0x97:
                Sub8bitRegisterFromAccumulator(Accumulator);
                break;
            case 0x98:
                Sub8bitRegisterAndCarryFromAccumulator(B);
                break;
            case 0x99:
                Sub8bitRegisterAndCarryFromAccumulator(C);
                break;
            case 0x9A:
                Sub8bitRegisterAndCarryFromAccumulator(D);
                break;
            case 0x9B:
                Sub8bitRegisterAndCarryFromAccumulator(E);
                break;
            case 0x9C:
                Sub8bitRegisterAndCarryFromAccumulator(H);
                break;
            case 0x9D:
                Sub8bitRegisterAndCarryFromAccumulator(L);
                break;
            case 0x9E:
                Sub8bitRegisterAndCarryFromAccumulator(HLValue);
                break;
            case 0x9F:
                Sub8bitRegisterAndCarryFromAccumulator(Accumulator);
                break;

            case 0xA0:
                And8bitRegisterWithAccumulator(B);
                break;
            case 0xA1:
                And8bitRegisterWithAccumulator(C);
                break;
            case 0xA2:
                And8bitRegisterWithAccumulator(D);
                break;
            case 0xA3:
                And8bitRegisterWithAccumulator(E);
                break;
            case 0xA4:
                And8bitRegisterWithAccumulator(H);
                break;
            case 0xA5:
                And8bitRegisterWithAccumulator(L);
                break;
            case 0xA6:
                And8bitRegisterWithAccumulator(HLValue);
                break;
            case 0xA7:
                And8bitRegisterWithAccumulator(Accumulator);
                break;
            case 0xA8:
                Xor8bitRegisterWithAccumulator(B);
                break;
            case 0xA9:
                Xor8bitRegisterWithAccumulator(C);
                break;
            case 0xAA:
                Xor8bitRegisterWithAccumulator(D);
                break;
            case 0xAB:
                Xor8bitRegisterWithAccumulator(E);
                break;
            case 0xAC:
                Xor8bitRegisterWithAccumulator(H);
                break;
            case 0xAD:
                Xor8bitRegisterWithAccumulator(L);
                break;
            case 0xAE:
                Xor8bitRegisterWithAccumulator(HLValue);
                break;
            case 0xAF:
                Xor8bitRegisterWithAccumulator(Accumulator);
                break;

            case 0xB0:
                Or8bitRegisterWithAccumulator(B);
                break;
            case 0xB1:
                Or8bitRegisterWithAccumulator(C);
                break;
            case 0xB2:
                Or8bitRegisterWithAccumulator(D);
                break;
            case 0xB3:
                Or8bitRegisterWithAccumulator(E);
                break;
            case 0xB4:
                Or8bitRegisterWithAccumulator(H);
                break;
            case 0xB5:
                Or8bitRegisterWithAccumulator(L);
                break;
            case 0xB6:
                Or8bitRegisterWithAccumulator(HLValue);
                break;
            case 0xB7:
                Or8bitRegisterWithAccumulator(Accumulator);
                break;
            case 0xB8:
                Compare8bitRegisterWithAccumulator(B);
                break;
            case 0xB9:
                Compare8bitRegisterWithAccumulator(C);
                break;
            case 0xBA:
                Compare8bitRegisterWithAccumulator(D);
                break;
            case 0xBB:
                Compare8bitRegisterWithAccumulator(E);
                break;
            case 0xBC:
                Compare8bitRegisterWithAccumulator(H);
                break;
            case 0xBD:
                Compare8bitRegisterWithAccumulator(L);
                break;
            case 0xBE:
                Compare8bitRegisterWithAccumulator(HLValue);
                break;
            case 0xBF:
                Compare8bitRegisterWithAccumulator(Accumulator);
                break;

            case 0xC0:
                break;
            case 0xC1:
                break;
            case 0xC2:
                break;
            case 0xC3:
                break;
            case 0xC4:
                break;
            case 0xC5:
                break;
            case 0xC6:
                break;
            case 0xC7:
                break;
            case 0xC8:
                break;
            case 0xC9:
                break;
            case 0xCA:
                break;
            case 0xCB:
                break;
            case 0xCC:
                break;
            case 0xCD:
                break;
            case 0xCE:
                break;
            case 0xCF:
                break;

            case 0xD0:
                break;
            case 0xD1:
                break;
            case 0xD2:
                break;
            case 0xD3:
                break;
            case 0xD4:
                break;
            case 0xD5:
                break;
            case 0xD6:
                break;
            case 0xD7:
                break;
            case 0xD8:
                break;
            case 0xD9:
                break;
            case 0xDA:
                break;
            case 0xDB:
                break;
            case 0xDC:
                break;
            case 0xDD:
                break;
            case 0xDE:
                break;
            case 0xDF:
                break;

            case 0xE0:
                break;
            case 0xE1:
                break;
            case 0xE2:
                break;
            case 0xE3:
                break;
            case 0xE4:
                break;
            case 0xE5:
                break;
            case 0xE6:
                break;
            case 0xE7:
                break;
            case 0xE8:
                break;
            case 0xE9:
                break;
            case 0xEA:
                break;
            case 0xEB:
                break;
            case 0xEC:
                break;
            case 0xED:
                break;
            case 0xEE:
                break;
            case 0xEF:
                break;

            case 0xF0:
                break;
            case 0xF1:
                break;
            case 0xF2:
                break;
            case 0xF3:
                break;
            case 0xF4:
                break;
            case 0xF5:
                break;
            case 0xF6:
                break;
            case 0xF7:
                break;
            case 0xF8:
                break;
            case 0xF9:
                break;
            case 0xFA:
                break;
            case 0xFB:
                break;
            case 0xFC:
                break;
            case 0xFD:
                break;
            case 0xFE:
                break;
            case 0xFF:
                break;

            default:
                throw new Exception($"Invalid opcode: 0x{instructionCode:X2}");
        }
    }

}