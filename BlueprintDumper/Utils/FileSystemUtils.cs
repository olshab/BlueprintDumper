namespace BlueprintDumper.Utils
{
    public static class FileSystemUtils
    {
        public static void CopyDirectory(string SourceDir, string DestDir)
        {
            DirectoryInfo source = new DirectoryInfo(SourceDir);
            DirectoryInfo destination = new DirectoryInfo(DestDir);

            if (!destination.Exists)
            {
                destination.Create();
            }

            // Copy all files with overwriting
            FileInfo[] files = source.GetFiles();
            foreach (FileInfo file in files)
            {
                file.CopyTo(Path.Combine(destination.FullName,
                    file.Name), true);
            }

            // Subdirectories
            foreach (DirectoryInfo dir in source.EnumerateDirectories())
            {
                string destinationDir = Path.Combine(destination.FullName, dir.Name);
                CopyDirectory(dir.FullName, destinationDir);
            }
        }

        public static void DeleteEmptyDirs(string? CurrentDir)
        {
            if (CurrentDir is null)
                return;

            if (Directory.EnumerateFiles(CurrentDir, "*.*", SearchOption.AllDirectories).Count() == 0)
            {
                Directory.Delete(CurrentDir, true);
                return;
            }

            foreach (string di in Directory.EnumerateDirectories(CurrentDir))
                DeleteEmptyDirs(di);
        }
    }
}
