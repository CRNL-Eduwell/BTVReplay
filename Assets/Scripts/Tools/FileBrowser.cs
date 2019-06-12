using System;
using System.Text;
using System.Runtime.InteropServices;
using SFB;
using System.IO;
using UnityEngine;

public class FileBrowser
{
    /// <summary>
    /// Open a qt file dialog and return the path of an existing directory.
    /// </summary>
    /// <param name="message"> message to be displayed in top of the file dialog </param>
    /// <param name="defaultDir"> default directory of the file dialog </param>
    /// <returns> return an empty path if no directory has been choosen or if an error occurs </returns>
    static public string getExistingDirectory(string message = "Select a directory", string defaultDir = "")
    {
        string[] paths = StandaloneFileBrowser.OpenFolderPanel(message, defaultDir, false);
        return paths.Length > 0 ? paths[0] : string.Empty;
    }

    /// <summary>
    /// Open a qt file dialog and return the path of a selected file.
    /// </summary>
    /// <param name="filtersArray"> extension filters of the files of be displayed in the file dialog (ex: filtersArray[0] = "txt", filtersArray[1] = "png" ...) </param>
    /// <param name="message">  message to be displayed in top of the file dialog  </param>
    /// <param name="defaultDir"> default directory of the file dialog </param>
    /// <returns> return an empty path if no file has been choosen or if an error occurs </returns>
    static public string getOpenFileName(string[] filtersArray = null, string message = "Select a file", string defaultDir = "")
    {
        string directory = string.IsNullOrEmpty(defaultDir) ? new DirectoryInfo(Application.dataPath).Parent.FullName : new DirectoryInfo(defaultDir).FullName;
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel(message, directory, new SFB.ExtensionFilter[] { new SFB.ExtensionFilter("Files", filtersArray) }, false);
        return paths.Length > 0 ? paths[0] : string.Empty;
    }

    /// <summary>
    /// Open a qt file dialog and return the list of path of the selected files.
    /// </summary>
    /// <param name="filtersArray">  extension filters of the files of be displayed in the file dialog (ex: filtersArray[0] = "txt", filtersArray[1] = "png" ...) </param>
    /// <param name="message"> message to be displayed in top of the file dialog </param>
    /// <param name="defaultDir"> default directory of the file dialog </param>
    /// <returns> return an empty path if no file has been choosen or if an error occurs </returns>
    static public string[] getOpenFilesName(string[] filtersArray = null, string message = "Select files", string defaultDir = "")
    {
        string directory = string.IsNullOrEmpty(defaultDir) ? new DirectoryInfo(Application.dataPath).Parent.FullName : new DirectoryInfo(defaultDir).FullName;
        var paths = SFB.StandaloneFileBrowser.OpenFilePanel(message, directory, new SFB.ExtensionFilter[] { new SFB.ExtensionFilter("Files", filtersArray) }, true);
        return paths;
    }

    /// <summary>
    /// Open a qt file dialog and return the path of a saved file
    /// </summary>
    /// <param name="filtersArray"> extension filters of the files of be displayed in the file dialog (ex: filtersArray[0] = "txt", filtersArray[1] = "png" ...) </param>
    /// <param name="message">  message to be displayed in top of the file dialog  </param>
    /// <param name="defaultDir"> default directory of the file dialog </param>
    /// <returns> return an empty path if no file has been choosen or if an error occurs </returns>
    static public string getSaveFileName(string[] filtersArray = null, string message = "Save to", string defaultDir = "", string defaultName = "")
    {
        string directory = string.IsNullOrEmpty(defaultDir) ? new DirectoryInfo(Application.dataPath).Parent.FullName : new DirectoryInfo(defaultDir).FullName;
        var path = SFB.StandaloneFileBrowser.SaveFilePanel(message, directory, defaultName, new SFB.ExtensionFilter[] { new SFB.ExtensionFilter("Files", filtersArray) });
        return path;
    }
}
