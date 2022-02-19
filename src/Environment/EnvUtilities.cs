using System.Collections.Generic;
using System.IO;

namespace Dottik.MemeDownloader.Utilities;

public class EnvironmentUtilities
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
}