using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Dottik.MemeDownloader.Utilities;

public static class EnvironmentUtilities
{
    /// <summary>
    /// Creates a batch of folders from a Array of strings.
    /// </summary>
    /// <param name="folderPaths">The folder paths of the folders to create.</param>
    public static void BatchCreateFolders(string[] folderPaths)
    {
        for (int i = 0; i < folderPaths.Length; i++)
        {
            if (!Directory.Exists(folderPaths[i]))
                Directory.CreateDirectory(folderPaths[i]);
        }
    }

    /// <summary>
    /// Creates a batch of folders from a List of strings.
    /// </summary>
    /// <param name="folderPaths">The folder paths of the folders to create.</param>
    public static void BatchCreateFolders(List<string> folderPaths)
    {
        for (int i = 0; i < folderPaths.Count; i++)
        {
            if (!Directory.Exists(folderPaths[i]))
                Directory.CreateDirectory(folderPaths[i]);
        }
    }

    /// <summary>
    /// Removes characters that are not allowed in file names.
    /// </summary>
    /// <returns>a new string without illegal characters.</returns>
    public static async Task<string> SanitizeString(string fileName)
    {
        char[] stringChars = fileName.ToCharArray();

        Func<char, bool> sanitizeStringFunc
            = new(inChar =>
                    {
                        // Iterate though the illegal chars in filenames, if match return false.
                        for (int i = 0; i < Path.GetInvalidFileNameChars().Length; i++)
                        {
                            if (inChar == Path.GetInvalidFileNameChars()[i])
                                return false;
                        }
                        // If no match, return true
                        return true;
                    });

        IEnumerable<char> trueString = await Task.Run(() => stringChars.Where(sanitizeStringFunc));

        StringBuilder result = new();
        result.Append(trueString.ToArray());

        return result.ToString();
    }
}