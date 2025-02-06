namespace GBemulator.CentralProcessingUnit;

public partial class CPU
{
    private void Add16bitRegisterToHL(ushort value)
    {
        int result = HL + BC;
        if (result >= 0xFFFF)
        {
            result -= 0xFFFF;
            CarryFlag = true;
        }
        else
        {
            CarryFlag = false;
        }

        HL = (ushort)result;

        HalfCarryFlag = HL.GetBit(3) == true; // I'm not quite sure i understand this yet
        SubtractionFlag = false;
    }

    private byte Increment8bitRegister(byte value)
    {
        value++;
        ZeroFlag = B == 0;
        SubtractionFlag = false;
        HalfCarryFlag = B.GetBit(3) == true; // I'm not quite sure i understand this yet

        return value;
    }

    private byte Decrement8bitRegister(byte value)
    {
        value--;
        ZeroFlag = B == 0;
        SubtractionFlag = true;
        HalfCarryFlag = B.GetBit(3) == true; // I'm not quite sure i understand this yet

        return value;
    }

    private byte LoadFromAddressInHL()
    {
        return _mmu.Read8(HL);
    }

    private void WriteToAddressInHL(byte value)
    {
        _mmu.Write8(HL, value);
    }

    private void ExecuteInstruction(byte instructionCode)
    {
        // ProgramCounter increments have been deferred so this takes care of the increment that would have happened when reading the current instruction
        ushort programCounter = (ushort)(ProgramCounter + 1);

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
                Accumulator = Accumulator.RotateByteLeft();
                ZeroFlag = false;
                SubtractionFlag = false;
                HalfCarryFlag = false;
                CarryFlag = Accumulator.GetBit(0); // old bit 7, now at bit 0
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
                Accumulator = Accumulator.RotateByteRight();
                ZeroFlag = false;
                SubtractionFlag = false;
                HalfCarryFlag = false;
                CarryFlag = Accumulator.GetBit(7); // old bit 0, now at bit 7
                break;

            case 0x10:
                _running = false;
                break;
            case 0x11:
                break;
            case 0x12:
                break;
            case 0x13:
                break;
            case 0x14:
                break;
            case 0x15:
                break;
            case 0x16:
                break;
            case 0x17:
                break;
            case 0x18:
                break;
            case 0x19:
                break;
            case 0x1A:
                break;
            case 0x1B:
                break;
            case 0x1C:
                break;
            case 0x1D:
                break;
            case 0x1E:
                break;
            case 0x1F:
                break;

            case 0x20:
                break;
            case 0x21:
                break;
            case 0x22:
                break;
            case 0x23:
                break;
            case 0x24:
                break;
            case 0x25:
                break;
            case 0x26:
                break;
            case 0x27:
                break;
            case 0x28:
                break;
            case 0x29:
                break;
            case 0x2A:
                break;
            case 0x2B:
                break;
            case 0x2C:
                break;
            case 0x2D:
                break;
            case 0x2E:
                break;
            case 0x2F:
                break;

            case 0x30:
                break;
            case 0x31:
                break;
            case 0x32:
                break;
            case 0x33:
                break;
            case 0x34:
                break;
            case 0x35:
                break;
            case 0x36:
                break;
            case 0x37:
                break;
            case 0x38:
                break;
            case 0x39:
                break;
            case 0x3A:
                break;
            case 0x3B:
                break;
            case 0x3C:
                break;
            case 0x3D:
                break;
            case 0x3E:
                break;
            case 0x3F:
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
                B = LoadFromAddressInHL();
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
                C = LoadFromAddressInHL();
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
                D = LoadFromAddressInHL();
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
                E = LoadFromAddressInHL();
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
                H = LoadFromAddressInHL();
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
                L = LoadFromAddressInHL();
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
                Accumulator = LoadFromAddressInHL();
                break;
            case 0x7F:
                Accumulator = Accumulator; // This instruction seems useless
                break;

            case 0x80:
                break;
            case 0x81:
                break;
            case 0x82:
                break;
            case 0x83:
                break;
            case 0x84:
                break;
            case 0x85:
                break;
            case 0x86:
                break;
            case 0x87:
                break;
            case 0x88:
                break;
            case 0x89:
                break;
            case 0x8A:
                break;
            case 0x8B:
                break;
            case 0x8C:
                break;
            case 0x8D:
                break;
            case 0x8E:
                break;
            case 0x8F:
                break;

            case 0x90:
                break;
            case 0x91:
                break;
            case 0x92:
                break;
            case 0x93:
                break;
            case 0x94:
                break;
            case 0x95:
                break;
            case 0x96:
                break;
            case 0x97:
                break;
            case 0x98:
                break;
            case 0x99:
                break;
            case 0x9A:
                break;
            case 0x9B:
                break;
            case 0x9C:
                break;
            case 0x9D:
                break;
            case 0x9E:
                break;
            case 0x9F:
                break;

            case 0xA0:
                break;
            case 0xA1:
                break;
            case 0xA2:
                break;
            case 0xA3:
                break;
            case 0xA4:
                break;
            case 0xA5:
                break;
            case 0xA6:
                break;
            case 0xA7:
                break;
            case 0xA8:
                break;
            case 0xA9:
                break;
            case 0xAA:
                break;
            case 0xAB:
                break;
            case 0xAC:
                break;
            case 0xAD:
                break;
            case 0xAE:
                break;
            case 0xAF:
                break;

            case 0xB0:
                break;
            case 0xB1:
                break;
            case 0xB2:
                break;
            case 0xB3:
                break;
            case 0xB4:
                break;
            case 0xB5:
                break;
            case 0xB6:
                break;
            case 0xB7:
                break;
            case 0xB8:
                break;
            case 0xB9:
                break;
            case 0xBA:
                break;
            case 0xBB:
                break;
            case 0xBC:
                break;
            case 0xBD:
                break;
            case 0xBE:
                break;
            case 0xBF:
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