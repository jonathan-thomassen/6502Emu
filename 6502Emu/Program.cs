using static _6502Emu.Memory;
using static _6502Emu.CPU;

LoadROMFile(@"6502test.bin");
WriteByte(RESVectorHigh, 0x08);
Reset();