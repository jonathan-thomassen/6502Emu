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
        internal static byte[] RAM = new byte[4096];
        internal static byte[] ROM = new byte[4096];

        internal static void LoadROMFile(string path) {
            FileStream fileStream = File.OpenRead(path);
            byte[] empty = new byte[0x800];
            byte[] romFile = new byte[4096];

            fileStream.Read(romFile);
            fileStream.Close();

            byte[] finalRom = new byte[empty.Length + romFile.Length];
            empty.CopyTo(finalRom, 0);
            romFile.CopyTo(finalRom, empty.Length);

            ROM = finalRom;
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
