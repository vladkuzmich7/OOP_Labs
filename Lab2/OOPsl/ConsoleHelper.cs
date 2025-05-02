using System;
using System.Runtime.InteropServices;

public static class ConsoleHelper
{
    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
    private const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
    private const uint ENABLE_MOUSE_INPUT = 0x0010;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    public static void DisableQuickEditMode()
    {
        IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
        if (!GetConsoleMode(consoleHandle, out uint mode))
        {
            Console.WriteLine("Не удалось получить режим консоли.");
            return;
        }
        mode &= ~ENABLE_QUICK_EDIT_MODE;
        mode &= ~ENABLE_MOUSE_INPUT;
        mode |= ENABLE_EXTENDED_FLAGS;
        if (!SetConsoleMode(consoleHandle, mode))
        {
            Console.WriteLine("Не удалось установить режим консоли.");
        }
    }
}