namespace _6502Emu {
    internal static class Registers {
        internal static byte A;
        internal static byte X;
        internal static byte Y;
        internal static byte P;
        internal static byte S = 0xFF;
        internal static ushort PC;

        internal static bool C {
            get => (P & 0b0000_0001) == 0b0000_0001;
            set {
                if (value) {
                    P |= 0b0000_0001;
                } else {
                    P = (byte)~(~P | 0b0000_0001);
                }
            }
        }

        internal static bool Z {
            get => (P & 0b0000_0010) == 0b0000_0010;
            set {
                if (value) {
                    P |= 0b0000_0010;
                } else {
                    P = (byte)~(~P | 0b0000_0010);
                }
            }
        }

        internal static bool I {
            get => (P & 0b0000_0100) == 0b0000_0100;
            set {
                if (value) {
                    P |= 0b0000_0100;
                } else {
                    P = (byte)~(~P | 0b0000_0100);
                }
            }
        }

        internal static bool D {
            get => (P & 0b0000_1000) == 0b0000_1000;
            set {
                if (value) {
                    P |= 0b0000_1000;
                } else {
                    P = (byte)~(~P | 0b0000_1000);
                }
            }
        }

        internal static bool B {
            get => (P & 0b0001_0000) == 0b0001_0000;
            set {
                if (value) {
                    P |= 0b0001_0000;
                } else {
                    P = (byte)~(~P | 0b0001_0000);
                }
            }
        }

        internal static bool V {
            get => (P & 0b0100_0000) == 0b0100_0000;
            set {
                if (value) {
                    P |= 0b0100_0000;
                } else {
                    P = (byte)~(~P | 0b0100_0000);
                }
            }
        }

        internal static bool N {
            get => (P & 0b1000_0000) == 0b1000_0000;
            set {
                if (value) {
                    P |= 0b1000_0000;
                } else {
                    P = (byte)~(~P | 0b1000_0000);
                }
            }
        }
    }
}
