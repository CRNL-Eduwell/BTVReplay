using System.IO;

namespace BrainTV.Tools
{
    /// <summary>
    /// Crash-safe file writing: content is fully written to a temporary file first, then
    /// swapped in, so the destination is never left empty or truncated by a failed write.
    /// </summary>
    public static class AtomicFile
    {
        public static void WriteAllText(string path, string contents)
        {
            string tmpPath = path + ".tmp";
            File.WriteAllText(tmpPath, contents);
            if (File.Exists(path))
                File.Replace(tmpPath, path, null);
            else
                File.Move(tmpPath, path);
        }
    }
}
