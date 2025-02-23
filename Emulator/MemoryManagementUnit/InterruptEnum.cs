namespace GBemulator.MemoryManagementUnit;

// https://gbdev.io/pandocs/Interrupts.html
public enum Interrupt
{
    None = 0b0000_0000,
    VBlank = 0b0000_0001,
    LCD = 0b0000_0010,
    Timer = 0b0000_0100,
    Serial = 0b0000_1000,
    Joypad = 0b0001_0000,
}