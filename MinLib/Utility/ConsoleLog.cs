namespace MinLib.Utility;

public static class ConsoleLog
{
    public static void EmptyLine(int lines = 1)
    {
        for (int i = 0; i < lines; i++) Console.WriteLine();
    }

    public static void InLine(object msg)
    {
        Display(msg, ConsoleColor.Gray, true);
    }

    public static void InLine(object msg, ConsoleColor color)
    {
        Display(msg, color, true);
    }

    public static void Message(object msg, bool inLine = false)
    {
        Display(msg, ConsoleColor.Gray, inLine);
    }

    public static void Highlight(object msg, bool inLine = false)
    {
        Display(msg, ConsoleColor.White, inLine);
    }

    public static void Warning(object msg, bool inLine = false)
    {
        Display(msg, ConsoleColor.Yellow, inLine);
    }

    public static void Error(object msg, bool inLine = false)
    {
        Display(msg, ConsoleColor.Red, inLine);
    }

    public static void Success(object msg, bool inLine = false)
    {
        Display(msg, ConsoleColor.Green, inLine);
    }

    public static void Title(object msg, bool inLine = false)
    {
        Display(msg, ConsoleColor.Cyan, inLine);
    }

    public static void Path(object msg, bool inLine = false)
    {
        Display(msg, ConsoleColor.DarkCyan, inLine);
    }

    public static void Ignore(object msg, bool inLine = false)
    {
        Display(msg, ConsoleColor.DarkGray, inLine);
    }

    public static void Debug(object msg)
    {
        Display(msg, ConsoleColor.Magenta);
    }

    public static void LogAction(object msg, Action action)
    {
        Message(msg, inLine: true);
        try
        {
            action.Invoke();
            Success("[DONE]");
        }
        catch (Exception e)
        {
            Error("[FAILED]");
            Error(e);
        }
    }

    private static void Display(object msg, ConsoleColor color = ConsoleColor.Gray, bool inLine = false)
    {
        Console.ForegroundColor = color;
        if (inLine)
        {
            Console.Write(msg);
        }
        else
        {
            Console.WriteLine(msg);
        }
        Console.ResetColor();
    }
}
