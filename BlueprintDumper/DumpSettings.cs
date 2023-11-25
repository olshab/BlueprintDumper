using CUE4Parse.UE4.Versions;
using UAssetAPI.UnrealTypes;

namespace BlueprintDumper
{
    public static class DumpSettings
    {
        public static bool bSerializeDebugInfo = true;
        public static bool bUsePropertyWhitelist = true;  /// for tiles
        public static bool bChangePathToStaticMeshes = true;  /// for tiles
        public static bool bChangePathToBlueprints = true;  /// for tiles
        public static bool bConvertActorSpawnerIntoChildActor = true;  /// for tiles
        public static bool bExportReferencedMeshes = true;
        public static bool bExportTextures = true;
        public static bool bExportReferencedBlueprints = true;
        public static bool bScanProjectForReferencedAssets = true;
        public static bool bIgnoreExistingAssetsAtPath = true;
        public static bool bRemoveCollisonInBoxComponent = true;
        public static bool bListMaterialsWithTextures = true;
        public static bool bUseSettingsFromFiles = false;

        public static string AssetsDirectory = @"C:\Users\Oleg\Desktop\dumper";
        public static string GameDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Dead by Daylight\DeadByDaylight\Content\Paks";
        public static string ProjectDirectory = @"C:\Users\Oleg\Desktop\OldTiles";

        public static VersionContainer CUE4Parse_GameVersion = new VersionContainer(EGame.GAME_UE4_27);
        public static EngineVersion UAssetAPI_GameVersion = EngineVersion.VER_UE4_27;

        public static string BlueprintsDirectory = "/Game/NewTiles/Blueprints/_buffer";
        public static string NewPathToMeshes = "/Game/NewTiles/Meshes/_buffer";  /// used only if bChangePathToStaticMeshes set to True
        public static string NewPathToBlueprints = "/Game/NewTiles/Blueprints/_buffer";  /// used only if bChangePathToBlueprints set to True
        public static string[] IgnoreExistingAssetsAtPath = {  /// used only if bScanProjectForReferencedAssets and bIgnoreExistingAssetsAtPath set to True
            "/Game/OriginalTiles",
            //"/Game/NewTiles",
            "/Game/MergedTiles",
        };

        public static string CustomTag = "NewTiles";

        public static string[] PossibleBaseColorParameterNames = {
            "BaseColor",
            "Diffuse",
            "BlueChannel_Texture",  // from MI_GroundWall
        };

        // in case we want to use assets which already exist in game
        public static string[] DontChangePathForPackages = {

        };

        public static string DumpsDirectory = $"{AssetsDirectory}\\Dumps";

        public static void ReadSettingsFromFile(string FilePath)
        {
            string[] SettingsStrings = File.ReadAllLines("DumpSettings/Settings.txt");
            int i = 0;
            string Entry;
            string[] KeyValue;

            {
                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bSerializeDebugInfo");
                bSerializeDebugInfo = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bUsePropertyWhitelist");
                bUsePropertyWhitelist = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bChangePathToStaticMeshes");
                bChangePathToStaticMeshes = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bChangePathToBlueprints");
                bChangePathToBlueprints = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bConvertActorSpawnerIntoChildActor");
                bConvertActorSpawnerIntoChildActor = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bExportReferencedMeshes");
                bExportReferencedMeshes = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bExportTextures");
                bExportTextures = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bExportReferencedBlueprints");
                bExportReferencedBlueprints = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bScanProjectForReferencedAssets");
                bScanProjectForReferencedAssets = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bIgnoreExistingAssetsAtPath");
                bIgnoreExistingAssetsAtPath = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bRemoveCollisonInBoxCollision");
                bRemoveCollisonInBoxComponent = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "bListMaterialsWithTextures");
                bListMaterialsWithTextures = GetBoolValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "AssetsDirectory");
                AssetsDirectory = GetStringValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "GameDirectory");
                GameDirectory = GetStringValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "ProjectDirectory");
                ProjectDirectory = GetStringValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "BlueprintsDirectory");
                BlueprintsDirectory = GetStringValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "NewPathToMeshes");
                NewPathToMeshes = GetStringValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "NewPathToBlueprints");
                NewPathToBlueprints = GetStringValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "CustomTag");
                CustomTag = GetStringValue(KeyValue[1]);

                Entry = SettingsStrings[i++];
                KeyValue = Entry.Split('=');
                CheckPair(KeyValue, "UE4_Version");
                int EngineVersion = GetIntValue(KeyValue[1]);
                CUE4Parse_GameVersion = new VersionContainer(Get_CUE4Parse_EGameVersion(EngineVersion));
                UAssetAPI_GameVersion = Get_UAssetAPI_EngineVersion(EngineVersion);
            }
        }

        public static void CheckPair(string[] KeyValue, string ExpectedKey)
        {
            if (KeyValue.Length != 2)
                throw new Exception("Failed to load settings from file");

            if (KeyValue[0].Length <= 0)
                throw new Exception("Failed to read key");

            if (KeyValue[1].Length <= 0)
                throw new Exception("Failed to read value");

            if (KeyValue[0] != ExpectedKey)
                throw new Exception($"Got key {KeyValue[0]}, expected {ExpectedKey}");
        }

        public static bool GetBoolValue(string ValueAsString)
        {
            return Convert.ToBoolean(ValueAsString);
        }

        public static string GetStringValue(string ValueAsString)
        {
            return ValueAsString.Substring(1, ValueAsString.Length - 2);  // chop quotation marks
        }

        public static int GetIntValue(string ValueAsString)
        {
            return Convert.ToInt32(ValueAsString);
        }

        public static EGame Get_CUE4Parse_EGameVersion(int EngineVersion)
        {
            return EngineVersion switch
            {
                8 => EGame.GAME_UE4_8,
                9 => EGame.GAME_UE4_9,
                10 => EGame.GAME_UE4_10,
                11 => EGame.GAME_UE4_11,
                12 => EGame.GAME_UE4_12,
                13 => EGame.GAME_UE4_13,
                14 => EGame.GAME_UE4_14,
                15 => EGame.GAME_UE4_15,
                16 => EGame.GAME_UE4_16,
                17 => EGame.GAME_UE4_17,
                18 => EGame.GAME_UE4_18,
                19 => EGame.GAME_UE4_19,
                20 => EGame.GAME_UE4_20,
                21 => EGame.GAME_UE4_21,
                22 => EGame.GAME_UE4_22,
                23 => EGame.GAME_UE4_23,
                24 => EGame.GAME_UE4_24,
                25 => EGame.GAME_UE4_25,
                26 => EGame.GAME_UE4_26,
                27 => EGame.GAME_UE4_27,
                _ => EGame.GAME_UE4_27,
            };
        }

        public static EngineVersion Get_UAssetAPI_EngineVersion(int engineVersion)
        {
            return engineVersion switch
            {
                8 => EngineVersion.VER_UE4_8,
                9 => EngineVersion.VER_UE4_9,
                10 => EngineVersion.VER_UE4_10,
                11 => EngineVersion.VER_UE4_11,
                12 => EngineVersion.VER_UE4_12,
                13 => EngineVersion.VER_UE4_13,
                14 => EngineVersion.VER_UE4_14,
                15 => EngineVersion.VER_UE4_15,
                16 => EngineVersion.VER_UE4_16,
                17 => EngineVersion.VER_UE4_17,
                18 => EngineVersion.VER_UE4_18,
                19 => EngineVersion.VER_UE4_19,
                20 => EngineVersion.VER_UE4_20,
                21 => EngineVersion.VER_UE4_21,
                22 => EngineVersion.VER_UE4_22,
                23 => EngineVersion.VER_UE4_23,
                24 => EngineVersion.VER_UE4_24,
                25 => EngineVersion.VER_UE4_25,
                26 => EngineVersion.VER_UE4_26,
                27 => EngineVersion.VER_UE4_27,
                _ => EngineVersion.VER_UE4_27,
            };
        }

        public static void LoadStringsFromFileIntoArr(string FilePath, ref string[] StringsArray)
        {
            StringsArray = File.ReadAllLines(FilePath);
        }
    }
}
