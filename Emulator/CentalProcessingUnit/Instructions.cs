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
        value2 &= 0xFFF;
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

    private void Add8bitRegisterToAccumulator(byte value)
    {
        int result = Accumulator + value;

        ZeroFlag = ((byte)result) == 0;
        SubtractionFlag = false;
        CarryFlag = result > 0xFF;
        HalfCarryFlag = CheckHalfCarryAddition(Accumulator, value);

        Accumulator = (byte)result;
    }

    private void Add8bitRegisterAndCarryToAccumulator(byte value)
    {
        int result = Accumulator + value + (CarryFlag ? 1 : 0);

        ZeroFlag = ((byte)result) == 0;
        SubtractionFlag = false;
        CarryFlag = result > 0xFF;
        HalfCarryFlag = CheckHalfCarryAddition(Accumulator, value, CarryFlag);

        Accumulator = (byte)result; // numbers greater than 255 overflow when cast to byte
    }

    private void Sub8bitRegisterFromAccumulator(byte value)
    {
        int result = Accumulator - value;

        ZeroFlag = ((byte)result) == 0;
        SubtractionFlag = true;
        CarryFlag = result < 0;
        HalfCarryFlag = CheckHalfCarrySubtraction(Accumulator, value);

        Accumulator = (byte)result; // negative numbers underflow when cast to byte
    }

    private void Sub8bitRegisterAndCarryFromAccumulator(byte value)
    {
        int result = Accumulator - value - (CarryFlag ? 1 : 0);

        ZeroFlag = ((byte)result) == 0;
        SubtractionFlag = true;
        CarryFlag = result < 0;
        HalfCarryFlag = CheckHalfCarrySubtraction(Accumulator, value, CarryFlag);

        Accumulator = (byte)result; // negative numbers underflow when cast to byte
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
        Accumulator = (byte)(Accumulator | value);

        ZeroFlag = Accumulator == 0;
        SubtractionFlag = false;
        CarryFlag = false;
        HalfCarryFlag = false;
    }

    // The Rotate instruction mnemonics dont make sense to me and seem flipped
    // RLC r8 & RRC r8 rotates only the byte. Acts like rotating 8 bits and also saving the overflowing bit in the Carry flag.  
    // They corrospond to the RotateByte functions

    // RL r8 & RR r8 rotates the byte through the Carry flag. Acts like rotating 9 bits with the carry flag at the end of the rotate direction. 
    // They corrospond to the RotateByteThroughCarry functions
    private byte RotateByteLeft(byte value, bool checkZeroFlag)
    {
        byte result = value.RotateByteLeft();

        ZeroFlag = checkZeroFlag == true && result == 0;
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = result.GetBit(0); // old bit 7, now at bit 0

        return result;
    }

    private byte RotateByteLeftThroughCarry(byte value, bool checkZeroFlag)
    {
        bool oldCarry = CarryFlag;

        byte result = RotateByteLeft(value, checkZeroFlag);
        result = result.SetBit(0, oldCarry);

        ZeroFlag = checkZeroFlag == true && result == 0;

        return result;
    }

    private byte RotateByteRight(byte value, bool checkZeroFlag)
    {
        byte result = value.RotateByteRight();

        ZeroFlag = checkZeroFlag == true && result == 0;
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = result.GetBit(7); // old bit 0, now at bit 7

        return result;
    }

    private byte RotateByteRightThroughCarry(byte value, bool checkZeroFlag)
    {
        bool oldCarry = CarryFlag;

        byte result = RotateByteRight(value, checkZeroFlag);
        result = result.SetBit(7, oldCarry);

        ZeroFlag = checkZeroFlag == true && result == 0;

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

    private void JumpRelative(bool condition)
    {
        byte next = GetNext8Bits();
        if (condition == true)
        {
            instructionConditionMet = true;
            sbyte amount = (sbyte)next;
            ProgramCounter = (ushort)(ProgramCounter + amount);
        }
    }

    private void PushStack(ushort value)
    {
        StackPointer -= 2;
        _mmu.Write16(StackPointer, value);
    }

    private ushort PopStack()
    {
        ushort result = _mmu.Read16(StackPointer);
        StackPointer += 2;

        return result;
    }

    private void Jump(ushort address, bool condition)
    {
        if (condition == true)
        {
            instructionConditionMet = true;
            ProgramCounter = address;
        }
    }

    private void CallSubroutine(ushort address, bool condition)
    {
        if (condition == true)
        {
            instructionConditionMet = true;
            PushStack(ProgramCounter);
            Jump(address, true);
        }
    }

    private void ReturnFromSubroutine(bool condition)
    {
        if (condition == true)
        {
            instructionConditionMet = true;
            ushort address = PopStack();
            ProgramCounter = address;
        }
    }

    private ushort AddSignedByteToStackPointer(byte value)
    {
        sbyte signedValue = (sbyte)value;
        int result = StackPointer + signedValue;

        ZeroFlag = false;
        SubtractionFlag = false;
        CarryFlag = result > 0xFFFF || result < 0;

        if (signedValue >= 0)
        {
            HalfCarryFlag = CheckHalfCarryAddition((byte)StackPointer, (byte)signedValue);
        }
        else
        {
            HalfCarryFlag = CheckHalfCarrySubtraction((byte)StackPointer, (byte)-signedValue);
        }

        return (ushort)result;
    }

    private void DecimalAdjustAccumulator()
    {
        byte adjustment = 0;

        if (SubtractionFlag == true)
        {
            if (HalfCarryFlag == true)
            {
                adjustment += 0x06;
            }

            if (CarryFlag == true)
            {
                adjustment += 0x60;
            }

            Accumulator -= adjustment;
        }
        else
        {
            if (HalfCarryFlag == true || (Accumulator & 0x0F) > 0x09)
            {
                adjustment += 0x06;
            }

            if (CarryFlag == true || Accumulator > 0x99)
            {
                adjustment += 0x60;
                CarryFlag = true;
            }

            Accumulator += adjustment;
        }

        ZeroFlag = Accumulator == 0;
        HalfCarryFlag = false;
    }

    private byte HLValue
    {
        get => _mmu.Read8(HL);
        set => _mmu.Write8(HL, value);
    }

    private void ExecuteInstruction(byte instructionCode)
    {
        ushort highRAMAddress = 0xFF00;

        switch (instructionCode)
        {
            case 0x00:
                // No Operation
                break;
            case 0x01:
                BC = GetNext16Bits();
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
                B = GetNext8Bits();
                break;
            case 0x07:
                Accumulator = RotateByteLeft(Accumulator, false);
                break;
            case 0x08:
                _mmu.Write16(GetNext16Bits(), StackPointer);
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
                C = GetNext8Bits();
                break;
            case 0x0F:
                Accumulator = RotateByteRight(Accumulator, false);
                break;

            case 0x10:
                Running = false;
                break;
            case 0x11:
                DE = GetNext16Bits();
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
                D = GetNext8Bits();
                break;
            case 0x17:
                Accumulator = RotateByteLeftThroughCarry(Accumulator, false);
                break;
            case 0x18:
                JumpRelative(true);
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
                E = GetNext8Bits();
                break;
            case 0x1F:
                Accumulator = RotateByteRightThroughCarry(Accumulator, false);
                break;

            case 0x20:
                JumpRelative(ZeroFlag == false);
                break;
            case 0x21:
                HL = GetNext16Bits();
                break;
            case 0x22:
                HLValue = Accumulator;
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
                H = GetNext8Bits();
                break;
            case 0x27:
                DecimalAdjustAccumulator();
                break;
            case 0x28:
                JumpRelative(ZeroFlag == true);
                break;
            case 0x29:
                Add16bitRegisterToHL(HL);
                break;
            case 0x2A:
                Accumulator = HLValue;
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
                L = GetNext8Bits();
                break;
            case 0x2F:
                Accumulator = (byte)~Accumulator;

                SubtractionFlag = true;
                HalfCarryFlag = true;
                break;

            case 0x30:
                JumpRelative(CarryFlag == false);
                break;
            case 0x31:
                StackPointer = GetNext16Bits();
                break;
            case 0x32:
                HLValue = Accumulator;
                HL--;
                break;
            case 0x33:
                StackPointer++;
                break;
            case 0x34:
                HLValue = Increment8bitRegister(HLValue);
                break;
            case 0x35:
                HLValue = Decrement8bitRegister(HLValue);
                break;
            case 0x36:
                HLValue = GetNext8Bits();
                break;
            case 0x37:
                SubtractionFlag = false;
                HalfCarryFlag = false;
                CarryFlag = true;
                break;
            case 0x38:
                JumpRelative(CarryFlag == true);
                break;
            case 0x39:
                Add16bitRegisterToHL(StackPointer);
                break;
            case 0x3A:
                Accumulator = HLValue;
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
                Accumulator = GetNext8Bits();
                break;
            case 0x3F:
                SubtractionFlag = false;
                HalfCarryFlag = false;
                CarryFlag = !CarryFlag;
                break;

            case 0x40:
                B = B; // Does nothing and can be treated as a no-op. Though some emulators use them for debugging
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
                C = C; // Does nothing and can be treated as a no-op. Though some emulators use them for debugging
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
                D = D; // Does nothing and can be treated as a no-op. Though some emulators use them for debugging
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
                E = E; // Does nothing and can be treated as a no-op. Though some emulators use them for debugging
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
                H = H; // Does nothing and can be treated as a no-op. Though some emulators use them for debugging
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
                L = L; // Does nothing and can be treated as a no-op. Though some emulators use them for debugging
                break;
            case 0x6E:
                L = HLValue;
                break;
            case 0x6F:
                L = Accumulator;
                break;

            case 0x70:
                HLValue = B;
                break;
            case 0x71:
                HLValue = C;
                break;
            case 0x72:
                HLValue = D;
                break;
            case 0x73:
                HLValue = E;
                break;
            case 0x74:
                HLValue = H;
                break;
            case 0x75:
                HLValue = L;
                break;
            case 0x76:
                throw new NotImplementedException("HALT not implemented");
                break;
            case 0x77:
                HLValue = Accumulator;
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
                Accumulator = Accumulator; // Does nothing and can be treated as a no-op. Though some emulators use them for debugging
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
                ReturnFromSubroutine(ZeroFlag == false);
                break;
            case 0xC1:
                BC = PopStack();
                break;
            case 0xC2:
                Jump(GetNext16Bits(), ZeroFlag == false);
                break;
            case 0xC3:
                Jump(GetNext16Bits(), true);
                break;
            case 0xC4:
                CallSubroutine(GetNext16Bits(), ZeroFlag == false);
                break;
            case 0xC5:
                PushStack(BC);
                break;
            case 0xC6:
                Add8bitRegisterToAccumulator(GetNext8Bits());
                break;
            case 0xC7:
                CallSubroutine(0x00, true);
                break;
            case 0xC8:
                ReturnFromSubroutine(ZeroFlag == true);
                break;
            case 0xC9:
                ReturnFromSubroutine(true);
                break;
            case 0xCA:
                Jump(GetNext16Bits(), ZeroFlag == true);
                break;
            case 0xCB:
                ExecuteCBInstruction(GetNext8Bits());
                break;
            case 0xCC:
                CallSubroutine(GetNext16Bits(), ZeroFlag == true);
                break;
            case 0xCD:
                CallSubroutine(GetNext16Bits(), true);
                break;
            case 0xCE:
                Add8bitRegisterAndCarryToAccumulator(GetNext8Bits());
                break;
            case 0xCF:
                CallSubroutine(0x08, true);
                break;

            case 0xD0:
                ReturnFromSubroutine(CarryFlag == false);
                break;
            case 0xD1:
                DE = PopStack();
                break;
            case 0xD2:
                Jump(GetNext16Bits(), CarryFlag == false);
                break;
            case 0xD3:
                throw new Exception("Unused instruction");
            case 0xD4:
                CallSubroutine(GetNext16Bits(), CarryFlag == false);
                break;
            case 0xD5:
                PushStack(DE);
                break;
            case 0xD6:
                Sub8bitRegisterFromAccumulator(GetNext8Bits());
                break;
            case 0xD7:
                CallSubroutine(0x10, true);
                break;
            case 0xD8:
                ReturnFromSubroutine(CarryFlag == true);
                break;
            case 0xD9:
                InterruptMasterEnable = true;
                ReturnFromSubroutine(true);
                break;
            case 0xDA:
                Jump(GetNext16Bits(), CarryFlag == true);
                break;
            case 0xDB:
                throw new Exception("Unused instruction");
            case 0xDC:
                CallSubroutine(GetNext16Bits(), CarryFlag == true);
                break;
            case 0xDD:
                throw new Exception("Unused instruction");
            case 0xDE:
                Sub8bitRegisterAndCarryFromAccumulator(GetNext8Bits());
                break;
            case 0xDF:
                CallSubroutine(0x18, true);
                break;

            case 0xE0:
                _mmu.Write8((ushort)(highRAMAddress + GetNext8Bits()), Accumulator);
                break;
            case 0xE1:
                HL = PopStack();
                break;
            case 0xE2:
                _mmu.Write8((ushort)(highRAMAddress + C), Accumulator);
                break;
            case 0xE3:
                throw new Exception("Unused instruction");
            case 0xE4:
                throw new Exception("Unused instruction");
            case 0xE5:
                PushStack(HL);
                break;
            case 0xE6:
                And8bitRegisterWithAccumulator(GetNext8Bits());
                break;
            case 0xE7:
                CallSubroutine(0x20, true);
                break;
            case 0xE8:
                StackPointer = AddSignedByteToStackPointer(GetNext8Bits());
                break;
            case 0xE9:
                Jump(HL, true);
                break;
            case 0xEA:
                _mmu.Write8(GetNext16Bits(), Accumulator);
                break;
            case 0xEB:
                throw new Exception("Unused instruction");
            case 0xEC:
                throw new Exception("Unused instruction");
            case 0xED:
                throw new Exception("Unused instruction");
            case 0xEE:
                Xor8bitRegisterWithAccumulator(GetNext8Bits());
                break;
            case 0xEF:
                CallSubroutine(0x28, true);
                break;

            case 0xF0:
                Accumulator = _mmu.Read8((ushort)(highRAMAddress + GetNext8Bits()));
                break;
            case 0xF1:
                AccumulatorFlags = PopStack();
                break;
            case 0xF2:
                Accumulator = _mmu.Read8((ushort)(highRAMAddress + C));
                break;
            case 0xF3:
                InterruptMasterEnable = false;
                break;
            case 0xF4:
                throw new Exception("Unused instruction");
            case 0xF5:
                PushStack(AccumulatorFlags);
                break;
            case 0xF6:
                Or8bitRegisterWithAccumulator(GetNext8Bits());
                break;
            case 0xF7:
                CallSubroutine(0x30, true);
                break;
            case 0xF8:
                HL = AddSignedByteToStackPointer(GetNext8Bits());
                break;
            case 0xF9:
                StackPointer = HL;
                break;
            case 0xFA:
                Accumulator = _mmu.Read8(GetNext16Bits());
                break;
            case 0xFB:
                QueueInterruptMasterEnableSet = true;
                break;
            case 0xFC:
                throw new Exception("Unused instruction");
            case 0xFD:
                throw new Exception("Unused instruction");
            case 0xFE:
                Compare8bitRegisterWithAccumulator(GetNext8Bits());
                break;
            case 0xFF:
                CallSubroutine(0x38, true);
                break;

            default:
                throw new Exception($"Invalid opcode: 0x{instructionCode:X}");
        }
    }

    private byte ShiftByteLeft(byte value)
    {
        bool oldBit7 = value.GetBit(7);
        value <<= 1;

        ZeroFlag = value == 0;
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = oldBit7;

        return value;
    }

    private byte ShiftByteRight(byte value)
    {
        bool oldBit0 = value.GetBit(0);
        value >>= 1;

        ZeroFlag = value == 0;
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = oldBit0;

        return value;
    }

    private byte SwapNibbles(byte value)
    {
        byte bottomNibble = (byte)(value & 0x0F);
        bottomNibble <<= 4;
        value >>= 4;
        value += bottomNibble;

        ZeroFlag = value == 0;
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = false;

        return value;
    }

    private byte ShiftByteRightArithmically(byte value)
    {
        bool oldBit7 = value.GetBit(7);
        bool oldBit0 = value.GetBit(0);
        value >>= 1;
        value = value.SetBit(7, oldBit7);

        ZeroFlag = value == 0;
        SubtractionFlag = false;
        HalfCarryFlag = false;
        CarryFlag = oldBit0;

        return value;
    }

    private void CheckBit(byte value, byte place)
    {
        ZeroFlag = value.GetBit(place) == false;
        SubtractionFlag = false;
        HalfCarryFlag = true;
    }

    // The CB instructions were very repetitive and were therefor not made by hand but instead by this: 
    /*
    StreamWriter outputFile = new StreamWriter("output.txt");

    for (int i = 0; i <= 0xFF; i++)
    {
        string register = (i % 8) switch
        {
            0 => "B",
            1 => "C",
            2 => "D",
            3 => "E",
            4 => "H",
            5 => "L",
            6 => "HLValue",
            7 => "Accumulator",
            _ => "ERROR"
        };

        string instruction = i switch
        {
            <= 0x07 => $"{register} = RotateByteLeft({register}, true);",
            <= 0x0F => $"{register} = RotateByteRight({register}, true);",
            <= 0x17 => $"{register} = RotateByteLeftThroughCarry({register}, true);",
            <= 0x1F => $"{register} = RotateByteRightThroughCarry({register}, true);",
            <= 0x27 => $"{register} = ShiftByteLeft({register});",
            <= 0x2F => $"{register} = ShiftByteRightArithmically({register});",
            <= 0x37 => $"{register} = SwapNibbles({register});",
            <= 0x3F => $"{register} = ShiftByteRight({register});",
            <= 0x7F => $"CheckBit({register}, {(i / 8) % 8});",
            <= 0xBF => $"{register} = {register}.SetBit({(i / 8) % 8}, false);",
            <= 0xFF => $"{register} = {register}.SetBit({(i / 8) % 8}, true);",
            _ => "",
        };

        outputFile.WriteLine($"case 0x{i:X2}: ");
        outputFile.WriteLine(instruction);

        outputFile.WriteLine("break;");

        if (i % 8 == 7)
        {
            outputFile.WriteLine();
        }
    }

    outputFile.Close();
    */

    private void ExecuteCBInstruction(byte instructionCode)
    {
        switch (instructionCode)
        {
            case 0x00:
                B = RotateByteLeft(B, true);
                break;
            case 0x01:
                C = RotateByteLeft(C, true);
                break;
            case 0x02:
                D = RotateByteLeft(D, true);
                break;
            case 0x03:
                E = RotateByteLeft(E, true);
                break;
            case 0x04:
                H = RotateByteLeft(H, true);
                break;
            case 0x05:
                L = RotateByteLeft(L, true);
                break;
            case 0x06:
                HLValue = RotateByteLeft(HLValue, true);
                break;
            case 0x07:
                Accumulator = RotateByteLeft(Accumulator, true);
                break;

            case 0x08:
                B = RotateByteRight(B, true);
                break;
            case 0x09:
                C = RotateByteRight(C, true);
                break;
            case 0x0A:
                D = RotateByteRight(D, true);
                break;
            case 0x0B:
                E = RotateByteRight(E, true);
                break;
            case 0x0C:
                H = RotateByteRight(H, true);
                break;
            case 0x0D:
                L = RotateByteRight(L, true);
                break;
            case 0x0E:
                HLValue = RotateByteRight(HLValue, true);
                break;
            case 0x0F:
                Accumulator = RotateByteRight(Accumulator, true);
                break;

            case 0x10:
                B = RotateByteLeftThroughCarry(B, true);
                break;
            case 0x11:
                C = RotateByteLeftThroughCarry(C, true);
                break;
            case 0x12:
                D = RotateByteLeftThroughCarry(D, true);
                break;
            case 0x13:
                E = RotateByteLeftThroughCarry(E, true);
                break;
            case 0x14:
                H = RotateByteLeftThroughCarry(H, true);
                break;
            case 0x15:
                L = RotateByteLeftThroughCarry(L, true);
                break;
            case 0x16:
                HLValue = RotateByteLeftThroughCarry(HLValue, true);
                break;
            case 0x17:
                Accumulator = RotateByteLeftThroughCarry(Accumulator, true);
                break;

            case 0x18:
                B = RotateByteRightThroughCarry(B, true);
                break;
            case 0x19:
                C = RotateByteRightThroughCarry(C, true);
                break;
            case 0x1A:
                D = RotateByteRightThroughCarry(D, true);
                break;
            case 0x1B:
                E = RotateByteRightThroughCarry(E, true);
                break;
            case 0x1C:
                H = RotateByteRightThroughCarry(H, true);
                break;
            case 0x1D:
                L = RotateByteRightThroughCarry(L, true);
                break;
            case 0x1E:
                HLValue = RotateByteRightThroughCarry(HLValue, true);
                break;
            case 0x1F:
                Accumulator = RotateByteRightThroughCarry(Accumulator, true);
                break;

            case 0x20:
                B = ShiftByteLeft(B);
                break;
            case 0x21:
                C = ShiftByteLeft(C);
                break;
            case 0x22:
                D = ShiftByteLeft(D);
                break;
            case 0x23:
                E = ShiftByteLeft(E);
                break;
            case 0x24:
                H = ShiftByteLeft(H);
                break;
            case 0x25:
                L = ShiftByteLeft(L);
                break;
            case 0x26:
                HLValue = ShiftByteLeft(HLValue);
                break;
            case 0x27:
                Accumulator = ShiftByteLeft(Accumulator);
                break;

            case 0x28:
                B = ShiftByteRightArithmically(B);
                break;
            case 0x29:
                C = ShiftByteRightArithmically(C);
                break;
            case 0x2A:
                D = ShiftByteRightArithmically(D);
                break;
            case 0x2B:
                E = ShiftByteRightArithmically(E);
                break;
            case 0x2C:
                H = ShiftByteRightArithmically(H);
                break;
            case 0x2D:
                L = ShiftByteRightArithmically(L);
                break;
            case 0x2E:
                HLValue = ShiftByteRightArithmically(HLValue);
                break;
            case 0x2F:
                Accumulator = ShiftByteRightArithmically(Accumulator);
                break;

            case 0x30:
                B = SwapNibbles(B);
                break;
            case 0x31:
                C = SwapNibbles(C);
                break;
            case 0x32:
                D = SwapNibbles(D);
                break;
            case 0x33:
                E = SwapNibbles(E);
                break;
            case 0x34:
                H = SwapNibbles(H);
                break;
            case 0x35:
                L = SwapNibbles(L);
                break;
            case 0x36:
                HLValue = SwapNibbles(HLValue);
                break;
            case 0x37:
                Accumulator = SwapNibbles(Accumulator);
                break;

            case 0x38:
                B = ShiftByteRight(B);
                break;
            case 0x39:
                C = ShiftByteRight(C);
                break;
            case 0x3A:
                D = ShiftByteRight(D);
                break;
            case 0x3B:
                E = ShiftByteRight(E);
                break;
            case 0x3C:
                H = ShiftByteRight(H);
                break;
            case 0x3D:
                L = ShiftByteRight(L);
                break;
            case 0x3E:
                HLValue = ShiftByteRight(HLValue);
                break;
            case 0x3F:
                Accumulator = ShiftByteRight(Accumulator);
                break;

            case 0x40:
                CheckBit(B, 0);
                break;
            case 0x41:
                CheckBit(C, 0);
                break;
            case 0x42:
                CheckBit(D, 0);
                break;
            case 0x43:
                CheckBit(E, 0);
                break;
            case 0x44:
                CheckBit(H, 0);
                break;
            case 0x45:
                CheckBit(L, 0);
                break;
            case 0x46:
                CheckBit(HLValue, 0);
                break;
            case 0x47:
                CheckBit(Accumulator, 0);
                break;

            case 0x48:
                CheckBit(B, 1);
                break;
            case 0x49:
                CheckBit(C, 1);
                break;
            case 0x4A:
                CheckBit(D, 1);
                break;
            case 0x4B:
                CheckBit(E, 1);
                break;
            case 0x4C:
                CheckBit(H, 1);
                break;
            case 0x4D:
                CheckBit(L, 1);
                break;
            case 0x4E:
                CheckBit(HLValue, 1);
                break;
            case 0x4F:
                CheckBit(Accumulator, 1);
                break;

            case 0x50:
                CheckBit(B, 2);
                break;
            case 0x51:
                CheckBit(C, 2);
                break;
            case 0x52:
                CheckBit(D, 2);
                break;
            case 0x53:
                CheckBit(E, 2);
                break;
            case 0x54:
                CheckBit(H, 2);
                break;
            case 0x55:
                CheckBit(L, 2);
                break;
            case 0x56:
                CheckBit(HLValue, 2);
                break;
            case 0x57:
                CheckBit(Accumulator, 2);
                break;

            case 0x58:
                CheckBit(B, 3);
                break;
            case 0x59:
                CheckBit(C, 3);
                break;
            case 0x5A:
                CheckBit(D, 3);
                break;
            case 0x5B:
                CheckBit(E, 3);
                break;
            case 0x5C:
                CheckBit(H, 3);
                break;
            case 0x5D:
                CheckBit(L, 3);
                break;
            case 0x5E:
                CheckBit(HLValue, 3);
                break;
            case 0x5F:
                CheckBit(Accumulator, 3);
                break;

            case 0x60:
                CheckBit(B, 4);
                break;
            case 0x61:
                CheckBit(C, 4);
                break;
            case 0x62:
                CheckBit(D, 4);
                break;
            case 0x63:
                CheckBit(E, 4);
                break;
            case 0x64:
                CheckBit(H, 4);
                break;
            case 0x65:
                CheckBit(L, 4);
                break;
            case 0x66:
                CheckBit(HLValue, 4);
                break;
            case 0x67:
                CheckBit(Accumulator, 4);
                break;

            case 0x68:
                CheckBit(B, 5);
                break;
            case 0x69:
                CheckBit(C, 5);
                break;
            case 0x6A:
                CheckBit(D, 5);
                break;
            case 0x6B:
                CheckBit(E, 5);
                break;
            case 0x6C:
                CheckBit(H, 5);
                break;
            case 0x6D:
                CheckBit(L, 5);
                break;
            case 0x6E:
                CheckBit(HLValue, 5);
                break;
            case 0x6F:
                CheckBit(Accumulator, 5);
                break;

            case 0x70:
                CheckBit(B, 6);
                break;
            case 0x71:
                CheckBit(C, 6);
                break;
            case 0x72:
                CheckBit(D, 6);
                break;
            case 0x73:
                CheckBit(E, 6);
                break;
            case 0x74:
                CheckBit(H, 6);
                break;
            case 0x75:
                CheckBit(L, 6);
                break;
            case 0x76:
                CheckBit(HLValue, 6);
                break;
            case 0x77:
                CheckBit(Accumulator, 6);
                break;

            case 0x78:
                CheckBit(B, 7);
                break;
            case 0x79:
                CheckBit(C, 7);
                break;
            case 0x7A:
                CheckBit(D, 7);
                break;
            case 0x7B:
                CheckBit(E, 7);
                break;
            case 0x7C:
                CheckBit(H, 7);
                break;
            case 0x7D:
                CheckBit(L, 7);
                break;
            case 0x7E:
                CheckBit(HLValue, 7);
                break;
            case 0x7F:
                CheckBit(Accumulator, 7);
                break;

            case 0x80:
                B = B.SetBit(0, false);
                break;
            case 0x81:
                C = C.SetBit(0, false);
                break;
            case 0x82:
                D = D.SetBit(0, false);
                break;
            case 0x83:
                E = E.SetBit(0, false);
                break;
            case 0x84:
                H = H.SetBit(0, false);
                break;
            case 0x85:
                L = L.SetBit(0, false);
                break;
            case 0x86:
                HLValue = HLValue.SetBit(0, false);
                break;
            case 0x87:
                Accumulator = Accumulator.SetBit(0, false);
                break;

            case 0x88:
                B = B.SetBit(1, false);
                break;
            case 0x89:
                C = C.SetBit(1, false);
                break;
            case 0x8A:
                D = D.SetBit(1, false);
                break;
            case 0x8B:
                E = E.SetBit(1, false);
                break;
            case 0x8C:
                H = H.SetBit(1, false);
                break;
            case 0x8D:
                L = L.SetBit(1, false);
                break;
            case 0x8E:
                HLValue = HLValue.SetBit(1, false);
                break;
            case 0x8F:
                Accumulator = Accumulator.SetBit(1, false);
                break;

            case 0x90:
                B = B.SetBit(2, false);
                break;
            case 0x91:
                C = C.SetBit(2, false);
                break;
            case 0x92:
                D = D.SetBit(2, false);
                break;
            case 0x93:
                E = E.SetBit(2, false);
                break;
            case 0x94:
                H = H.SetBit(2, false);
                break;
            case 0x95:
                L = L.SetBit(2, false);
                break;
            case 0x96:
                HLValue = HLValue.SetBit(2, false);
                break;
            case 0x97:
                Accumulator = Accumulator.SetBit(2, false);
                break;

            case 0x98:
                B = B.SetBit(3, false);
                break;
            case 0x99:
                C = C.SetBit(3, false);
                break;
            case 0x9A:
                D = D.SetBit(3, false);
                break;
            case 0x9B:
                E = E.SetBit(3, false);
                break;
            case 0x9C:
                H = H.SetBit(3, false);
                break;
            case 0x9D:
                L = L.SetBit(3, false);
                break;
            case 0x9E:
                HLValue = HLValue.SetBit(3, false);
                break;
            case 0x9F:
                Accumulator = Accumulator.SetBit(3, false);
                break;

            case 0xA0:
                B = B.SetBit(4, false);
                break;
            case 0xA1:
                C = C.SetBit(4, false);
                break;
            case 0xA2:
                D = D.SetBit(4, false);
                break;
            case 0xA3:
                E = E.SetBit(4, false);
                break;
            case 0xA4:
                H = H.SetBit(4, false);
                break;
            case 0xA5:
                L = L.SetBit(4, false);
                break;
            case 0xA6:
                HLValue = HLValue.SetBit(4, false);
                break;
            case 0xA7:
                Accumulator = Accumulator.SetBit(4, false);
                break;

            case 0xA8:
                B = B.SetBit(5, false);
                break;
            case 0xA9:
                C = C.SetBit(5, false);
                break;
            case 0xAA:
                D = D.SetBit(5, false);
                break;
            case 0xAB:
                E = E.SetBit(5, false);
                break;
            case 0xAC:
                H = H.SetBit(5, false);
                break;
            case 0xAD:
                L = L.SetBit(5, false);
                break;
            case 0xAE:
                HLValue = HLValue.SetBit(5, false);
                break;
            case 0xAF:
                Accumulator = Accumulator.SetBit(5, false);
                break;

            case 0xB0:
                B = B.SetBit(6, false);
                break;
            case 0xB1:
                C = C.SetBit(6, false);
                break;
            case 0xB2:
                D = D.SetBit(6, false);
                break;
            case 0xB3:
                E = E.SetBit(6, false);
                break;
            case 0xB4:
                H = H.SetBit(6, false);
                break;
            case 0xB5:
                L = L.SetBit(6, false);
                break;
            case 0xB6:
                HLValue = HLValue.SetBit(6, false);
                break;
            case 0xB7:
                Accumulator = Accumulator.SetBit(6, false);
                break;

            case 0xB8:
                B = B.SetBit(7, false);
                break;
            case 0xB9:
                C = C.SetBit(7, false);
                break;
            case 0xBA:
                D = D.SetBit(7, false);
                break;
            case 0xBB:
                E = E.SetBit(7, false);
                break;
            case 0xBC:
                H = H.SetBit(7, false);
                break;
            case 0xBD:
                L = L.SetBit(7, false);
                break;
            case 0xBE:
                HLValue = HLValue.SetBit(7, false);
                break;
            case 0xBF:
                Accumulator = Accumulator.SetBit(7, false);
                break;

            case 0xC0:
                B = B.SetBit(0, true);
                break;
            case 0xC1:
                C = C.SetBit(0, true);
                break;
            case 0xC2:
                D = D.SetBit(0, true);
                break;
            case 0xC3:
                E = E.SetBit(0, true);
                break;
            case 0xC4:
                H = H.SetBit(0, true);
                break;
            case 0xC5:
                L = L.SetBit(0, true);
                break;
            case 0xC6:
                HLValue = HLValue.SetBit(0, true);
                break;
            case 0xC7:
                Accumulator = Accumulator.SetBit(0, true);
                break;

            case 0xC8:
                B = B.SetBit(1, true);
                break;
            case 0xC9:
                C = C.SetBit(1, true);
                break;
            case 0xCA:
                D = D.SetBit(1, true);
                break;
            case 0xCB:
                E = E.SetBit(1, true);
                break;
            case 0xCC:
                H = H.SetBit(1, true);
                break;
            case 0xCD:
                L = L.SetBit(1, true);
                break;
            case 0xCE:
                HLValue = HLValue.SetBit(1, true);
                break;
            case 0xCF:
                Accumulator = Accumulator.SetBit(1, true);
                break;

            case 0xD0:
                B = B.SetBit(2, true);
                break;
            case 0xD1:
                C = C.SetBit(2, true);
                break;
            case 0xD2:
                D = D.SetBit(2, true);
                break;
            case 0xD3:
                E = E.SetBit(2, true);
                break;
            case 0xD4:
                H = H.SetBit(2, true);
                break;
            case 0xD5:
                L = L.SetBit(2, true);
                break;
            case 0xD6:
                HLValue = HLValue.SetBit(2, true);
                break;
            case 0xD7:
                Accumulator = Accumulator.SetBit(2, true);
                break;

            case 0xD8:
                B = B.SetBit(3, true);
                break;
            case 0xD9:
                C = C.SetBit(3, true);
                break;
            case 0xDA:
                D = D.SetBit(3, true);
                break;
            case 0xDB:
                E = E.SetBit(3, true);
                break;
            case 0xDC:
                H = H.SetBit(3, true);
                break;
            case 0xDD:
                L = L.SetBit(3, true);
                break;
            case 0xDE:
                HLValue = HLValue.SetBit(3, true);
                break;
            case 0xDF:
                Accumulator = Accumulator.SetBit(3, true);
                break;

            case 0xE0:
                B = B.SetBit(4, true);
                break;
            case 0xE1:
                C = C.SetBit(4, true);
                break;
            case 0xE2:
                D = D.SetBit(4, true);
                break;
            case 0xE3:
                E = E.SetBit(4, true);
                break;
            case 0xE4:
                H = H.SetBit(4, true);
                break;
            case 0xE5:
                L = L.SetBit(4, true);
                break;
            case 0xE6:
                HLValue = HLValue.SetBit(4, true);
                break;
            case 0xE7:
                Accumulator = Accumulator.SetBit(4, true);
                break;

            case 0xE8:
                B = B.SetBit(5, true);
                break;
            case 0xE9:
                C = C.SetBit(5, true);
                break;
            case 0xEA:
                D = D.SetBit(5, true);
                break;
            case 0xEB:
                E = E.SetBit(5, true);
                break;
            case 0xEC:
                H = H.SetBit(5, true);
                break;
            case 0xED:
                L = L.SetBit(5, true);
                break;
            case 0xEE:
                HLValue = HLValue.SetBit(5, true);
                break;
            case 0xEF:
                Accumulator = Accumulator.SetBit(5, true);
                break;

            case 0xF0:
                B = B.SetBit(6, true);
                break;
            case 0xF1:
                C = C.SetBit(6, true);
                break;
            case 0xF2:
                D = D.SetBit(6, true);
                break;
            case 0xF3:
                E = E.SetBit(6, true);
                break;
            case 0xF4:
                H = H.SetBit(6, true);
                break;
            case 0xF5:
                L = L.SetBit(6, true);
                break;
            case 0xF6:
                HLValue = HLValue.SetBit(6, true);
                break;
            case 0xF7:
                Accumulator = Accumulator.SetBit(6, true);
                break;

            case 0xF8:
                B = B.SetBit(7, true);
                break;
            case 0xF9:
                C = C.SetBit(7, true);
                break;
            case 0xFA:
                D = D.SetBit(7, true);
                break;
            case 0xFB:
                E = E.SetBit(7, true);
                break;
            case 0xFC:
                H = H.SetBit(7, true);
                break;
            case 0xFD:
                L = L.SetBit(7, true);
                break;
            case 0xFE:
                HLValue = HLValue.SetBit(7, true);
                break;
            case 0xFF:
                Accumulator = Accumulator.SetBit(7, true);
                break;
        }
    }
}