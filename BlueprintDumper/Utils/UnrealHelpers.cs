using CUE4Parse.Utils;

namespace BlueprintDumper.Utils
{
    public static class UnrealHelpers
    {
        public static string GetAssetName(this string AssetPath)
        {
            /** if AssetPath in the following format: /Game/Meshes/SM_MyMesh */
            if (AssetPath.Contains('/'))
                return AssetPath.SubstringAfterLast('/');

            /** if AssetPath in the following format: ...\Content\Meshes\SM_MyMesh */
            else if (AssetPath.Contains('\\'))
                return AssetPath.SubstringAfterLast('\\').SubstringBeforeLast('.');

            return "";
        }

        public static string GetPackagePath(this string PackageName)
        {
            return "DeadByDaylight/Content/" + PackageName.Substring(6);
        }

        public static string GetPackageNameFromFilePath(this string FilePath)
        {
            return "/Game" + FilePath.SubstringAfter("Content").SubstringBeforeLast('.').Replace('\\', '/');
        }

        public static string[] GetAllPackages(this string AssetFilePath)
        {
            string AssetDirectory = AssetFilePath.SubstringBeforeLast('\\');
            string AssetFileName = AssetFilePath.SubstringAfterLast('\\').SubstringBeforeLast('.');

            return Directory.GetFiles(AssetDirectory, AssetFileName);
        }
    }
}
