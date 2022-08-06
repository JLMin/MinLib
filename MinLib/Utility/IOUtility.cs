using System.Text.Json;
using MinLib.Extension;

namespace MinLib.Utility;

public static class IOUtility
{
    #region Paths

    /// <summary>
    /// Gets the application root directory.
    /// </summary>
    public static readonly string ApplicationRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.ToString();

    /// <summary>
    /// Gets the folder application root directory.
    /// </summary>
    public static string ApplicationFolder(string folder)
    {
        return Path.GetFullPath(Path.Combine(ApplicationRoot, folder));
    }

    /// <summary>
    /// Gets the absolute path for the specified path with its parent.
    /// </summary>
    public static string GetAbsolutePath(string relativeTo, string path)
    {
        path = path.StartsWith("\\") ? path[1..] : path;
        return Path.GetFullPath(Path.Combine(relativeTo, path));
    }

    /// <summary>
    /// Gets the relative path from one path to another.
    /// </summary>
    public static string GetRelativePath(string relativeTo, string path)
    {
        return Path.GetRelativePath(relativeTo, path);
    }

    #endregion

    #region Files

    public static IEnumerable<string> GetFilesWithExtension(DirectoryInfo root, string ext)
    {
        string pattern = $"*.{ext}";

        foreach (FileInfo? source in root.GetFiles(pattern))
        {
            yield return source.ToString();
        }

        foreach (DirectoryInfo? dir in root.GetDirectories())
        {
            foreach (string? source in GetFilesWithExtension(dir, pattern))
            {
                yield return source;
            }
        }
    }

    /// <summary>
    /// Create a file.
    /// </summary>
    public static FileStream Create(string path)
    {
        CreateParentFolder(path);
        return File.Create(path);
    }

    /// <summary>
    /// Create a file (if not exists) and write lines to in it as a txt file.
    /// </summary>
    public static void CreateAndWriteLines(string path, IEnumerable<string> lines)
    {
        if (!File.Exists(path))
        {
            CreateParentFolder(path);
            using StreamWriter writer = File.CreateText(path);
            foreach (string? line in lines)
            {
                writer.WriteLine(line);
            }
        }
        else
        {
            File.WriteAllLines(path, lines);
        }
    }

    /// <summary>
    /// Copy a file from source to destination.
    /// </summary>
    public static void CopyFile(string from, string to)
    {
        if (!File.Exists(from))
        {
            throw new FileNotFoundException(from);
        }
        CreateParentFolder(to);
        File.Copy(from, to, overwrite: true);
    }

    /// <summary>
    /// Reads all lines from a 'txt' file.
    /// Empty lines and lines that starts with '#' will be ignored.
    /// The search path is the root directory of this assembly.
    /// </summary>
    public static IEnumerable<string> ReadText(string name)
    {
        string path = FileFullPath(name, "txt");
        return File.ReadLines(path)
                   .Where(x => !string.IsNullOrEmpty(x.Trim()));
    }

    /// <summary>
    /// Writes a IEnumerable of lines into a 'txt' file.
    /// The search path is the root directory of this assembly.
    /// </summary>
    public static void WriteText(string name, IEnumerable<string> lines)
    {
        string path = FileFullPath(name, "txt");
        if (!File.Exists(path))
        {
            using FileStream? _ = Create(path);
        }
        File.WriteAllLines(path, lines);
    }

    /// <summary>
    /// Reads the string content from a 'json' file.
    /// The search path is the root directory of this assembly.
    /// </summary>
    public static string ReadJson(string name)
    {
        string path = FileFullPath(name, "json");
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Writes a string object into a 'json' file.
    /// The search path is the root directory of this assembly.
    /// </summary>
    public static void WriteJson(string name, string json)
    {
        string path = FileFullPath(name, "json");
        if (!File.Exists(path))
        {
            using FileStream? _ = Create(path);
        }
        File.WriteAllText(path, json);
    }

    private static string FileFullPath(string name, string extension)
    {
        name = name.EndsWithIgnoreCase(extension)
                    ? name
                    : $"{name}.{extension}";

        return Path.Combine(ApplicationRoot, name);
    }

    public static T? DeserializeJson<T>(string sourcePath)
    {
        string json = ReadJson(sourcePath);

        return JsonSerializer.Deserialize<T>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    public static async Task<T?> DeserializeJsonAsync<T>(string sourcePath)
    {
        using FileStream stream = File.OpenRead(sourcePath);

        return await JsonSerializer.DeserializeAsync<T>(stream,
            new JsonSerializerOptions
            {
                MaxDepth = 1024,
                PropertyNameCaseInsensitive = true,
            });
    }

    private static void CreateParentFolder(string file)
    {
        string? folder = Directory.GetParent(file)?.FullName;
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    #endregion
}
