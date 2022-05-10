namespace _6502Emu {
    internal static class Memory {
        internal const ushort RAMLocation = 0x0000;
        internal const ushort StackLocation = 0x0100;
        internal const ushort IOLocation = 0x4000;
        internal const ushort ROMLocation = 0x8000;
        internal const ushort ROMOffset = 0x800;

        internal const ushort NMIVectorLow = 0xFFFA;
        internal const ushort NMIVectorHigh = 0xFFFB;
        internal const ushort RESVectorLow = 0xFFFC;
        internal const ushort RESVectorHigh = 0xFFFD;
        internal const ushort IRQVectorLow = 0xFFFE;
        internal const ushort IRQVectorHigh = 0xFFFF;

        internal static byte[] Map = new byte[0x10000];

        internal static void LoadROMFile(string path) {
            FileStream fileStream = File.OpenRead(path);
            byte[] romFile = new byte[4096];

            fileStream.Read(romFile);
            fileStream.Close();

            romFile.CopyTo(Map, ROMLocation + ROMOffset);
        }

        internal static byte ReadByte(ushort address) {
            return Map[address];
        }

        internal static void WriteByte(ushort address, byte value) {
            Map[address] = value;
        }
    }
}
