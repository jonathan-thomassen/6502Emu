using _6502Emu;
using static _6502Emu.Registers;
using static _6502Emu.Memory;
using static _6502Emu.CPU;

LoadROMFile(@"C:\Users\jonat\Desktop\o6502-2022-05-08-194137.bin");
P = 0x36;
WriteByte(0xFFFD, 0x08);
Reset();
PrintProcessorStatus();
while (true) {
    ExecuteInstruction(ReadByte((ushort)(PC++ + ROMLocation)));
    PrintProcessorStatus();
}

void PrintProcessorStatus() {
    Console.WriteLine("CycleCount: 0d" + CycleCount);
    Console.WriteLine("Registers:");
    Console.WriteLine($"A: 0d{A}");
    Console.WriteLine($"X: 0d{X}");
    Console.WriteLine($"Y: 0d{Y}");
    Console.WriteLine($"S: 0x{Convert.ToString(S, toBase: 16)}");
    Console.WriteLine($"PC: 0x{Convert.ToString(PC, toBase: 16)}");
    Console.WriteLine($"P: 0x{Convert.ToString(P, toBase: 16)}");
    Console.WriteLine("Status flags:");
    Console.WriteLine($"C: {(C ? "1" : "0")}");
    Console.WriteLine($"Z: {(Z ? "1" : "0")}");
    Console.WriteLine($"I: {(I ? "1" : "0")}");
    Console.WriteLine($"D: {(D ? "1" : "0")}");
    Console.WriteLine($"B: {(B ? "1" : "0")}");
    Console.WriteLine($"V: {(V ? "1" : "0")}");
    Console.WriteLine($"N: {(N ? "1" : "0")}");
}