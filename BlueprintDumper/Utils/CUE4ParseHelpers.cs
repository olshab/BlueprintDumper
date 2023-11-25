using CUE4Parse.FileProvider;
using CUE4Parse.Utils;

namespace BlueprintDumper.Utils
{
    public static class CUE4ParseHelpers
    {
        public static string? ExtractAsset(this DefaultFileProvider Provider, string PackagePath, string ExportDirectory)
        {
            string? SavedAsset = null;

            // Skip extracting if that asset is already extracted
            if (Directory.Exists(ExportDirectory))
                if (File.Exists($"{ExportDirectory}\\{PackagePath.GetAssetName()}.uasset"))
                    return null;

            bool bIsUmap = false;

            if (Provider.TrySavePackage(PackagePath, out var Packages))
            {
                foreach ((string FilePath, byte[] Data) in Packages)
                {
                    if (!Directory.Exists(ExportDirectory))
                        Directory.CreateDirectory(ExportDirectory);

                    File.WriteAllBytes($"{ExportDirectory}\\{FilePath.SubstringAfterLast('/')}", Data);
                    SavedAsset = $"{ExportDirectory}\\{FilePath.SubstringAfterLast('/').SubstringBeforeLast('.')}";

                    if (FilePath.SubstringAfterLast('.') == "umap")
                        bIsUmap = true;
                }
            }

            if (bIsUmap)
                return SavedAsset + ".umap";
            else
                return SavedAsset + ".uasset";
        }

        public static bool IsBlueprintChildOfAActor(this DefaultFileProvider Provider, string BlueprintPackagePath)
        {
            var allExports = Provider.LoadAllObjects(BlueprintPackagePath);
            foreach (var obj in allExports)
            {
                if (obj.Name.Contains("SimpleConstructionScript"))
                    return true;
            }

            return false;
        }
    }
}
