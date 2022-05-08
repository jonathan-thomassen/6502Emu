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
                if (takeBranch) {
                    SpendCycles(1);
                    if ((byte)PC > (byte)(PC += offset)) {
                        SpendCycles(1);
                    }
                }
            }
            #endregion

            #region Opcode switch
            switch (opcode) {
                case 0x00: // BRK
                    BRK();
                    break;
                case 0x01: // ORA ind,X
                    ORA(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x05: // ORA zpg
                    ORA(InstructionMode.ZeroPage);
                    break;
                case 0x06: // ASL zpg
                    ASL(InstructionMode.ZeroPage);
                    break;
                case 0x08: // PHP
                    PHP();
                    break;
                case 0x09: // ORA #
                    ORA(InstructionMode.Immediate);
                    break;
                case 0x0A: // ASL A
                    ASL(InstructionMode.Accumulator);
                    break;
                case 0x0D: // ORA abs
                    ORA(InstructionMode.Absolute);
                    break;
                case 0x0E: // ASL abs
                    ASL(InstructionMode.Absolute);
                    break;
                case 0x10: // BPL rel
                    Branch(!N);
                    break;
                case 0x11: // ORA ind,Y
                    ORA(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x15: // ORA zpg,X
                    ORA(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x16: // ASL zpg,X
                    ASL(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x18: // CLC
                    SpendCycles(2);
                    C = false;
                    break;
                case 0x19: // ORA abs,Y
                    ORA(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x1D: // ORA abs,X
                    ORA(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x1E: // ASL abs,X
                    ASL(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x20: // JSR abs
                    JSR();
                    break;
                case 0x21: // AND ind,X
                    AND(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x24: // BIT zpg
                    BIT(InstructionMode.ZeroPage);
                    break;
                case 0x25: // AND zpg
                    AND(InstructionMode.ZeroPage);
                    break;
                case 0x26: // ROL zpg
                    ROL(InstructionMode.ZeroPage);
                    break;
                case 0x28: // PLP
                    PLP();
                    break;
                case 0x29: // AND #
                    AND(InstructionMode.Immediate);
                    break;
                case 0x2A: // ROL A
                    ROL(InstructionMode.Accumulator);
                    break;
                case 0x2C: // BIT abs
                    BIT(InstructionMode.Absolute);
                    break;
                case 0x2D: // AND abs
                    AND(InstructionMode.Absolute);
                    break;
                case 0x2E: // ROL abs
                    ROL(InstructionMode.Absolute);
                    break;
                case 0x30: // BMI rel
                    Branch(N);
                    break;
                case 0x31: // AND ind,Y
                    AND(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x35: // AND zpg,X
                    AND(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x36: // ROL zpg,X
                    ROL(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x38: // SEC
                    SpendCycles(2);
                    C = true;
                    break;
                case 0x39: // AND abs,Y
                    AND(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x3D: // AND abs,X
                    AND(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x3E: // ROL abs,X
                    ROL(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x40: // RTI
                    RTI();
                    break;
                case 0x41: // EOR ind,X
                    EOR(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x45: // EOR zpg
                    EOR(InstructionMode.ZeroPage);
                    break;
                case 0x46: // LSR zpg
                    LSR(InstructionMode.ZeroPage);
                    break;
                case 0x48: // PHA
                    PHA();
                    break;
                case 0x49: // EOR #
                    EOR(InstructionMode.Immediate);
                    break;
                case 0x4A: // LSR A
                    LSR(InstructionMode.Accumulator);
                    break;
                case 0x4C: // JMP abs
                    JMP(InstructionMode.Absolute);
                    break;
                case 0x4D: // EOR abs
                    EOR(InstructionMode.Absolute);
                    break;
                case 0x4E: // LSR abs,X
                    LSR(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x50: // BVC rel
                    Branch(!V);
                    break;
                case 0x51: // EOR ind,X
                    EOR(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x55: // EOR zpg,X
                    EOR(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x56: // LSR zpg,X
                    LSR(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x58: // CLI
                    SpendCycles(2);
                    I = false;
                    break;
                case 0x59: // EOR abs,Y
                    EOR(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x5D: // EOR abs,X
                    EOR(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x5E: // LSR abs,X
                    LSR(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x60: // RTS
                    RTS();
                    break;
                case 0x61: // ADC ind,Y
                    ADC(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x65: // ADC zpg
                    ADC(InstructionMode.ZeroPage);
                    break;
                case 0x66: // ROR zpg
                    ROR(InstructionMode.ZeroPage);
                    break;
                case 0x68: // PLA
                    PLA();
                    break;
                case 0x69: // ADC #
                    ADC(InstructionMode.Immediate);
                    break;
                case 0x6A: // ROR A
                    ROR(InstructionMode.Accumulator);
                    break;
                case 0x6C: // JMP ind
                    JMP(InstructionMode.Indirect);
                    break;
                case 0x6D: // ADC abs
                    ADC(InstructionMode.Absolute);
                    break;
                case 0x6E: // ROR abs
                    ROR(InstructionMode.Absolute);
                    break;
                case 0x70: // BVS rel
                    Branch(V);
                    break;
                case 0x71: // ADC ind,Y
                    ADC(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x75: // ADC zpg,X
                    ADC(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x76: // ROR zpg,X
                    ROR(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x78: // SEI
                    SpendCycles(2);
                    I = true;
                    break;
                case 0x79: // ADC abs,Y
                    ADC(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x7D: // ADC abs,X
                    ADC(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x7E: // ROR abs,X
                    ROR(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0x81: // STA ind,X
                    STA(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0x84: // STY zpg
                    STY(InstructionMode.ZeroPage);
                    break;
                case 0x85: // STA zpg
                    STA(InstructionMode.ZeroPage);
                    break;
                case 0x86: // STX zpg
                    STX(InstructionMode.ZeroPage);
                    break;
                case 0x88: // DEY
                    DEY();
                    break;
                case 0x8A: // TXA
                    TXA();
                    break;
                case 0x8C: // STY abs
                    STY(InstructionMode.Absolute);
                    break;
                case 0x8D: // STA abs
                    STA(InstructionMode.Absolute);
                    break;
                case 0x8E: // STX abs
                    STX(InstructionMode.Absolute);
                    break;
                case 0x90: // BCC rel
                    Branch(!C);
                    break;
                case 0x91: // STA ind,Y
                    STA(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0x94: // STY zpg,X
                    STY(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x95: // STA zpg,X
                    STA(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0x96: // STX zpg,Y
                    STX(InstructionMode.ZeroPage, IndexRegister.Y);
                    break;
                case 0x98: // TYA
                    TYA();
                    break;
                case 0x99: // STA abs,Y
                    STA(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0x9A: // TXS
                    TXS();
                    break;
                case 0x9D: // STA abs,X
                    STA(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xA0: // LDY #
                    LDY(InstructionMode.Immediate);
                    break;
                case 0xA1: // LDA ind,X
                    LDA(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0xA2: // LDX #
                    LDX(InstructionMode.Immediate);
                    break;
                case 0xA4: // LDY zpg
                    LDY(InstructionMode.ZeroPage);
                    break;
                case 0xA5: // LDA zpg
                    LDA(InstructionMode.ZeroPage);
                    break;
                case 0xA6: // LDX zpg
                    LDX(InstructionMode.ZeroPage);
                    break;
                case 0xA8: // TAY
                    TAY();
                    break;
                case 0xA9: // LDA #
                    LDA(InstructionMode.Immediate);
                    break;
                case 0xAA: // TAX
                    TAX();
                    break;
                case 0xAC: // LDY abs
                    LDY(InstructionMode.Absolute);
                    break;
                case 0xAD: // LDA abs
                    LDA(InstructionMode.Absolute);
                    break;
                case 0xAE: // LDX abs
                    LDX(InstructionMode.Absolute);
                    break;
                case 0xB0: // BCS rel
                    Branch(C);
                    break;
                case 0xB1: // LDA ind,X
                    LDA(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0xB4: // LDY zpg,X
                    LDY(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xB5: // LDA zpg,X
                    LDA(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xB6: // LDX zpg,Y
                    LDX(InstructionMode.ZeroPage, IndexRegister.Y);
                    break;
                case 0xB8: // CLV
                    SpendCycles(2);
                    V = false;
                    break;
                case 0xB9: // LDA abs,Y
                    LDA(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0xBA: // TSX
                    TSX();
                    break;
                case 0xBC: // LDY abs,X
                    LDY(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xBD: // LDA abs,X
                    LDA(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xBE: // LDX abs,Y
                    LDX(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0xC0: // CPY #
                    CPY(InstructionMode.Immediate);
                    break;
                case 0xC1: // CMP ind,X
                    CMP(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0xC4: // CPY zpg
                    CPY(InstructionMode.ZeroPage);
                    break;
                case 0xC5: // CMP zpg
                    CMP(InstructionMode.ZeroPage);
                    break;
                case 0xC6: // DEC zpg
                    DEC(InstructionMode.ZeroPage);
                    break;
                case 0xC8: // INY
                    INY();
                    break;
                case 0xC9: // CMP #
                    CMP(InstructionMode.Immediate);
                    break;
                case 0xCA: // DEX
                    DEX();
                    break;
                case 0xCC: // CPY abs
                    CPY(InstructionMode.Absolute);
                    break;
                case 0xCD: // CMP abs
                    CMP(InstructionMode.Absolute);
                    break;
                case 0xCE: // DEC abs
                    DEC(InstructionMode.Absolute);
                    break;
                case 0xD0: // BNE rel
                    Branch(!Z);
                    break;
                case 0xD1: // CMP ind,Y
                    CMP(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0xD5: // CMP zpg,X
                    CMP(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xD6: // DEC zpg,X
                    DEC(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xD8: // CLD
                    SpendCycles(2);
                    D = false;
                    break;
                case 0xD9: // CMP abs,Y
                    CMP(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0xDD: // CMP abs,X
                    CMP(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xDE: // DEC abs,x
                    DEC(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xE0: // CPX #
                    CPX(InstructionMode.Immediate);
                    break;
                case 0xE1: // SBC ind,X
                    SBC(InstructionMode.Indirect, IndexRegister.X);
                    break;
                case 0xE4: // CPX zpg
                    CPX(InstructionMode.ZeroPage);
                    break;
                case 0xE5: // SBC zpg
                    SBC(InstructionMode.ZeroPage);
                    break;
                case 0xE6: // INC zpg
                    INC(InstructionMode.ZeroPage);
                    break;
                case 0xE8: // INX
                    INX();
                    break;
                case 0xE9: // SBC #
                    SBC(InstructionMode.Immediate);
                    break;
                case 0xEA: // NOP
                    NOP();
                    break;
                case 0xEC: // CPX abs
                    CPX(InstructionMode.Absolute);
                    break;
                case 0xED: // SBC abs
                    SBC(InstructionMode.Absolute);
                    break;
                case 0xEE: // INC abs
                    INC(InstructionMode.Absolute);
                    break;
                case 0xF0: // BEQ rel
                    Branch(Z);
                    break;
                case 0xF1: // SBC ind,Y
                    SBC(InstructionMode.Indirect, IndexRegister.Y);
                    break;
                case 0xF5: // SBC zpg,X
                    SBC(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xF6: // INC zpg,X
                    INC(InstructionMode.ZeroPage, IndexRegister.X);
                    break;
                case 0xF8: // SED
                    SpendCycles(2);
                    D = true;
                    break;
                case 0xF9: // SBC abs,Y
                    SBC(InstructionMode.Absolute, IndexRegister.Y);
                    break;
                case 0xFD: // SBC abs,X
                    SBC(InstructionMode.Absolute, IndexRegister.X);
                    break;
                case 0xFE: // INC abs,X
                    INC(InstructionMode.Absolute, IndexRegister.X);
                    break;
                default:
                    throw new SystemException($"Invalid opcode: {opcode}");
            }
            #endregion
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