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
        internal static byte[] RAM = new byte[2048];
        internal static byte[] ROM = new byte[2048];

        internal static void LoadROMFile(string path) {
            FileStream fileStream = File.OpenRead(path);
            byte[] romFile = new byte[2048];

            fileStream.Read(romFile);
            fileStream.Close();

            ROM = romFile;
        }

        internal static byte ReadByte(ushort address) {
            if (address >= RAMLocation && address <= RAMLocation + RAM.Length) {
                return RAM[address - RAMLocation];
            }
            if (address >= ROMLocation && address <= ROMLocation + ROM.Length) {
                return ROM[address - ROMLocation];
            }
            throw new SystemException($"Address undefined in memory address space: {address}");
        }

        internal static void WriteByte(ushort address, byte value) {
            if (address > RAM.Length) {
                throw new SystemException($"Address outside of memory: {address} / {RAM.Length}");
            }
            RAM[address] = value;
        }
    }
}
