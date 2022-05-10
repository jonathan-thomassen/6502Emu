using static _6502Emu.Registers;
using static _6502Emu.Memory;

namespace _6502Emu {
    #region Enums
    internal enum InstructionMode {
        Absolute,
        Accumulator,
        Immediate,
        Indirect,
        ZeroPage
    }

    internal enum IndexRegister {
        None,
        X,
        Y
    }
    #endregion

    internal static class CPU {
        internal static ulong CycleCount;

        internal static void Initialize() {
        }

        internal static void ExecuteInstruction(byte opcode) {
            #region Opcode functions
            // ADC - Add memory to accumulator with carry
            void ADC(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                bool bit7 = (A | 0b0111_1111) == 0b1111_1111;
                ushort address = 0x0000;

                switch (mode) {
                    case InstructionMode.Immediate: // 2B, 2C
                        SpendCycles(2);
                        address = (ushort)(PC++ + ROMLocation);
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 4/5C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += X)) {
                                    SpendCycles(1);
                                }
                                break;
                            case IndexRegister.Y: // 3B, 4/5C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                        }
                        break;
                    case InstructionMode.Indirect:
                        byte zeroPageAddress = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                zeroPageAddress += X;
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                break;
                            case IndexRegister.Y: // 2B, 5/6C
                                SpendCycles(5);
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                        }
                        break;
                }
                int accumulator = A + ReadByte(address) + (C ? 1 : 0);
                if (accumulator > 255) {
                    C = true;
                    accumulator -= 256;
                } else {
                    C = false;
                }
                A = (byte)accumulator;
                V = (A | 0b0111_1111) == 0b1111_1111 != bit7;
                N = (A | 0b0111_1111) == 0b1111_1111;
                Z = A == 0;
            }

            // AND - AND Memory with Accumulator
            void AND(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate:
                        switch (index) {
                            case IndexRegister.None: // 2B, 2C
                                SpendCycles(2);
                                address = (ushort)(PC++ + ROMLocation);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += X)) {
                                    SpendCycles(1);
                                }
                                break;
                            case IndexRegister.Y: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Indirect:
                        byte zeroPageAddress = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                zeroPageAddress += X;
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                break;
                            case IndexRegister.Y: // 2B, 5/6C
                                SpendCycles(5);
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = ReadByte(address);
                A = (byte)(A | value);
                Z = A == 0;
                N = (A | 0b0111_1111) == 0b1111_1111;
            }

            // ASL - Arithmetic Shift Left
            void ASL(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address = 0;
                bool onAccumulator = false;

                switch (mode) {
                    case InstructionMode.Accumulator:
                        switch (index) {
                            case IndexRegister.None: // 1B, 2C
                                SpendCycles(2);
                                onAccumulator = true;
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 5C
                                SpendCycles(5);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 6C
                                SpendCycles(6);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 7C
                                address = (ushort)(((highByte << 8) | lowByte) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = onAccumulator ? A : ReadByte(address);

                C = (value | 0b0111_1111) == 0b1111_1111;
                N = (value | 0b1011_1111) == 0b1111_1111;
                value <<= 1;
                Z = value == 0;

                if (onAccumulator) {
                    A = value;
                } else {
                    WriteByte(address, value);
                }
            }

            // BIT - Bit Test
            void BIT(InstructionMode mode) {
                ushort address;
                switch (mode) {
                    case InstructionMode.ZeroPage: // 2B, 3C
                        SpendCycles(3);
                        address = (byte)PC++;
                        break;
                    case InstructionMode.Absolute: // 3B, 4C
                        SpendCycles(4);
                        byte highByte = ReadByte((byte)PC++);
                        byte lowByte = ReadByte((byte)PC++);
                        address = (ushort)(highByte << 8 | lowByte);
                        break;
                    default:
                        throw new SystemException($"Invalid Instruction Addressing Mode: {mode}");
                }

                byte value = ReadByte(address);
                N = (value | 0b0111_1111) == 0b1111_1111;
                V = (value | 0b1011_1111) == 0b1111_1111;
                byte result = (byte)(value & A);
                Z = result == 0;
            }

            // CMP - Compare
            void CMP(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate:
                        switch (index) {
                            case IndexRegister.None: // 2B, 2C
                                SpendCycles(2);
                                address = (ushort)(PC++ + ROMLocation);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += X)) {
                                    SpendCycles(1);
                                }
                                break;
                            case IndexRegister.Y: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Indirect:
                        byte zeroPageAddress = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                zeroPageAddress += X;
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                break;
                            case IndexRegister.Y: // 2B, 5/6C
                                SpendCycles(5);
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = ReadByte(address);
                C = value <= A;
                Z = A == value;
                byte result = (byte)(A - value);
                N = (result | 0b0111_1111) == 0b1111_1111;
            }

            // CPX - Compare X Register
            void CPX(InstructionMode mode) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate: // 2B, 2C
                        SpendCycles(2);
                        address = (ushort)(PC++ + ROMLocation);
                        break;
                    case InstructionMode.ZeroPage: // 2B, 3C
                        SpendCycles(3);
                        address = ReadByte((ushort)(PC++ + ROMLocation));
                        break;
                    case InstructionMode.Absolute: // 3B, 4C
                        SpendCycles(4);
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        address = (ushort)((highByte << 8) | lowByte);
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = ReadByte(address);
                C = X >= value;
                Z = value == X;
                byte result = (byte)(X - value);
                N = (result | 0b0111_1111) == 0b1111_1111;
            }

            // CPY - Compare Y Register
            void CPY(InstructionMode mode) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate: // 2B, 2C
                        SpendCycles(2);
                        address = (ushort)(PC++ + ROMLocation);
                        break;
                    case InstructionMode.ZeroPage: // 2B, 3C
                        SpendCycles(3);
                        address = ReadByte((ushort)(PC++ + ROMLocation));
                        break;
                    case InstructionMode.Absolute: // 3B, 4C
                        SpendCycles(4);
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        address = (ushort)((highByte << 8) | lowByte);
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = ReadByte(address);
                C = Y >= value;
                Z = value == X;
                byte result = (byte)(Y - value);
                N = (result | 0b0111_1111) == 0b1111_1111;
            }

            // DEC - Decrement Memory By One
            void DEC(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 5C
                                SpendCycles(5);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 6C
                                SpendCycles(6);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 7C
                                SpendCycles(7);
                                address = (ushort)(((highByte << 8) | lowByte) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte result = (byte)(ReadByte(address) - 1);
                WriteByte(address, result);
                N = (result | 0b0111_1111) == 0b1111_1111;
                Z = result == 0;
            }

            // EOR - Exclusive OR Memory with Accumulator
            void EOR(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate:
                        switch (index) {
                            case IndexRegister.None: // 2B, 2C
                                SpendCycles(2);
                                address = (ushort)(PC++ + ROMLocation);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += X)) {
                                    SpendCycles(1);
                                }
                                break;
                            case IndexRegister.Y: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Indirect:
                        byte zeroPageAddress = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                zeroPageAddress += X;
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                break;
                            case IndexRegister.Y: // 2B, 5/6C
                                SpendCycles(5);
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = ReadByte(address);
                A = (byte)(A ^ value);
                Z = A == 0;
                N = (A | 0b0111_1111) == 0b1111_1111;
            }

            // INC - Increment Memory By One
            void INC(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;
                byte result;

                switch (mode) {
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 5C
                                SpendCycles(5);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 6C
                                SpendCycles(6);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 7C
                                SpendCycles(7);
                                address = (ushort)(((highByte << 8) | lowByte) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                result = (byte)(ReadByte(address) + 1);
                WriteByte(address, result);
                N = (result | 0b0111_1111) == 0b1111_1111;
                Z = result == 0;
            }

            // JMP - Jump
            void JMP(InstructionMode mode) {
                byte lowByte = ReadByte(PC++);
                byte highByte = ReadByte(PC++);

                switch (mode) {
                    case InstructionMode.Absolute: // 3B, 3C
                        SpendCycles(3);
                        PC = (ushort)(highByte << 8 | lowByte);
                        break;
                    case InstructionMode.Indirect: // 3B, 5C
                        SpendCycles(5);
                        PC = ReadByte((ushort)(highByte << 8 | lowByte));
                        break;
                    default:
                        throw new SystemException($"Invalid Instruction Addressing Mode: {mode}");
                }
            }

            // LDA - Load Accumulator
            void LDA(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate:
                        switch (index) {
                            case IndexRegister.None: // 2B, 2C
                                SpendCycles(2);
                                address = (ushort)(PC++ + ROMLocation);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += X)) {
                                    SpendCycles(1);
                                }
                                break;
                            case IndexRegister.Y: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Indirect:
                        byte zeroPageAddress = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                zeroPageAddress += X;
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                break;
                            case IndexRegister.Y: // 2B, 5/6C
                                SpendCycles(5);
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                A = ReadByte(address);
                Z = A == 0;
                N = (A | 0b0111_1111) == 0b1111_1111;
            }

            // LDX - Load Index X With Memory
            void LDX(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate:
                        address = (ushort)(PC++ + ROMLocation);
                        switch (index) {
                            case IndexRegister.None: // 2B, 2C
                                SpendCycles(2);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        address = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                break;
                            case IndexRegister.Y: // 2B, 4C
                                SpendCycles(4);
                                address += Y;
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        address = (ushort)((highByte << 8) | lowByte);
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                break;
                            case IndexRegister.Y: // 3B, 4/5C
                                SpendCycles(4);
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                X = ReadByte(address);
                N = (X | 0b0111_1111) == 0b1111_1111;
                Z = X == 0;
            }

            // LDY - Load Y Register
            void LDY(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate:
                        address = (ushort)(PC++ + ROMLocation);
                        switch (index) {
                            case IndexRegister.None: // 2B, 2C
                                SpendCycles(2);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        address = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address += X;
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        address = (ushort)((highByte << 8) | lowByte);
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                break;
                            case IndexRegister.X: // 3B, 4/5C
                                SpendCycles(4);
                                if ((byte)address > (byte)(address += X)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                Y = ReadByte(address);
                N = (Y | 0b0111_1111) == 0b1111_1111;
                Z = Y == 0;
            }

            // LSR - Logical Shift Right
            void LSR(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address = 0;
                bool onAccumulator = false;

                switch (mode) {
                    case InstructionMode.Accumulator:
                        switch (index) {
                            case IndexRegister.None: // 1B, 2C
                                SpendCycles(2);
                                onAccumulator = true;
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 5C
                                SpendCycles(5);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 6C
                                SpendCycles(6);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 7C
                                address = (ushort)(((highByte << 8) | lowByte) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = onAccumulator ? A : ReadByte(address);

                C = (value | 0b1111_1110) == 0b1111_1111;
                value >>= 1;
                Z = value == 0;
                N = false;

                if (onAccumulator) {
                    A = value;
                } else {
                    WriteByte(address, value);
                }
            }

            // ORA - OR memory with accumulator
            void ORA(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate:
                        switch (index) {
                            case IndexRegister.None: // 2B, 2C
                                SpendCycles(2);
                                address = (ushort)(PC++ + ROMLocation);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += X)) {
                                    SpendCycles(1);
                                }
                                break;
                            case IndexRegister.Y: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Indirect:
                        byte zeroPageAddress = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                zeroPageAddress += X;
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                break;
                            case IndexRegister.Y: // 2B, 5/6C
                                SpendCycles(5);
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = ReadByte(address);
                A = (byte)(A & value);
                Z = A == 0;
                N = (A | 0b0111_1111) == 0b1111_1111;
            }

            // ROL - Rotate Left
            void ROL(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address = 0;
                bool onAccumulator = false;

                switch (mode) {
                    case InstructionMode.Accumulator:
                        switch (index) {
                            case IndexRegister.None: // 1B, 2C
                                SpendCycles(2);
                                onAccumulator = true;
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 5C
                                SpendCycles(5);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 6C
                                SpendCycles(6);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 7C
                                address = (ushort)(((highByte << 8) | lowByte) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = onAccumulator ? A : ReadByte(address);

                N = (value | 0b1011_1111) == 0b1111_1111;
                bool carry = (value | 0b0111_1111) == 0b1111_1111;
                value <<= 1;
                if (C) {
                    value |= 0b0000_0001;
                }

                C = carry;
                Z = value == 0;

                if (onAccumulator) {
                    A = value;
                } else {
                    WriteByte(address, value);
                }
            }

            // ROR - Rotate Right
            void ROR(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address = 0;
                bool onAccumulator = false;

                switch (mode) {
                    case InstructionMode.Accumulator:
                        switch (index) {
                            case IndexRegister.None: // 1B, 2C
                                SpendCycles(2);
                                onAccumulator = true;
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 5C
                                SpendCycles(5);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 6C
                                SpendCycles(6);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 7C
                                address = (ushort)(((highByte << 8) | lowByte) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = onAccumulator ? A : ReadByte(address);

                N = C;
                bool carry = (value | 0b1111_1110) == 0b1111_1111;
                value >>= 1;
                if (C) {
                    value |= 0b1000_0000;
                }

                C = carry;
                Z = value == 0;

                if (onAccumulator) {
                    A = value;
                } else {
                    WriteByte(address, value);
                }
            }

            // SBC - Subtract Memory from Accumulator with Carry
            void SBC(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.Immediate:
                        switch (index) {
                            case IndexRegister.None: // 2B, 2C
                                SpendCycles(2);
                                address = (ushort)(PC++ + ROMLocation);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += X)) {
                                    SpendCycles(1);
                                }
                                break;
                            case IndexRegister.Y: // 3B, 4/5C
                                address = (ushort)((highByte << 8) | lowByte);
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Indirect:
                        byte zeroPageAddress = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                zeroPageAddress += X;
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                break;
                            case IndexRegister.Y: // 2B, 5/6C
                                SpendCycles(5);
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                if ((byte)address > (byte)(address += Y)) {
                                    SpendCycles(1);
                                }
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                byte value = ReadByte(address);
                bool bit7 = (A | 0b1011_1111) == 0b1111_1111;
                int borrow = A - (value + (C ? 1 : 0));
                A -= (byte)(value + (C ? 1 : 0));
                C = borrow >= 0;
                if (bit7) {
                    V = (A | 0b1011_1111) == 0b1011_1111;
                }
                Z = A == 0;
                N = (A | 0b0111_1111) == 0b1111_1111;
            }

            // STA - Store Accumulator
            void STA(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.ZeroPage:
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                address = ReadByte((ushort)(PC++ + ROMLocation));
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address = (byte)(ReadByte((ushort)(PC++ + ROMLocation)) + X);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                address = (ushort)((highByte << 8) | lowByte);
                                break;
                            case IndexRegister.X: // 3B, 5C
                                SpendCycles(5);
                                address = (ushort)((highByte << 8) | lowByte + X);
                                break;
                            case IndexRegister.Y: // 3B, 5C
                                SpendCycles(5);
                                address = (ushort)((highByte << 8) | lowByte + Y);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Indirect:
                        byte zeroPageAddress = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.X: // 2B, 6C
                                SpendCycles(6);
                                zeroPageAddress += X;
                                address = (ushort)((ReadByte((ushort)(zeroPageAddress + 1)) << 8) | ReadByte(zeroPageAddress));
                                break;
                            case IndexRegister.Y: // 2B, 6C
                                SpendCycles(5);
                                address = (ushort)(((ReadByte((ushort)(zeroPageAddress + 1)) << 8) |
                                                   ReadByte(zeroPageAddress)) + Y);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                WriteByte(address, A);
            }

            // STX - Store X Register
            void STX(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.ZeroPage:
                        address = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                break;
                            case IndexRegister.Y: // 2B, 4C
                                SpendCycles(4);
                                address += Y;
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        address = (ushort)((highByte << 8) | lowByte);
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                WriteByte(address, X);
            }

            // STY - Store Y Register
            void STY(InstructionMode mode, IndexRegister index = IndexRegister.None) {
                ushort address;

                switch (mode) {
                    case InstructionMode.ZeroPage:
                        address = ReadByte((ushort)(PC++ + ROMLocation));
                        switch (index) {
                            case IndexRegister.None: // 2B, 3C
                                SpendCycles(3);
                                break;
                            case IndexRegister.X: // 2B, 4C
                                SpendCycles(4);
                                address += X;
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    case InstructionMode.Absolute:
                        byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                        byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                        address = (ushort)((highByte << 8) | lowByte);
                        switch (index) {
                            case IndexRegister.None: // 3B, 4C
                                SpendCycles(4);
                                break;
                            default:
                                throw new SystemException($"Invalid Index Register: {index}");
                        }
                        break;
                    default:
                        throw new SystemException($"Invalid Addressing Mode: {mode}");
                }

                WriteByte(address, Y);
            }

            void BRK() {
                // 1B, 7C
                SpendCycles(7);
                WriteByte(S--, (byte)((PC + 2) >> 8));
                WriteByte(S--, (byte)(PC + 2));
                WriteByte(S--, (byte)(P | 0b0001_0000));
                PC = (ushort)((ReadByte(0xFFFF) << 8) | ReadByte(0xFFFE));
            }

            void JSR() {
                // 3B, 6C
                SpendCycles(6);
                byte lowByte = ReadByte((ushort)(PC++ + ROMLocation));
                byte highByte = ReadByte((ushort)(PC++ + ROMLocation));
                WriteByte((ushort)(StackLocation + S--), (byte)(PC >> 8));
                WriteByte((ushort)(StackLocation + S--), (byte)PC);
                PC = (ushort)(highByte << 8 | lowByte);
            }

            void PHA() {
                // 1B, 3C
                SpendCycles(3);
                WriteByte((ushort)(StackLocation + S--), A);
            }

            void PLA() {
                // 1B, 4C
                SpendCycles(4);
                A = ReadByte((ushort)(StackLocation + S++));
                N = (A | 0b0111_1111) == 0b1111_1111;
                Z = A == 0;
            }

            void TXS() {
                // 1B, 2C
                SpendCycles(2);
                S = X;
            }

            void TSX() {
                // 1B, 2C
                SpendCycles(2);
                X = S;
                N = (X | 0b0111_1111) == 0b1111_1111;
                Z = X == 0;
            }

            void PHP() {
                // 1B, 3C
                SpendCycles(3);
                WriteByte((ushort)(StackLocation + S--), P);
            }

            void PLP() {
                // 1B, 4C
                SpendCycles(4);
                P = ReadByte((ushort)(StackLocation + S++));
            }

            void RTI() {
                // 1B, 6C
                SpendCycles(6);
                P = ReadByte((ushort)(StackLocation + S++));
                byte lowByte = ReadByte((ushort)(StackLocation + S++));
                byte highByte = ReadByte((ushort)(StackLocation + S++));
                PC = (ushort)(highByte << 8 | lowByte);
            }

            void RTS() {
                // 1B, 6C
                SpendCycles(6);
                byte lowByte = ReadByte((ushort)(StackLocation + S++));
                byte highByte = ReadByte((ushort)(StackLocation + S++));
                PC = (ushort)((highByte << 8 | lowByte) + 1);
            }

            void TXA() {
                // 1B, 2C
                SpendCycles(2);
                A = X;
                N = (A | 0b0111_1111) == 0b1111_1111;
                Z = A == 0;
            }

            void TYA() {
                // 1B, 2C
                SpendCycles(2);
                A = Y;
                N = (A | 0b0111_1111) == 0b1111_1111;
                Z = A == 0;
            }

            void TAX() {
                // 1B, 2C
                SpendCycles(2);
                X = A;
                N = (X | 0b0111_1111) == 0b1111_1111;
                Z = X == 0;
            }

            void TAY() {
                // 1B, 2C
                SpendCycles(2);
                Y = A;
                N = (Y | 0b0111_1111) == 0b1111_1111;
                Z = Y == 0;
            }

            void INX() {
                // 1B, 2C
                SpendCycles(2);
                X++;
                N = (X | 0b0111_1111) == 0b1111_1111;
                Z = X == 0;
            }

            void INY() {
                // 1B, 2C
                SpendCycles(2);
                Y++;
                N = (Y | 0b0111_1111) == 0b1111_1111;
                Z = Y == 0;
            }

            // - Decrement Index Register X By One
            void DEX() {
                // 1B, 2C
                SpendCycles(2);
                X--;
                N = (X | 0b0111_1111) == 0b1111_1111;
                Z = X == 0;
            }

            void DEY() {
                // 1B, 2C
                SpendCycles(2);
                Y--;
                N = (Y | 0b0111_1111) == 0b1111_1111;
                Z = Y == 0;
            }

            // NOP - No operation
            void NOP() {
                // 1B, 2C
                SpendCycles(2);
            }

            void Branch(bool takeBranch) {
                SpendCycles(2);
                byte offset = ReadByte((ushort)(PC++ + ROMLocation));
                sbyte signedOffset;

                if (offset < 128) {
                    signedOffset = (sbyte)(offset);
                } else {
                    signedOffset = (sbyte)(offset - 256);
                }

                if (takeBranch) {
                    SpendCycles(1);
                    if (signedOffset < 0) {
                        if ((byte)PC < (byte)(PC = (ushort)(PC + signedOffset))) {
                            SpendCycles(1);
                        }
                    } else {
                        if ((byte)PC > (byte)(PC = (ushort)(PC + signedOffset))) {
                            SpendCycles(1);
                        }
                    }
                }
            }
            #endregion

            #region Opcode switch
            switch (opcode) {
                case 0x00: // BRK
                    Console.WriteLine("Executing instruction BRK (Force Break)...");
                    BRK();
                    break;
                case 0x01: // ORA ind,X
                    Console.WriteLine("Executing instruction ORA (OR Memory with Accumulator), Mode: Indirect, Index: X...");
                    ORA(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x05: // ORA zpg
                    Console.WriteLine("Executing instruction ORA (OR Memory with Accumulator), Mode: ZeroPage...");
                    ORA(InstructionMode.ZeroPage);
                    break;
                case 0x06: // ASL zpg
                    Console.WriteLine("Executing instruction ASL (Arithmetic Shift Left), Mode: ZeroPage...");
                    ASL(InstructionMode.ZeroPage);
                    break;
                case 0x08: // PHP
                    Console.WriteLine("Executing instruction PHP (Push Processor Status on Stack with Accumulator)...");
                    PHP();
                    break;
                case 0x09: // ORA #
                    Console.WriteLine("Executing instruction ORA (OR Memory with Accumulator), Mode: Immediate...");
                    ORA(InstructionMode.Immediate);
                    break;
                case 0x0A: // ASL A
                    Console.WriteLine("Executing instruction ASL (Arithmetic Shift Left), Mode: Accumulator...");
                    ASL(InstructionMode.Accumulator);
                    break;
                case 0x0D: // ORA abs
                    Console.WriteLine("Executing instruction ORA (OR Memory with Accumulator), Mode: Absolute...");
                    ORA(InstructionMode.Absolute);
                    break;
                case 0x0E: // ASL abs
                    Console.WriteLine("Executing instruction ASL (Arithmetic Shift Left), Mode: Absolute...");
                    ASL(InstructionMode.Absolute);
                    break;
                case 0x10: // BPL rel
                    Console.WriteLine("Executing instruction BPL (Branch on Result Plus)...");
                    Branch(!N);
                    break;
                case 0x11: // ORA ind,Y
                    Console.WriteLine("Executing instruction ORA (OR Memory with Accumulator), Mode: Indirect, Index: Y...");
                    ORA(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x15: // ORA zpg,X
                    Console.WriteLine("Executing instruction ORA (OR Memory with Accumulator), Mode: ZeroPage, Index: X...");
                    ORA(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x16: // ASL zpg,X
                    Console.WriteLine("Executing instruction ASL (Arithmetic Shift Left), Mode: ZeroPage, Index: X...");
                    ASL(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x18: // CLC
                    Console.WriteLine("Executing instruction CLC (Clear Carry Flag)...");
                    SpendCycles(2);
                    C = false;
                    break;
                case 0x19: // ORA abs,Y
                    Console.WriteLine("Executing instruction ORA (OR Memory with Accumulator), Mode: Absolute, Index: Y...");
                    ORA(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x1D: // ORA abs,X
                    Console.WriteLine("Executing instruction ORA (OR Memory with Accumulator), Mode: Absolute, Index: X...");
                    ORA(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x1E: // ASL abs,X
                    Console.WriteLine("Executing instruction ASL (Arithmetic Shift Left), Mode: Absolute, Index: X...");
                    ASL(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x20: // JSR abs
                    Console.WriteLine("Executing instruction JST (Jump to Subroutine)...");
                    JSR();
                    break;
                case 0x21: // AND ind,X
                    Console.WriteLine("Executing instruction AND (AND Memory with Accumulator), Mode: Indirect, Index: X...");
                    AND(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x24: // BIT zpg
                    Console.WriteLine("Executing instruction BIT (Test Bits in Memory with Accumulator), Mode: ZeroPage...");
                    BIT(InstructionMode.ZeroPage);
                    break;
                case 0x25: // AND zpg
                    Console.WriteLine("Executing instruction AND (AND Memory with Accumulator), Mode: ZeroPage...");
                    AND(InstructionMode.ZeroPage);
                    break;
                case 0x26: // ROL zpg
                    Console.WriteLine("Executing instruction ROL (Rotate Left), Mode: ZeroPage...");
                    ROL(InstructionMode.ZeroPage);
                    break;
                case 0x28: // PLP
                    Console.WriteLine("Executing instruction PLP (Pull Processor Status from Stack)...");
                    PLP();
                    break;
                case 0x29: // AND #
                    Console.WriteLine("Executing instruction AND (AND Memory with Accumulator), Mode: Immediate...");
                    AND(InstructionMode.Immediate);
                    break;
                case 0x2A: // ROL A
                    Console.WriteLine("Executing instruction ROL (Rotate Left), Mode: Accumulator...");
                    ROL(InstructionMode.Accumulator);
                    break;
                case 0x2C: // BIT abs
                    Console.WriteLine("Executing instruction BIT (Test Bits in Memory with Accumulator), Mode: Absolute...");
                    BIT(InstructionMode.Absolute);
                    break;
                case 0x2D: // AND abs
                    Console.WriteLine("Executing instruction AND (AND Memory with Accumulator), Mode: Absolute...");
                    AND(InstructionMode.Absolute);
                    break;
                case 0x2E: // ROL abs
                    Console.WriteLine("Executing instruction ROL (Rotate Left), Mode: Absolute...");
                    ROL(InstructionMode.Absolute);
                    break;
                case 0x30: // BMI rel
                    Console.WriteLine("Executing instruction BMI (Branch on Result Minus)...");
                    Branch(N);
                    break;
                case 0x31: // AND ind,Y
                    Console.WriteLine("Executing instruction AND (AND Memory with Accumulator), Mode: Indirect, Index: Y...");
                    AND(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x35: // AND zpg,X
                    Console.WriteLine("Executing instruction AND (AND Memory with Accumulator), Mode: ZeroPage, Index: X...");
                    AND(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x36: // ROL zpg,X
                    Console.WriteLine("Executing instruction ROL (Rotate Left), Mode: ZeroPage, Index: X...");
                    ROL(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x38: // SEC
                    Console.WriteLine("Executing instruction SEC (Set Carry Flag)...");
                    SpendCycles(2);
                    C = true;
                    break;
                case 0x39: // AND abs,Y
                    Console.WriteLine("Executing instruction AND (AND Memory with Accumulator), Mode: Absolute, Index: Y...");
                    AND(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x3D: // AND abs,X
                    Console.WriteLine("Executing instruction AND (AND Memory with Accumulator), Mode: Absolute, Index: X...");
                    AND(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x3E: // ROL abs,X
                    Console.WriteLine("Executing instruction ROL (Rotate Left with Accumulator), Mode: Absolute, Index: X...");
                    ROL(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x40: // RTI
                    Console.WriteLine("Executing instruction RTI (Return from Interrupt)...");
                    RTI();
                    break;
                case 0x41: // EOR ind,X
                    Console.WriteLine("Executing instruction EOR (XOR Memory with Accumulator), Mode: Indirect, Index: X...");
                    EOR(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x45: // EOR zpg
                    Console.WriteLine("Executing instruction EOR (XOR Memory with Accumulator), Mode: ZeroPage...");
                    EOR(InstructionMode.ZeroPage);
                    break;
                case 0x46: // LSR zpg
                    Console.WriteLine("Executing instruction LSR (Logical Shift Right), Mode: ZeroPage...");
                    LSR(InstructionMode.ZeroPage);
                    break;
                case 0x48: // PHA
                    Console.WriteLine("Executing instruction PHA (Push Accumulator to Stack)...");
                    PHA();
                    break;
                case 0x49: // EOR #
                    Console.WriteLine("Executing instruction EOR (XOR Memory with Accumulator), Mode: Immediate...");
                    EOR(InstructionMode.Immediate);
                    break;
                case 0x4A: // LSR A
                    Console.WriteLine("Executing instruction LSR (Logical Shift Right), Mode: Accumulator...");
                    LSR(InstructionMode.Accumulator);
                    break;
                case 0x4C: // JMP abs
                    Console.WriteLine("Executing instruction JMP (Jump to New Location), Mode: Absolute...");
                    JMP(InstructionMode.Absolute);
                    break;
                case 0x4D: // EOR abs
                    Console.WriteLine("Executing instruction EOR (XOR Memory with Accumulator), Mode: Absolute...");
                    EOR(InstructionMode.Absolute);
                    break;
                case 0x4E: // LSR abs,X
                    Console.WriteLine("Executing instruction LSR (Logical Shift Right), Mode: Absolute, Index: X...");
                    LSR(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x50: // BVC rel
                    Console.WriteLine("Executing instruction BVC (Branch on Overflow Clear)...");
                    Branch(!V);
                    break;
                case 0x51: // EOR ind,X
                    Console.WriteLine("Executing instruction EOR (XOR Memory with Accumulator), Mode: Indirect, Index: X...");
                    EOR(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x55: // EOR zpg,X
                    Console.WriteLine("Executing instruction EOR (XOR Memory with Accumulator), Mode: ZeroPage, Index: X...");
                    EOR(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x56: // LSR zpg,X
                    Console.WriteLine("Executing instruction LSR (Logical Shift Right), Mode: ZeroPage, Index: X...");
                    LSR(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x58: // CLI
                    Console.WriteLine("Executing instruction CLI (Clear Interrupt Disable)...");
                    SpendCycles(2);
                    I = false;
                    break;
                case 0x59: // EOR abs,Y
                    Console.WriteLine("Executing instruction EOR (XOR Memory with Accumulator), Mode: Absolute, Index: Y...");
                    EOR(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x5D: // EOR abs,X
                    Console.WriteLine("Executing instruction EOR (XOR Memory with Accumulator), Mode: Absolute, Index: X...");
                    EOR(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x5E: // LSR abs,X
                    Console.WriteLine("Executing instruction LSR (Logical Shift Right), Mode: Absolute, Index: X...");
                    LSR(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x60: // RTS
                    Console.WriteLine("Executing instruction RTS (Return from Subroutine)...");
                    RTS();
                    break;
                case 0x61: // ADC ind,Y
                    Console.WriteLine("Executing instruction ADC (Add with Carry to Accumulator), Mode: Indirect, Index: Y...");
                    ADC(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x65: // ADC zpg
                    Console.WriteLine("Executing instruction ADC (Add with Carry to Accumulator), Mode: ZeroPage...");
                    ADC(InstructionMode.ZeroPage);
                    break;
                case 0x66: // ROR zpg
                    Console.WriteLine("Executing instruction ROR (Rotate Right), Mode: ZeroPage...");
                    ROR(InstructionMode.ZeroPage);
                    break;
                case 0x68: // PLA
                    Console.WriteLine("Executing instruction PLA (Pull Accumulator from Stack)...");
                    PLA();
                    break;
                case 0x69: // ADC #
                    Console.WriteLine("Executing instruction ADC (Add with Carry to Accumulator), Mode: Immediate...");
                    ADC(InstructionMode.Immediate);
                    break;
                case 0x6A: // ROR A
                    Console.WriteLine("Executing instruction ROR (Rotate Right), Mode: Accumulator...");
                    ROR(InstructionMode.Accumulator);
                    break;
                case 0x6C: // JMP ind
                    Console.WriteLine("Executing instruction JMP (Jump to New Location), Mode: Indirect...");
                    JMP(InstructionMode.Indirect);
                    break;
                case 0x6D: // ADC abs
                    Console.WriteLine("Executing instruction ADC (Add with Carry to Accumulator), Mode: Absolute...");
                    ADC(InstructionMode.Absolute);
                    break;
                case 0x6E: // ROR abs
                    Console.WriteLine("Executing instruction ROR (Rotate Right), Mode: Absolute...");
                    ROR(InstructionMode.Absolute);
                    break;
                case 0x70: // BVS rel
                    Console.WriteLine("Executing instruction BVS (Branch on Overflow Set)...");
                    Branch(V);
                    break;
                case 0x71: // ADC ind,Y
                    Console.WriteLine("Executing instruction ADC (Add with Carry to Accumulator), Mode: Indirect, Index: Y...");
                    ADC(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x75: // ADC zpg,X
                    Console.WriteLine("Executing instruction ADC (Add with Carry to Accumulator), Mode: ZeroPage, Index: X...");
                    ADC(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x76: // ROR zpg,X
                    Console.WriteLine("Executing instruction ROR (Rotate Right), Mode: ZeroPage, Index: X...");
                    ROR(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x78: // SEI
                    Console.WriteLine("Executing instruction SEI (Set Interrupt Disable)...");
                    SpendCycles(2);
                    I = true;
                    break;
                case 0x79: // ADC abs,Y
                    Console.WriteLine("Executing instruction ADC (Add with Carry to Accumulator), Mode: Absolute, Index: Y...");
                    ADC(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x7D: // ADC abs,X
                    Console.WriteLine("Executing instruction ADC (Add with Carry to Accumulator), Mode: Absolute, Index: X...");
                    ADC(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x7E: // ROR abs,X
                    Console.WriteLine("Executing instruction ROR (Rotate Right), Mode: Absolute, Index: X...");
                    ROR(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x81: // STA ind,X
                    Console.WriteLine("Executing instruction STA (Store Accumulator in Memory), Mode: Indirect, Index: X...");
                    STA(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x84: // STY zpg
                    Console.WriteLine("Executing instruction STY (Store Index Register Y in Memory), Mode: ZeroPage...");
                    STY(InstructionMode.ZeroPage);
                    break;
                case 0x85: // STA zpg
                    Console.WriteLine("Executing instruction STA (Store Accumulator in Memory), Mode: ZeroPage...");
                    STA(InstructionMode.ZeroPage);
                    break;
                case 0x86: // STX zpg
                    Console.WriteLine("Executing instruction STX (Store Index Register X in Memory), Mode: ZeroPage...");
                    STX(InstructionMode.ZeroPage);
                    break;
                case 0x88: // DEY
                    Console.WriteLine("Executing instruction DEY (Decrement Index Register Y)...");
                    DEY();
                    break;
                case 0x8A: // TXA
                    Console.WriteLine("Executing instruction TXA (Transfer Index Register X to Accumulator)...");
                    TXA();
                    break;
                case 0x8C: // STY abs
                    Console.WriteLine("Executing instruction STY (Store Index Register Y in Memory), Mode: Absolute...");
                    STY(InstructionMode.Absolute);
                    break;
                case 0x8D: // STA abs
                    Console.WriteLine("Executing instruction STA (Store Accumulator in Memory), Mode: Absolute...");
                    STA(InstructionMode.Absolute);
                    break;
                case 0x8E: // STX abs
                    Console.WriteLine("Executing instruction STX (Store Index Register X in Memory), Mode: Absolute...");
                    STX(InstructionMode.Absolute);
                    break;
                case 0x90: // BCC rel
                    Console.WriteLine("Executing instruction BCC (Branch on Carry Clear)...");
                    Branch(!C);
                    break;
                case 0x91: // STA ind,Y
                    Console.WriteLine("Executing instruction STA (Store Accumulator in Memory), Mode: Indirect, Index: Y...");
                    STA(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x94: // STY zpg,X
                    Console.WriteLine("Executing instruction STY (Store Index Register Y in Memory), Mode: ZeroPage, Index: X...");
                    STY(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x95: // STA zpg,X
                    Console.WriteLine("Executing instruction STA (Store Accumulator in Memory), Mode: ZeroPage, Index: X...");
                    STA(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x96: // STX zpg,Y
                    Console.WriteLine("Executing instruction STX (Store Index Register X in Memory), Mode: ZeroPage, Index: Y...");
                    STX(InstructionMode.ZeroPage, IndexRegister.Y);
                    break;
                case 0x98: // TYA
                    Console.WriteLine("Executing instruction TYA (Transfer Index Register Y to Accumulator)...");
                    TYA();
                    break;
                case 0x99: // STA abs,Y
                    Console.WriteLine("Executing instruction STA (Store Accumulator in Memory), Mode: Absolute, Index: Y...");
                    STA(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x9A: // TXS
                    Console.WriteLine("Executing instruction TXS (Transfer Index Register X to Stack Pointer)...");
                    TXS();
                    break;
                case 0x9D: // STA abs,X
                    Console.WriteLine("Executing instruction STA (Store Accumulator in Memory), Mode: Absolute, Index: X...");
                    STA(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xA0: // LDY #
                    Console.WriteLine("Executing instruction LDY (Load Index Register Y with Memory), Mode: Immediate...");
                    LDY(InstructionMode.Immediate);
                    break;
                case 0xA1: // LDA ind,X
                    Console.WriteLine("Executing instruction LDA (Load Accumulator with Memory), Mode: Indirect, Index: X...");
                    LDA(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0xA2: // LDX #
                    Console.WriteLine("Executing instruction LDX (Load Index Register X with Memory), Mode: Immediate...");
                    LDX(InstructionMode.Immediate);
                    break;
                case 0xA4: // LDY zpg
                    Console.WriteLine("Executing instruction LDY (Load Index Register Y with Memory), Mode: ZeroPage...");
                    LDY(InstructionMode.ZeroPage);
                    break;
                case 0xA5: // LDA zpg
                    Console.WriteLine("Executing instruction LDA (Load Accumulator with Memory), Mode: ZeroPage...");
                    LDA(InstructionMode.ZeroPage);
                    break;
                case 0xA6: // LDX zpg
                    Console.WriteLine("Executing instruction LDX (Load Index Register X with Memory), Mode: ZeroPage...");
                    LDX(InstructionMode.ZeroPage);
                    break;
                case 0xA8: // TAY
                    Console.WriteLine("Executing instruction TAY (Transfer Accumulator to Index Register Y)...");
                    TAY();
                    break;
                case 0xA9: // LDA #
                    Console.WriteLine("Executing instruction LDA (Load Accumulator with Memory), Mode: Immediate...");
                    LDA(InstructionMode.Immediate);
                    break;
                case 0xAA: // TAX
                    Console.WriteLine("Executing instruction TAX (Transfer Accumulator to Index Register X)...");
                    TAX();
                    break;
                case 0xAC: // LDY abs
                    Console.WriteLine("Executing instruction LDY (Load Index Register Y with Memory), Mode: Absolute...");
                    LDY(InstructionMode.Absolute);
                    break;
                case 0xAD: // LDA abs
                    Console.WriteLine("Executing instruction LDA (Load Accumulator with Memory), Mode: Absolute...");
                    LDA(InstructionMode.Absolute);
                    break;
                case 0xAE: // LDX abs
                    Console.WriteLine("Executing instruction LDX (Load Index Register X with Memory), Mode: Absolute...");
                    LDX(InstructionMode.Absolute);
                    break;
                case 0xB0: // BCS rel
                    Console.WriteLine("Executing instruction BCS (Branch on Carry Set)...");
                    Branch(C);
                    break;
                case 0xB1: // LDA ind,X
                    Console.WriteLine("Executing instruction LDA (Load Accumulator with Memory), Mode: Indirect, Index: X...");
                    LDA(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0xB4: // LDY zpg,X
                    Console.WriteLine("Executing instruction LDY (Load Index Register Y with Memory), Mode: ZeroPage, Index: X...");
                    LDY(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xB5: // LDA zpg,X
                    Console.WriteLine("Executing instruction LDA (Load Accumulatore with Memory), Mode: ZeroPage, Index: X...");
                    LDA(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xB6: // LDX zpg,Y
                    Console.WriteLine("Executing instruction LDX (Load Index Register X with Memory), Mode: ZeroPage, Index: Y...");
                    LDX(InstructionMode.ZeroPage, IndexRegister.Y);
                    break;
                case 0xB8: // CLV
                    Console.WriteLine("Executing instruction CLV (Clear Overflow Flag)...");
                    SpendCycles(2);
                    V = false;
                    break;
                case 0xB9: // LDA abs,Y
                    Console.WriteLine("Executing instruction LDA (Load Accumulator with Memory), Mode: Absolute, Index: Y...");
                    LDA(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0xBA: // TSX
                    Console.WriteLine("Executing instruction TSX (Transfer Stack Pointer to Index Register X)...");
                    TSX();
                    break;
                case 0xBC: // LDY abs,X
                    Console.WriteLine("Executing instruction LDY (Load Index Register Y with Memory), Mode: Absolute, Index: X...");
                    LDY(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xBD: // LDA abs,X
                    Console.WriteLine("Executing instruction LDA (Load Accumulator with Memory), Mode: Absolute, Index: X...");
                    LDA(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xBE: // LDX abs,Y
                    Console.WriteLine("Executing instruction LDX (Load Index Register X with Memory), Mode: Absolute, Index: Y...");
                    LDX(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0xC0: // CPY #
                    Console.WriteLine("Executing instruction CPY (Compare Index Register Y with Memory), Mode: Immediate...");
                    CPY(InstructionMode.Immediate);
                    break;
                case 0xC1: // CMP ind,X
                    Console.WriteLine("Executing instruction CMP (Compare Accumulator with Memory), Mode: Indirect, Index: X...");
                    CMP(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0xC4: // CPY zpg
                    Console.WriteLine("Executing instruction CPY (Compare Index Register Y with Memory), Mode: ZeroPage...");
                    CPY(InstructionMode.ZeroPage);
                    break;
                case 0xC5: // CMP zpg
                    Console.WriteLine("Executing instruction CMP (Compare Accumulator with Memory), Mode: ZeroPage...");
                    CMP(InstructionMode.ZeroPage);
                    break;
                case 0xC6: // DEC zpg
                    Console.WriteLine("Executing instruction DEC (Decrement Memory by One), Mode: ZeroPage...");
                    DEC(InstructionMode.ZeroPage);
                    break;
                case 0xC8: // INY
                    Console.WriteLine("Executing instruction INY (Increment Index Register Y by One)...");
                    INY();
                    break;
                case 0xC9: // CMP #
                    Console.WriteLine("Executing instruction CMP (Compare Accumulator with Memory), Mode: Immediate...");
                    CMP(InstructionMode.Immediate);
                    break;
                case 0xCA: // DEX
                    Console.WriteLine("Executing instruction DEX (Decrement Index Register X by One)...");
                    DEX();
                    break;
                case 0xCC: // CPY abs
                    Console.WriteLine("Executing instruction CPY (Compare Index Register Y with Memory), Mode: Absolute...");
                    CPY(InstructionMode.Absolute);
                    break;
                case 0xCD: // CMP abs
                    Console.WriteLine("Executing instruction CMP (Compare Accumulator with Memory), Mode: Absolute...");
                    CMP(InstructionMode.Absolute);
                    break;
                case 0xCE: // DEC abs
                    Console.WriteLine("Executing instruction DEC (Decrement Memory by One), Mode: Absolute...");
                    DEC(InstructionMode.Absolute);
                    break;
                case 0xD0: // BNE rel
                    Console.WriteLine("Executing instruction BNE (Branch on Not Equal (Zero Clear))...");
                    Branch(!Z);
                    break;
                case 0xD1: // CMP ind,Y
                    Console.WriteLine("Executing instruction CMP (Compare Accumulator with Memory), Mode: Indirect, Index: Y...");
                    CMP(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0xD5: // CMP zpg,X
                    Console.WriteLine("Executing instruction CMP (Compare Accumulator with Memory), Mode: ZeroPage, Index: X...");
                    CMP(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xD6: // DEC zpg,X
                    Console.WriteLine("Executing instruction DEC (Decrement Memory by One), Mode: ZeroPage, Index: X...");
                    DEC(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xD8: // CLD
                    Console.WriteLine("Executing instruction CLD (Clear Decimal Mode)...");
                    SpendCycles(2);
                    D = false;
                    break;
                case 0xD9: // CMP abs,Y
                    Console.WriteLine("Executing instruction CMP (Compare Accumulator with Memory), Mode: Absolute, Index: Y...");
                    CMP(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0xDD: // CMP abs,X
                    Console.WriteLine("Executing instruction CMP (Compare Accumulator with Memory), Mode: Absolute, Index: X...");
                    CMP(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xDE: // DEC abs,x
                    Console.WriteLine("Executing instruction DEC (Decrement Memory by One), Mode: Absolute, Index: X...");
                    DEC(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xE0: // CPX #
                    Console.WriteLine("Executing instruction CPX (Compare Index Register X with Memory), Mode: Immediate...");
                    CPX(InstructionMode.Immediate);
                    break;
                case 0xE1: // SBC ind,X
                    Console.WriteLine("Executing instruction SBC (Subtract with Carry), Mode: Indirect, Index: X...");
                    SBC(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0xE4: // CPX zpg
                    Console.WriteLine("Executing instruction CPX (Compare Index Register X with Memory), Mode: ZeroPage...");
                    CPX(InstructionMode.ZeroPage);
                    break;
                case 0xE5: // SBC zpg
                    Console.WriteLine("Executing instruction SBC (Subtract with Carry), Mode: ZeroPage...");
                    SBC(InstructionMode.ZeroPage);
                    break;
                case 0xE6: // INC zpg
                    Console.WriteLine("Executing instruction INC (Increment Memory by One), Mode: ZeroPage...");
                    INC(InstructionMode.ZeroPage);
                    break;
                case 0xE8: // INX
                    Console.WriteLine("Executing instruction INX (Increment Index Register X by One)...");
                    INX();
                    break;
                case 0xE9: // SBC #
                    Console.WriteLine("Executing instruction SBC (Subtract with Carry), Mode: Immediate...");
                    SBC(InstructionMode.Immediate);
                    break;
                case 0xEA: // NOP
                    Console.WriteLine("Executing instruction NOP (No Operation)...");
                    NOP();
                    break;
                case 0xEC: // CPX abs
                    Console.WriteLine("Executing instruction CPX (Compare Index Register X with Memory), Mode: Absolute...");
                    CPX(InstructionMode.Absolute);
                    break;
                case 0xED: // SBC abs
                    Console.WriteLine("Executing instruction SBC (Subtract with Carry), Mode: Absolute...");
                    SBC(InstructionMode.Absolute);
                    break;
                case 0xEE: // INC abs
                    Console.WriteLine("Executing instruction INC (Increment Memory by One with Carry), Mode: Absolute...");
                    INC(InstructionMode.Absolute);
                    break;
                case 0xF0: // BEQ rel
                    Console.WriteLine("Executing instruction BEQ (Branch on Equal (Zero Set))...");
                    Branch(Z);
                    break;
                case 0xF1: // SBC ind,Y
                    Console.WriteLine("Executing instruction SBC (Subtract with Carry), Mode: Indirect, Index: Y...");
                    SBC(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0xF5: // SBC zpg,X
                    Console.WriteLine("Executing instruction SBC (Subtract with Carry), Mode: ZeroPage, Index: X...");
                    SBC(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xF6: // INC zpg,X
                    Console.WriteLine("Executing instruction INC (Increment Memory by One), Mode: ZeroPage, Index: X...");
                    INC(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xF8: // SED
                    Console.WriteLine("Executing instruction SED (Set Decimal Mode)...");
                    SpendCycles(2);
                    D = true;
                    break;
                case 0xF9: // SBC abs,Y
                    Console.WriteLine("Executing instruction SBC (Subtract with Carry), Mode: Absolute, Index: Y...");
                    SBC(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0xFD: // SBC abs,X
                    Console.WriteLine("Executing instruction SBC (Subtract with Carry), Mode: Absolute, Index: X...");
                    SBC(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xFE: // INC abs,X
                    Console.WriteLine("Executing instruction INC (Increment Memory by One), Mode: Absolute, Index: X...");
                    INC(InstructionMode.Absolute, IndexRegister.X);
                    break;
                default:
                    throw new SystemException($"Invalid opcode: {opcode}");
            }
            #endregion
        }

        internal static void Reset() {
            Console.WriteLine("Resetting the CPU...");
            SpendCycles(7);
            PC = (ushort)((ReadByte(0xFFFD) << 8) | ReadByte(0xFFFC));
            A = 0;
            X = 0;
            Y = 0;
            P = 0x36;
            S = 0xFF;
        }

        internal static void TriggerInterrupt(bool maskable) {
            if (!I || !maskable) {
                SpendCycles(8);
                I = true;
                WriteByte((ushort)(S-- + StackLocation), (byte)(PC >> 8));
                WriteByte((ushort)(S-- + StackLocation), (byte)PC);
                WriteByte((ushort)(S-- + StackLocation), P);
                PC = maskable
                    ? (ushort)((ReadByte(0xFFFF) << 8) | ReadByte(0xFFFE))
                    : (ushort)((ReadByte(0xFFFB) << 8) | ReadByte(0xFFFA));
                PC++;
            }
        }

        internal static void SpendCycles(int cycles) {
            for (int i = 0; i < cycles; i++) {
                Console.WriteLine("Press any key to spend a cycle:");
                Console.ReadKey();
                CycleCount++;
                Console.WriteLine("");
            }
        }
    }
}