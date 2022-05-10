using static _6502Emu.Memory;
using static _6502Emu.CPU;

LoadROMFile(@"C:\Users\jonat\Desktop\o6502-2022-05-08-194137.bin");
WriteByte(RESVectorHigh, 0x08);
Reset();