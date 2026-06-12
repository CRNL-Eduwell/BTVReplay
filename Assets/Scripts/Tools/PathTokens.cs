using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Converts between the absolute paths the application works with in memory and the portable
/// token form stored in patient bases on disk: "${NAME}/relative/part". Tokens only ever exist
/// in the persisted files - expansion happens at load, tokenization at save - so nothing else
/// in the application has to know about them.
///
/// "${APPCONFIG}" is built in and resolves to the application's Config folder, which fixes the
/// MNI mesh paths embedded in every base without any configuration. All other roots are named
/// by the user in the preferences (machine-specific locations, e.g. a clinical data share).
/// </summary>
public static class PathTokens
{
    public const string AppConfigToken = "APPCONFIG";

    private const string TokenPrefix = "${";
    private const char TokenSuffix = '}';

    /// <summary>
    /// Expands a stored path to an absolute one. Non-token paths (legacy absolute paths) are
    /// returned untouched. A token whose root is not configured on this machine is returned
    /// as-is and a warning names the missing root, so the resulting "file not found" is
    /// explainable.
    /// </summary>
    public static string Resolve(string storedPath, IReadOnlyList<PathRoot> roots, string appConfigPath)
    {
        if (string.IsNullOrEmpty(storedPath) || !storedPath.StartsWith(TokenPrefix, StringComparison.Ordinal))
            return storedPath;

        int closingIndex = storedPath.IndexOf(TokenSuffix);
        if (closingIndex < 0)
            return storedPath;

        string name = storedPath.Substring(TokenPrefix.Length, closingIndex - TokenPrefix.Length);
        string remainder = storedPath.Substring(closingIndex + 1).TrimStart('/', '\\');

        string root = name.Equals(AppConfigToken, StringComparison.OrdinalIgnoreCase)
            ? appConfigPath
            : FindRootPath(roots, name);

        if (string.IsNullOrEmpty(root))
        {
            UnityEngine.Debug.LogWarning("PathTokens: the path root \"" + name + "\" is not configured on this machine (stored path: " + storedPath + "). Add it in the user preferences to resolve this path.");
            return storedPath;
        }

        return CombineNative(root, remainder);
    }

    /// <summary>
    /// Converts an absolute path under a known root into its portable "${NAME}/..." form. The
    /// longest matching root wins (so a root nested inside another takes precedence). Paths
    /// under no known root are returned untouched and stay absolute in the saved file.
    /// </summary>
    public static string Tokenize(string absolutePath, IReadOnlyList<PathRoot> roots, string appConfigPath)
    {
        if (string.IsNullOrEmpty(absolutePath) || absolutePath.StartsWith(TokenPrefix, StringComparison.Ordinal))
            return absolutePath;

        string bestName = null;
        string bestRemainder = null;
        int bestLength = -1;

        void Consider(string name, string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath)) return;
            string remainder = RelativeTo(absolutePath, rootPath);
            if (remainder == null) return;
            if (rootPath.Length > bestLength)
            {
                bestLength = rootPath.Length;
                bestName = name;
                bestRemainder = remainder;
            }
        }

        Consider(AppConfigToken, appConfigPath);
        if (roots != null)
        {
            foreach (PathRoot root in roots)
            {
                if (root != null && !string.IsNullOrEmpty(root.Name))
                    Consider(root.Name, root.Path);
            }
        }

        if (bestName == null)
            return absolutePath;

        return TokenPrefix + bestName + TokenSuffix + "/" + bestRemainder;
    }

    private static string FindRootPath(IReadOnlyList<PathRoot> roots, string name)
    {
        if (roots == null) return null;
        for (int i = 0; i < roots.Count; i++)
        {
            PathRoot root = roots[i];
            if (root != null && string.Equals(root.Name, name, StringComparison.OrdinalIgnoreCase))
                return root.Path;
        }
        return null;
    }

    // Returns the path of "path" relative to "root" with forward slashes, or null when path is
    // not under root. Comparison is separator-agnostic and case-insensitive (Windows paths and
    // the default macOS file system are case-insensitive; a false negative would merely leave
    // the path absolute).
    private static string RelativeTo(string path, string root)
    {
        string normalizedPath = path.Replace('\\', '/');
        string normalizedRoot = root.Replace('\\', '/').TrimEnd('/');

        if (normalizedPath.Length <= normalizedRoot.Length + 1)
            return null;
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            return null;
        if (normalizedPath[normalizedRoot.Length] != '/')
            return null;

        return normalizedPath.Substring(normalizedRoot.Length + 1);
    }

    private static string CombineNative(string root, string remainder)
    {
        string trimmedRoot = root.TrimEnd('/', '\\');
        if (string.IsNullOrEmpty(remainder))
            return trimmedRoot;
        char separator = Path.DirectorySeparatorChar;
        return trimmedRoot + separator + remainder.Replace('/', separator).Replace('\\', separator);
    }
}
