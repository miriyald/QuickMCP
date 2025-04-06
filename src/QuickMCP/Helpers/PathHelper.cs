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
            }
        }
        catch
        {
            
        }
        return fileName;
    }
}