namespace GBemulator;

public static class Helpers
{
    public static byte GetHigh(this ushort value)
    {
        value >>= 8;
        return (byte)value;
    }

    public static byte GetLow(this ushort value)
    {
        value &= 0x00ff;
        return (byte)value;
    }

    public static ushort SetHigh(this ushort value, byte highValue)
    {
        value &= 0x00ff;
        value += (ushort)(highValue << 8);
        return value;
    }

    public static ushort SetLow(this ushort value, byte lowValue)
    {
        value &= 0xff00;
        value += lowValue;
        return value;
    }

    public static bool GetBit(this ushort bits, byte place)
    {
        if (place > 15)
        {
            throw new ArgumentException("Place must be between 0-15", nameof(place));
        }

        bits >>= place;
        bits &= 1;
        return bits != 0;
    }

    public static bool GetBit(this byte bits, byte place)
    {
        if (place > 7)
        {
            throw new ArgumentException("Place must be between 0-7", nameof(place));
        }

        bits >>= place;
        bits &= 1;
        return bits != 0;
    }

    public static ushort SetBit(this ushort bits, byte place, bool value)
    {
        if (place > 15)
        {
            throw new ArgumentException("Place must be between 0-15", nameof(place));
        }

        ushort mask = (ushort)(1 << place);
        mask ^= ushort.MaxValue;
        bits &= mask;

        if (value == true)
        {
            bits += (ushort)(1 << place);
        }

        return bits;
    }

    public static byte SetBit(this byte bits, byte place, bool value)
    {
        if (place > 7)
        {
            throw new ArgumentException("Place must be between 0-7", nameof(place));
        }

        byte mask = (byte)(1 << place);
        mask ^= byte.MaxValue;
        bits &= mask;

        if (value == true)
        {
            bits += (byte)(1 << place);
        }

        return bits;
    }

    public static ushort BytesToUshort(byte mostSignificant, byte leastSignificant)
    {
        return (ushort)((mostSignificant << 8) + leastSignificant);
    }

    public static (byte, byte) UshortToBytes(ushort value)
    {
        byte leastSignificant = (byte)(value & byte.MaxValue);
        byte mostSignificant = (byte)(value >> 8);

        return (mostSignificant, leastSignificant);
    }

    public static byte RotateByteLeft(this byte value)
    {
        bool bit7 = GetBit(value, 7);
        value <<= 1;
        value = SetBit(value, 0, bit7);

        return value;
    }

    public static byte RotateByteRight(this byte value)
    {
        bool bit0 = GetBit(value, 0);
        value >>= 1;
        value = SetBit(value, 7, bit0);

        return value;
    }

    public static int ConsoleReadNumber()
    {
        string? input = Console.ReadLine();

        if (int.TryParse(input, out int result))
        {
            return result;
        }

        return 0;
    }

    // This should not be single function but I'm lazy and a file selecter isn't the goal of this project
    public static byte[] LoadROM(string path = "..")
    {
        string currentPath = Path.GetFullPath(path);
        int selectionPointer = 0;
        int displayLength = 10;
        byte[]? ROM = null;
        List<FileSystemInfo> entries = [];

        Console.CursorVisible = false;

        if (File.Exists(currentPath))
        {
            ROM = File.ReadAllBytes(currentPath);
        }
        else if (Directory.Exists(currentPath))
        {
            ChangeFolder(currentPath);
        }
        else
        {
            ChangeFolder(Path.GetFullPath(".."));
        }

        while (ROM == null)
        {
            PrintDisplay();
            ReadInput();
        }

        Console.CursorVisible = true;

        return ROM;

        void ChangeFolder(string path)
        {
            currentPath = path;
            var foldersInDir = Directory.GetDirectories(currentPath).Select(entry => new DirectoryInfo(entry));
            var filesInDir = Directory.GetFiles(currentPath).Select(entry => new FileInfo(entry));
            entries = [.. foldersInDir, .. filesInDir];
            selectionPointer = 0;
        }

        void PrintDisplay()
        {
            Console.Clear();
            Console.WriteLine("Select Test ROM");

            // Integer division truncates the decimals so this "rounds" to nearest displayLength
            int pageStart = (selectionPointer / displayLength) * displayLength;

            Console.WriteLine(currentPath);

            Console.WriteLine($"[{selectionPointer}/{entries.Count}] ");

            if (entries.Count == 0)
            {
                Console.WriteLine("The folder is empty");
                return;
            }

            for (int i = pageStart; i < pageStart + displayLength; i++)
            {
                if (i >= entries.Count)
                {
                    break;
                }

                if (i == selectionPointer)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine($"[{i - (pageStart)}] {entries[i].Name}");

                Console.ResetColor();
            }
        }

        void ReadInput()
        {
            ConsoleKeyInfo input = Console.ReadKey();

            int inputNumber = input.KeyChar - '0';

            if (inputNumber >= 0 && inputNumber <= 9)
            {
                OpenEntry(selectionPointer + inputNumber);
            }

            switch (input.Key)
            {
                case ConsoleKey.UpArrow:
                    selectionPointer--; WrapSelectionPointer(); break;

                case ConsoleKey.DownArrow:
                    selectionPointer++; WrapSelectionPointer(); break;

                case ConsoleKey.RightArrow:
                    OpenEntry(selectionPointer); break;

                case ConsoleKey.Backspace:
                case ConsoleKey.LeftArrow:
                    MoveToParentFolder(); break;

                case ConsoleKey.Escape:
                    Environment.Exit(0); break;

                default: break;
            }
        }

        void WrapSelectionPointer()
        {
            selectionPointer %= entries.Count;

            if (selectionPointer < 0)
            {
                selectionPointer += entries.Count;
            }
        }

        void OpenEntry(int selectionIndex)
        {
            var entry = entries[selectionIndex];

            if (entry is DirectoryInfo)
            {
                ChangeFolder(entry.FullName);
            }
            else if (entry is FileInfo)
            {
                ROM = File.ReadAllBytes(entry.FullName);
            }
        }

        void MoveToParentFolder()
        {
            var parentDir = Directory.GetParent(currentPath);

            if (parentDir != null)
            {
                ChangeFolder(parentDir.FullName);
            }
        }
    }

    public static Dictionary<string, string?> TokenizeArgs(string[] args)
    {
        var tokens = new Dictionary<string, string?>();

        string lastToken = "--path"; // Read the first option as a path if it doesn't start with "--"

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg.StartsWith("--"))
            {
                tokens[arg] = null;
                lastToken = arg;
            }
            else if (string.IsNullOrEmpty(lastToken) == false)
            {
                tokens[lastToken] = arg;
                lastToken = "";
            }
        }

        return tokens;
    }

    public static string? GetArgString(Dictionary<string, string?> tokens, string argName)
    {
        if (tokens.TryGetValue(argName, out var value) == false)
        {
            return null;
        }

        return value;
    }

    public static bool GetArgBool(Dictionary<string, string?> tokens, string argName)
    {
        return tokens.ContainsKey(argName);
    }

    public static int? GetArgInt(Dictionary<string, string?> tokens, string argName)
    {
        if (tokens.TryGetValue(argName, out var value) == false)
        {
            return null;
        }

        if (int.TryParse(value, out var intValue) == false)
        {
            return null;
        }

        return intValue;
    }
}
