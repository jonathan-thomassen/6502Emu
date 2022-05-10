using static _6502Emu.CPU;

namespace _6502Emu {
    internal class InputOutput {
        internal void SetIRQ() {
            IRQ = true;
        }

        internal void SetNMI() {
            NMI = true;
        }

        internal void ClearIRQ() {
            IRQ = false;
        }

        internal void ClearNMI() {
            NMI = false;
        }

        void MainInputOutputLoop() {

        }
    }
}
