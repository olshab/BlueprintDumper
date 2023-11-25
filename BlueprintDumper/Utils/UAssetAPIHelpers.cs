using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using CUE4Parse.Utils;

namespace BlueprintDumper.Utils
{
    public static class UAssetAPIHelpers
    {
        public static T? FindPropertyByName<T>(this NormalExport ExportToSearchIn, string PropertyName) where T : PropertyData
        {
            foreach (PropertyData data in ExportToSearchIn.Data)
                if (data.Name.ToString() == PropertyName)
                    return (T)data;

            return null;
        }

        public static T FindPropertyByNameChecked<T>(this NormalExport ExportToSearchIn, string PropertyName) where T : PropertyData
        {
            foreach (PropertyData data in ExportToSearchIn.Data)
                if (data.Name.ToString() == PropertyName)
                    return (T)data;

            throw new Exception($"Failed to find property {PropertyName} in export {ExportToSearchIn.ObjectName.ToString()}");
        }

        public static T? FindPropertyByName<T>(this StructPropertyData StructToSearchIn, string PropertyName) where T : PropertyData
        {
            foreach (PropertyData data in StructToSearchIn.Value)
                if (data.Name.ToString() == PropertyName)
                    return (T)data;

            return null;
        }

        public static T FindPropertyByNameChecked<T>(this StructPropertyData StructToSearchIn, string PropertyName) where T : PropertyData
        {
            foreach (PropertyData data in StructToSearchIn.Value)
                if (data.Name.ToString() == PropertyName)
                    return (T)data;

            throw new Exception($"Failed to find property {PropertyName} in export {StructToSearchIn.Name.ToString()}");
        }

        public static NormalExport? FindExportByClassName(this UAsset Asset, string ClassName)
        {
            for (int i = 0; i < Asset.Exports.Count; i++)
            {
                Export export = Asset.Exports[i];

                if (!(export is NormalExport))
                    continue;

                if (!export.ClassIndex.IsImport())
                    continue;

                Import Class = export.ClassIndex.ToImport(Asset);
                if (Class.ObjectName.ToString() == ClassName)
                    return (NormalExport)export;
            }

            return null;
        }

        public static Import GetOutermostPackage(this Import InImport, UAsset Asset)
        {
            Import CurrentImport = InImport;

            while (!CurrentImport.OuterIndex.IsNull())
                CurrentImport = CurrentImport.OuterIndex.ToImport(Asset);

            return CurrentImport;
        }

        public static FPackageIndex? FindExportWithObjectName(this UAsset Asset, string ObjectName)
        {
            for (int i = 0; i < Asset.Exports.Count; i++)
            {
                if (Asset.Exports[i].ObjectName.ToString() == ObjectName)
                    return FPackageIndex.FromExport(i);
            }

            return null;
        }

        public static FPackageIndex? FindImportWithObjectName(this UAsset Asset, string ObjectName)
        {
            for (int i = 0; i < Asset.Imports.Count; i++)
            {
                if (Asset.Imports[i].ObjectName.ToString() == ObjectName)
                    return FPackageIndex.FromImport(i);
            }

            return null;
        }

        public static FPackageIndex GetPackageIndex(this Export Export, UAsset Asset)
        {
            for (int i = 0; i < Asset.Exports.Count; i++)
            {
                if (Asset.Exports[i] == Export)
                    return FPackageIndex.FromExport(i);
            }

            return new FPackageIndex();
        }

        public static FPackageIndex GetPackageIndex(this Import Import, UAsset Asset)
        {
            for (int i = 0; i < Asset.Imports.Count; i++)
            {
                if (Asset.Imports[i] == Import)
                    return FPackageIndex.FromImport(i);
            }

            return new FPackageIndex();
        }

        public static FPackageIndex? CreateImportFromSoftObjectRef(this UAsset Asset, string SoftObjectRef, string ClassName)
        {
            // Sanity check
            if (!SoftObjectRef.Contains('.') || ClassName == string.Empty)
                return null;

            string PackageName = SoftObjectRef.SubstringBeforeLast('.');
            string ObjectName = SoftObjectRef.SubstringAfterLast('.');

            FPackageIndex? ObjectImport = Asset.FindImportWithObjectName(PackageName);
            
            /** If Package import for such object already exists */
            //if (ObjectImport is not null)
            //    return ObjectImport;

            Import PackageImport = new Import("/Script/CoreUObject", "Package", new FPackageIndex(0), PackageName, false, Asset);
            FPackageIndex PackageImportIdx = Asset.AddImport(PackageImport);

            Import AssetImport = new Import("/Script/Engine", ClassName, PackageImportIdx, ObjectName, false, Asset);
            FPackageIndex AssetImportIdx = Asset.AddImport(AssetImport);

            return AssetImportIdx;
        }
    }
}
