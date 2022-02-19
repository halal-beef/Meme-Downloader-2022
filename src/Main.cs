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

                case ProgramModes.SETUP:

                    break;
            }
        }
    }

    static ProgramModes ParseArguments(string[] bootArgs)
    {
        for (int i = 0; i < bootArgs.Length; i++)
        {
            if (bootArgs.Contains("--setup"))
            {
                return ProgramModes.SETUP;
            }
        }
        return ProgramModes.NORMAL;
    }
}