using System.IO;

public static class FileSystemExtensions
{
    public static void CopyFilesRecursively(this DirectoryInfo source, DirectoryInfo target)
    {
        if (!source.Exists) return;

        if (!target.Exists) Directory.CreateDirectory(target.FullName);

        foreach (DirectoryInfo dir in source.GetDirectories())
            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
        foreach (FileInfo file in source.GetFiles())
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
    }
    public static string CopyToDirectory(this string path, DirectoryInfo targetDirectory, bool overwrite = false)
    {
        if (string.IsNullOrEmpty(path)) return "";

        FileInfo file = new FileInfo(path);

        if (!file.Exists) return path;
        if (!targetDirectory.Exists) return path;

        string newFilePath = Path.Combine(targetDirectory.FullName, file.Name);
        if (new FileInfo(newFilePath).Exists) return newFilePath;

        File.Copy(file.FullName, newFilePath, overwrite);
        return newFilePath;
    }
}