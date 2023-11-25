using BlueprintDumper.BlueprintGeneratedClass;
using BlueprintDumper.Serialization;
using BlueprintDumper.Utils;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.Utils;
using CUE4Parse_Conversion;
using CUE4Parse_Conversion.Textures;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace BlueprintDumper
{
    public static class Program
    {
        public static UAsset? Asset = null;
        public static DefaultFileProvider? Provider = null;

        public static List<string> AlreadyExportedTextures = new List<string>();  // used in ExportTexturesFromMaterial method
        public static List<string> AlreadyListedMaterials = new List<string>();
        
        // used in ExportReferencedMeshes/Blueprints: pairs AssetName - PackagePath (i.e. SM_Mesh - /Game/Meshes/SM_Mesh)
        public static Dictionary<string, string> AlreadyExistingAssets = new Dictionary<string, string>();

        public static HashSet<FPackageIndex> ReferencedAssets = new HashSet<FPackageIndex>();

        public static HashSet<string> ReferencedBlueprints = new HashSet<string>();
        public static HashSet<string> ReferencedStaticMeshes = new HashSet<string>();
        public static HashSet<string> ReferencedMaterials = new HashSet<string>();

        static void Main(string[] args)
        {
            /** First of all, get all settings */
            if (DumpSettings.bUseSettingsFromFiles)
            {
                if (!File.Exists("DumpSettings/Settings.txt"))
                    throw new Exception("Failed to find settings text file");
                DumpSettings.ReadSettingsFromFile("DumpSettings/Settings.txt");

                if (!File.Exists("DumpSettings/IgnoreExistingAssetsAtPath.txt"))
                    throw new Exception("Failed to find IgnoreExistingAssetsAtPath text file");
                DumpSettings.LoadStringsFromFileIntoArr("DumpSettings/IgnoreExistingAssetsAtPath.txt", ref DumpSettings.IgnoreExistingAssetsAtPath);

                if (!File.Exists("DumpSettings/PossibleBaseColorParameters.txt"))
                    throw new Exception("Failed to find PossibleBaseColorParameters text file");
                DumpSettings.LoadStringsFromFileIntoArr("DumpSettings/PossibleBaseColorParameters.txt", ref DumpSettings.PossibleBaseColorParameterNames);
            }

            Provider = new DefaultFileProvider(
                DumpSettings.GameDirectory,
                SearchOption.TopDirectoryOnly,
                true,
                DumpSettings.CUE4Parse_GameVersion
            );
            Provider.Initialize();
            Provider.Mount();

            if (DumpSettings.bScanProjectForReferencedAssets)
                GetProjectAssets(ref AlreadyExistingAssets);

            DirectoryInfo di = new DirectoryInfo(DumpSettings.AssetsDirectory);
            FileInfo[] UassetFiles = di.GetFiles("*.uasset");

            if (Directory.Exists(DumpSettings.DumpsDirectory))
                Directory.Delete(DumpSettings.DumpsDirectory, true);
            Directory.CreateDirectory(DumpSettings.DumpsDirectory);


            foreach (FileInfo UAssetFile in UassetFiles)
                DumpBlueprint(UAssetFile);


            /** Save the lists of referenced assets */
            SaveReferencedAssetsList(ReferencedBlueprints, $"{DumpSettings.DumpsDirectory}\\ReferencedBlueprints.txt");
            SaveReferencedAssetsList(ReferencedStaticMeshes, $"{DumpSettings.DumpsDirectory}\\ReferencedStaticMeshes.txt");
            SaveReferencedAssetsList(ReferencedMaterials, $"{DumpSettings.DumpsDirectory}\\ReferencedMaterials.txt");

            if (DumpSettings.bExportReferencedMeshes)
                ExportReferencedMeshes();

            if (DumpSettings.bExportReferencedBlueprints)
                ExportReferencedBlueprints();
        }

        static void DumpBlueprint(FileInfo UAssetFile)
        {
            Asset = new UAsset(UAssetFile.FullName, DumpSettings.UAssetAPI_GameVersion);
            UBlueprintGeneratedClass BPGC = new UBlueprintGeneratedClass(Asset.GetClassExport());

            /** Assuming this blueprint is child of AActor */
            if (!BPGC.HasAnyComponents())
                return;

            BlueprintSerializer Serializer = new BlueprintSerializer(
                $"{UAssetFile.DirectoryName}\\Dumps\\{UAssetFile.Name.Replace(UAssetFile.Extension, ".json")}",
                Asset
            );
            Serializer.SerializeBlueprintNameAndParent(DumpSettings.BlueprintsDirectory, AlreadyExistingAssets);

            ReferencedAssets.Clear();

            Serializer.Serialize(BPGC.SimpleConstructionScript);
            Serializer.Serialize(BPGC.InheritableComponentHandler);

            ListReferencedAssets("BlueprintGeneratedClass", ReferencedBlueprints);
            ListReferencedAssets("StaticMesh", ReferencedStaticMeshes);
            ListReferencedAssets("MaterialInstanceConstant", ReferencedMaterials);

            CheckMissingReferences();

            /** If referenced StaticMesh or Blueprint doesn't exist in project yet, we change path to it in advance,
              * so when we will generate this blueprint in Editor, we will get correct references 
              */
            if (DumpSettings.bChangePathToStaticMeshes)
                ChangePathToAssets("StaticMesh", DumpSettings.NewPathToMeshes);
            if (DumpSettings.bChangePathToBlueprints)
                ChangePathToAssets("BlueprintGeneratedClass", DumpSettings.NewPathToBlueprints);

            if (DumpSettings.bScanProjectForReferencedAssets)
                ChangePathToExistingAssets();

            Serializer.SerializeImportTable();
            Serializer.SaveToDisk();
        }

        static void ListReferencedAssets(string AssetType, HashSet<string> ReferencedAssets)
        {
            if (Asset is null)
                throw new Exception();

            foreach (Import Import in Asset.Imports)
            {
                if (Import.ClassName.ToString() == AssetType)
                {
                    // Skip CDO
                    if (Import.ObjectName.ToString().StartsWith("Default__"))
                        continue;

                    // Skip adding reference to parent blueprint class
                    Asset.GetParentClass(out var _not_used, out FName ParentClass);
                    if (Import.ObjectName.ToString() == ParentClass.ToString())
                        continue;

                    Import Package = Import.GetOutermostPackage(Asset);
                    ReferencedAssets.Add(Package.ObjectName.ToString());
                }
            }
        }

        static void SaveReferencedAssetsList(HashSet<string> ReferencedAsset, string FilePath)
        {
            File.WriteAllLines(FilePath, ReferencedAsset.OrderBy(x => x).ToList());
        }
        
        static void CheckMissingReferences()
        {
            if (Asset is null)
                throw new Exception();

            foreach (FPackageIndex Index in ReferencedAssets)
            {
                string ClassName = Index.ToImport(Asset).ClassName.ToString();
                Import PackageImport = Index.ToImport(Asset).GetOutermostPackage(Asset);

                if (PackageImport.ObjectName.ToString().StartsWith("/Game"))
                {
                    string ReferencedAsset = PackageImport.ObjectName.ToString().GetAssetName();

                    if (!AlreadyExistingAssets.Keys.Contains(ReferencedAsset))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"Blueprint {Asset.FilePath.GetAssetName()} has a reference to '{ClassName}'{ReferencedAsset} which doesn't exist yet");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }
        }
       
        static void ChangePathToAssets(string ClassName, string NewPath)
        {
            /** 
             *  for example, change "/Game/Meshes/Environment/Asylum/Blockers/SM_ASY_EdgesWall_Small_02"
             *  to "{NewPath}/SM_ASY_EdgesWall_Small_02" 
             *  
             *  Assets that are already existing in project Content folder are ignored (if corresponding settings are set)
             */

            if (Asset is null)
                throw new Exception();

            foreach (Import import in Asset.Imports)
            {
                if (import.ClassName.ToString() == ClassName)
                {
                    Import Package = import.OuterIndex.ToImport(Asset);
                    string AssetName = Package.ObjectName.ToString().GetAssetName();

                    if (DumpSettings.DontChangePathForPackages.Contains(Package.ObjectName.ToString()))
                        continue;

                    if (import.ObjectName.ToString().StartsWith("Default__"))
                        continue;

                    if (AlreadyExistingAssets.Keys.Contains(AssetName))
                    {
                        Package.ObjectName = new FName(Asset, AlreadyExistingAssets[AssetName]);
                        continue;
                    }

                    Package.ObjectName = new FName(Asset, $"{NewPath}/{AssetName}");
                }
            }
        }
        
        static void ChangePathToExistingAssets()
        {
            if (Asset is null)
                throw new Exception();

            foreach (Import import in Asset.Imports)
                if (import.ClassName.ToString() == "Package" && !import.ObjectName.ToString().StartsWith("/Script"))
                {
                    string AssetName = import.ObjectName.ToString().GetAssetName();

                    if (AlreadyExistingAssets.Keys.Contains(AssetName))
                    {
                        import.ObjectName = new FName(Asset, AlreadyExistingAssets[AssetName]);
                        continue;
                    }
                }
        }

        static void ExportReferencedMeshes()
        {
            if (Provider is null)
                throw new Exception();

            string ExportDirectory = $"{DumpSettings.AssetsDirectory}\\Intermediates";

            if (!Directory.Exists(ExportDirectory))
                Directory.CreateDirectory(ExportDirectory);

            foreach (string MeshPath in ReferencedStaticMeshes)
            {
                string AssetName = MeshPath.GetAssetName();

                if (!MeshPath.StartsWith("/Game"))
                    continue;

                if (DumpSettings.DontChangePathForPackages.Contains(MeshPath))
                    continue;

                if (AlreadyExistingAssets.Keys.Contains(AssetName))
                {
                    Console.WriteLine($"Skipping {AssetName} because it already exists in project");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Exporting mesh {AssetName}...");
                Console.ForegroundColor = ConsoleColor.Gray;

                var AssetPath = MeshPath.Replace("/Game", "DeadByDaylight/Content");
                var Object = Provider.LoadAllObjects(AssetPath);

                foreach (var obj in Object)
                {
                    if (obj is UStaticMesh StaticMesh)
                    {
                        var exporterOptions = new ExporterOptions();
                        exporterOptions.ExportMaterials = false;

                        Exporter exporter = new Exporter(StaticMesh, exporterOptions);
                        DirectoryInfo directory = new DirectoryInfo(ExportDirectory);
                        string label;
                        string savedFilePath;
                        exporter.TryWriteToDir(directory, out label, out savedFilePath);

                        if (DumpSettings.bExportTextures)
                        {
                            string TextureDumpsDirectory = $"{DumpSettings.DumpsDirectory}\\Textures";

                            if (!Directory.Exists(TextureDumpsDirectory))
                                Directory.CreateDirectory(TextureDumpsDirectory);

                            foreach (ResolvedObject? Material in StaticMesh.Materials)
                                if (Material is not null)
                                {
                                    UMaterialInterface? MaterialInterface = Material.Load<UMaterialInterface>();
                                    if (MaterialInterface is not null)
                                        ExportTexturesFromMaterial(MaterialInterface, TextureDumpsDirectory);
                                }
                        }
                    }
                }
            }

            string MeshesExportsDirectory = $"{DumpSettings.DumpsDirectory}\\Meshes";

            string[] PskFiles = Directory.GetFiles(ExportDirectory, "*.pskx", SearchOption.AllDirectories);
            foreach (string PskFile in PskFiles)
            {
                if (!Directory.Exists(MeshesExportsDirectory))
                    Directory.CreateDirectory(MeshesExportsDirectory);

                string newDirectory = MeshesExportsDirectory + PskFile.SubstringAfterWithLast('\\');
                try
                {
                    File.Move(PskFile, newDirectory);
                }
                catch { }
            }

            Directory.Delete(ExportDirectory, true);
        }
        
        static void ExportTexturesFromMaterial(UMaterialInterface StaticMaterial, string exportDirectory)
        {
            if (Asset is null)
                throw new Exception();

            if (AlreadyExistingAssets.ContainsKey(StaticMaterial.Name) || AlreadyListedMaterials.Contains(StaticMaterial.Name))
                return;

            if (StaticMaterial is UMaterialInstanceConstant MI)
            {
                List<string> DiffuseTextures = new List<string>();

                string MaterialsAndTexturesFilePath = $"{DumpSettings.DumpsDirectory}\\MaterialsAndTextures.txt";

                if (!File.Exists(MaterialsAndTexturesFilePath))
                    File.Create(MaterialsAndTexturesFilePath).Close();

                List<string> Materials = File.ReadAllLines(MaterialsAndTexturesFilePath).ToList();
                Materials.Add(MI.Name);

                FTextureParameterValue[] Textures = MI.TextureParameterValues;

                foreach (FTextureParameterValue TextureValue in Textures)
                {
                    if (TextureValue.ParameterValue.TryLoad(out UTexture2D Texture))
                    {
                        foreach (string PossibleBaseColor in DumpSettings.PossibleBaseColorParameterNames)
                            if (TextureValue.Name.ToLower().Replace(" ", "").Contains(PossibleBaseColor.ToLower()))
                            {
                                string AssetName = Texture.Name;

                                if (AlreadyExistingAssets.ContainsKey(AssetName))
                                {
                                    DiffuseTextures.Add(AlreadyExistingAssets[AssetName]);
                                }
                                else
                                {
                                    DiffuseTextures.Add(AssetName);
                                    ExportTexture(Texture, AssetName, exportDirectory);
                                }

                                Materials.Add($"\t\"{TextureValue.Name}\": \"{AssetName}\"");
                            }
                    }
                }

                Materials.Add("");
                File.WriteAllLines(MaterialsAndTexturesFilePath, Materials);

                AlreadyListedMaterials.Add(MI.Name);

                string MaterialsDirectory = $"{DumpSettings.DumpsDirectory}\\Materials";
                if (!Directory.Exists(MaterialsDirectory))
                    Directory.CreateDirectory(MaterialsDirectory);

                File.WriteAllLines($"{MaterialsDirectory}\\{MI.Name}.txt", DiffuseTextures);
            }
        }
        
        static void ExportReferencedBlueprints()
        {
            if (Provider is null)
                throw new Exception();

            foreach (string BlueprintPath in ReferencedBlueprints)
            {
                if (!BlueprintPath.StartsWith("/Game"))
                    continue;

                if (DumpSettings.DontChangePathForPackages.Contains(BlueprintPath))
                    continue;

                if (AlreadyExistingAssets.Keys.Contains(BlueprintPath.GetAssetName()))
                {
                    Console.WriteLine($"Skipping {BlueprintPath.GetAssetName()} because it already exists in project");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Exporting blueprint {BlueprintPath.SubstringAfterLast('/')}...");
                Console.ForegroundColor = ConsoleColor.Gray;

                string BlueprintExportsDirectory = $"{DumpSettings.DumpsDirectory}\\Blueprints";
                if (!Directory.Exists(BlueprintExportsDirectory))
                    Directory.CreateDirectory(BlueprintExportsDirectory);

                var AssetPath = BlueprintPath.Replace("/Game", "DeadByDaylight/Content");

                if (Provider.TrySavePackage(AssetPath, out var Packages))
                    foreach ((string filePath, byte[] data) in Packages)
                    {
                        File.WriteAllBytes($"{BlueprintExportsDirectory}\\{filePath.SubstringAfterLast('/')}", data);
                    }
            }
        }
        
        static void ExportTexture(UTexture2D Texture, string AssetName, string ExportDirectory)
        {
            if (AlreadyExistingAssets.Keys.Contains(AssetName))
            {
                Console.WriteLine($"Skipping {AssetName} because it already exists in project");
                return;
            }

            if (AlreadyExportedTextures.Contains(AssetName))
                return;

            AlreadyExportedTextures.Add(AssetName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Exporting texture {AssetName}...");
            Console.ForegroundColor = ConsoleColor.Gray;

            using FileStream fs = new FileStream($"{ExportDirectory}\\{AssetName}.png", FileMode.Create, FileAccess.Write);

            var Bitmap = Texture.Decode();
            if (Bitmap is not null)
            {
                using var Data = Bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
                using var Stream = Data.AsStream();
                Stream.CopyTo(fs);
            }            
        }

        static void GetProjectAssets(ref Dictionary<string, string> OutAlreadyExistingAssets)
        {
            if (!Directory.Exists(DumpSettings.ProjectDirectory))
                throw new Exception("Project directory doesn't exist. Uncheck bScanProjectForReferencedAssets");

            string[] ProjectAssets = Directory.GetFiles($"{DumpSettings.ProjectDirectory}\\Content", "*.uasset", SearchOption.AllDirectories);

            foreach (string projectAssetPath in ProjectAssets)
            {
                string AssetPath = "/Game" + projectAssetPath.SubstringAfter("Content").SubstringBeforeLast('.').Replace('\\', '/');

                bool bIncludeAsset = true;
                if (DumpSettings.bIgnoreExistingAssetsAtPath)
                {
                    foreach (string IgnorePath in DumpSettings.IgnoreExistingAssetsAtPath)
                        if (AssetPath.StartsWith(IgnorePath))
                        {
                            bIncludeAsset = false;
                            break;
                        }
                }

                if (bIncludeAsset)
                {
                    if (AlreadyExistingAssets.ContainsKey(projectAssetPath.GetAssetName()))
                        throw new Exception($"Two assets with the same name: {OutAlreadyExistingAssets[projectAssetPath.GetAssetName()]} and {AssetPath}");

                    OutAlreadyExistingAssets.Add(projectAssetPath.GetAssetName(), AssetPath);
                }
            }
        }

    }
}
