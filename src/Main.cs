using Spectre.Console;
using System;
using System.Linq;
namespace Dottik.MemeDownloader;

public class MainActivity
{
    /// <summary>
    /// The program's main entry point.
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        if (args is not null && args.Length >= 1)
        {
            switch (ParseArguments(args))
            {
                // Print help & exit
                case ProgramModes.HELP:
                    Utils.PrintHelp();
                    Environment.Exit(0);
                    break;
                // Run initial setup & exit
                case ProgramModes.SETUP:

                    Environment.Exit(0);
                    break;
            }
        }
    }

    static ProgramModes ParseArguments(string[] bootArgs)
    {
        for (int i = 0; i < bootArgs.Length; i++)
        {
            if (bootArgs.Contains("--setup") || bootArgs.Contains("-setup")) {
                return ProgramModes.SETUP;
            } else if (bootArgs.Contains("--help") || bootArgs.Contains("-help")) { 
                return ProgramModes.HELP;
            }
        }
        return ProgramModes.NORMAL;
    }
}
public class Utils
{
    public static void PrintHelp()
    {
        AnsiConsole.MarkupLine(
            "||||----------------------||||\r\n" +
            "|||| [red]Meme Downloader 2022[/] ||||\r\n" +
            "||||----------------------||||\r\n" +
            "\r\n" +
            " [green]-setup[/] = Run initial program setup.\r\n" +
            "\r\n" +
            " [green]-help[/] = Print this message.\r\n"
            );
    }
}