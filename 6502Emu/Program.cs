using static _6502Emu.Registers;
using static _6502Emu.Memory;
using static _6502Emu.CPU;

LoadROMFile(@"C:\Users\jonat\Desktop\o6502-2022-05-08-194137.bin");
P = 0x36;
PrintProcessorStatus();
while (true) {
    ExecuteInstruction(ReadByte((ushort)(PC++ + ROMLocation)));
    PrintProcessorStatus();
}

void PrintProcessorStatus() {
    Console.WriteLine("CycleCount: " + CycleCount);
    Console.WriteLine("Registers:");
    Console.WriteLine($"A: {A}");
    Console.WriteLine($"X: {X}");
    Console.WriteLine($"Y: {Y}");
    Console.WriteLine($"S: 0x{Convert.ToString(S, toBase: 16)}");
    Console.WriteLine($"PC: 0x{Convert.ToString(PC, toBase: 16)}");
    Console.WriteLine("Status flags:");
    Console.WriteLine($"C: {C}");
    Console.WriteLine($"Z: {Z}");
    Console.WriteLine($"I: {I}");
    Console.WriteLine($"D: {D}");
    Console.WriteLine($"B: {B}");
    Console.WriteLine($"V: {V}");
    Console.WriteLine($"N: {N}");
}