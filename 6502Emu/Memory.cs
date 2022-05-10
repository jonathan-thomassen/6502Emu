using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6502Emu {
    internal static class Memory {
        internal const ushort RAMLocation = 0x0000;
        internal const ushort StackLocation = 0x0100;
        internal const ushort IOLocation = 0x4000;
        internal const ushort ROMLocation = 0x8000;
        internal const ushort InterruptLocation = 0xFFFA;
        internal static byte[] Map = new byte[0x10000];

        internal static void LoadROMFile(string path) {
            FileStream fileStream = File.OpenRead(path);
            byte[] romFile = new byte[4096];

            fileStream.Read(romFile);
            fileStream.Close();

            romFile.CopyTo(Map, 0x8800);
        }

        internal static byte ReadByte(ushort address) {
            return Map[address];
        }

        internal static void WriteByte(ushort address, byte value) {
            Map[address] = value;
        }
    }
}
