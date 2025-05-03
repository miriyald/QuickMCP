using System.Diagnostics;

namespace QuickMCP.Helpers;

public static class PathHelper
{
    public static string GetFullPath(string fileName,string[]? additionalPaths = null)
    {
        try
        {
            if(File.Exists(fileName))
                return fileName;
            var listLookUpDirectories = new List<string>();
            if(additionalPaths != null)
                listLookUpDirectories.AddRange(additionalPaths);
            listLookUpDirectories.Add(Environment.CurrentDirectory);
            listLookUpDirectories.Add(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty) ?? string.Empty);
            listLookUpDirectories.Add(Directory.GetCurrentDirectory());
           
            var path = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(path))
            {
                listLookUpDirectories.AddRange(path.Split("\r\n,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            }

            var name = Path.GetFileName(fileName);
            foreach (var directory in listLookUpDirectories)
            {
                var fullPath = Path.Combine(directory, name);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
                var fullPath2 = Path.Combine(directory, fileName);
                if (File.Exists(fullPath2))
                {
                    return fullPath2;
                }
            }
        }
        catch
        {
            
        }
        return fileName;
    }
}