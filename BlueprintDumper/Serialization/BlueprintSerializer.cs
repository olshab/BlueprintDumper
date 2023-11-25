using System.Text;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.UnrealTypes;
using CUE4Parse.Utils;
using BlueprintDumper.Utils;
using BlueprintDumper.BlueprintGeneratedClass;
using BlueprintDumper.Components;

namespace BlueprintDumper.Serialization
{
    public class BlueprintSerializer
    {
        private JsonWriter writer;

        private StringBuilder sb;
        private string JsonFilePath;

        private UAsset AssociatedAsset;

        private PropertySerializer PropertySerializer;

        public BlueprintSerializer(string JsonFilePath, UAsset AssociatedAsset)
        {
            this.AssociatedAsset = AssociatedAsset;
            this.JsonFilePath = JsonFilePath;            

            sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            writer = new JsonTextWriter(sw);

            PropertySerializer = new PropertySerializer(writer, AssociatedAsset);

            writer.Formatting = Formatting.Indented;
            writer.WriteStartObject();
        }

        public void SerializeBlueprintNameAndParent(string BlueprintDirectory, Dictionary<string, string> AlreadyExistingAssets)
        {
            writer.WritePropertyName("PackageName");
            string PackageName = $"{BlueprintDirectory}/{AssociatedAsset.FilePath.SubstringAfterLast('\\').SubstringBeforeLast('.')}";
            writer.WriteValue(PackageName);

            AssociatedAsset.GetParentClass(out FName ParentClassPath, out FName ParentClassExportName);

            if (!ParentClassPath.ToString().StartsWith("/Script"))
            {
                string ParentClassAssetName = ParentClassPath.ToString().GetAssetName();

                if (!AlreadyExistingAssets.ContainsKey(ParentClassAssetName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] For {AssociatedAsset.FilePath.GetAssetName()} parent class {ParentClassAssetName} doesn't exist");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    string ExistingParentBP = AlreadyExistingAssets[ParentClassAssetName];
                    ParentClassPath = new FName(AssociatedAsset, ExistingParentBP);
                }
            }

            writer.WritePropertyName("ParentClassPackage");
            writer.WriteValue(ParentClassPath.ToString());

            writer.WritePropertyName("ParentClass");
            writer.WriteValue(ParentClassExportName.ToString());
        }

        public void Serialize(USimpleConstructionScript? SCS)
        {
            writer.WritePropertyName("Components");
            writer.WriteStartArray();

            if (SCS is not null)
            {
                foreach (USCS_Node RootNode in SCS.RootNodes)
                    Serialize(RootNode, RootNode.ParentComponentOrVariableName);
            }

            writer.WriteEndArray();
        }

        public void Serialize(USCS_Node Node, string? ParentComponentName)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("InternalVariableName");
            writer.WriteValue(Node.InternalVariableName is not null ? Node.InternalVariableName.ToString() : string.Empty);

            writer.WritePropertyName("ParentNodeName");
            writer.WriteValue(ParentComponentName is not null ? ParentComponentName : string.Empty);

            if (Node.ComponentTemplate is null)
            {
                writer.WritePropertyName("ComponentClass");
                FPackageIndex? ComponentClass = Node.ComponentClass?.GetPackageIndex(AssociatedAsset);
                writer.WriteValue(ComponentClass is not null ? ComponentClass.Index : 0);

                writer.WritePropertyName("Properties");
                writer.WriteStartObject();
                writer.WriteEndObject();

                writer.WriteEndObject();
                return;
            }

            if (DumpSettings.bConvertActorSpawnerIntoChildActor && Node.ComponentTemplate is UActorSpawner)
            {
                FPackageIndex? ChildActorComponent = AssociatedAsset.FindImportWithObjectName("ChildActorComponent");
                
                if (ChildActorComponent is null)
                {
                    Import ChildActorCompImport = new Import(
                        "/Script/CoreUObject",
                        "Class",
                        AssociatedAsset.FindImportWithObjectName("/Script/Engine"),
                        "ChildActorComponent",
                        false,
                        AssociatedAsset
                    );
                    ChildActorComponent = AssociatedAsset.AddImport(ChildActorCompImport);

                    Import ChildActorDefault = new Import(
                        "/Script/Engine",
                        "ChildActorComponent",
                        AssociatedAsset.FindImportWithObjectName("/Script/Engine"),
                        "Default__ChildActorComponent",
                        false,
                        AssociatedAsset
                    );
                    AssociatedAsset.AddImport(ChildActorDefault);                    
                }

                Node.ComponentClass = ChildActorComponent.ToImport(AssociatedAsset);
            }

            // TODO: UBoxComponent

            writer.WritePropertyName("ComponentClass");
            writer.WriteValue(Node.ComponentClass is not null ? Node.ComponentClass.GetPackageIndex(AssociatedAsset).Index : 0);

            /// DEBUG PURPOSES ONLY
            if (DumpSettings.bSerializeDebugInfo)
            {
                string ObjectName = "";
                writer.WritePropertyName("DEBUG_PURPOSES_ONLY_ClassName");

                if (Node.ComponentClass is not null)
                    ObjectName = Node.ComponentClass.ObjectName.ToString();

                writer.WriteValue(ObjectName);
            }

            /** Add custom ComponentTag */
            if (DumpSettings.CustomTag != string.Empty)
                Node.ComponentTemplate.AddComponentTag(DumpSettings.CustomTag);

            writer.WritePropertyName("Properties");
            writer.WriteStartObject();

            Node.ComponentTemplate.Serialize(PropertySerializer);

            writer.WriteEndObject();

            writer.WriteEndObject();

            /** Serialize all child nodes */
            foreach (USCS_Node ChildNode in Node.ChildNodes)
                Serialize(ChildNode, Node.InternalVariableName);
        }

        public void Serialize(UInheritableComponentHandler? InheritableComponentHandler)
        {
            writer.WritePropertyName("ComponentOverrideRecords");
            writer.WriteStartArray();

            if (InheritableComponentHandler is not null)
            {
                foreach (FComponentOverrideRecord Record in InheritableComponentHandler.Records)
                    Serialize(Record);
            }

            writer.WriteEndArray();
        }

        public void Serialize(FComponentOverrideRecord Record)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("SCSVariableName");
            writer.WriteValue(Record.ComponentKey.SCSVariableName);

            writer.WritePropertyName("ComponentClass");
            FPackageIndex? ComponentClass = Record.ComponentClass?.GetPackageIndex(AssociatedAsset);
            writer.WriteValue(ComponentClass is not null ? ComponentClass.Index : 0);

            /// DEBUG PURPOSES ONLY
            if (DumpSettings.bSerializeDebugInfo)
            {
                string ObjectName = "";
                writer.WritePropertyName("DEBUG_PURPOSES_ONLY_ClassName");

                if (Record.ComponentClass is not null)
                    ObjectName = Record.ComponentClass.ObjectName.ToString();

                writer.WriteValue(ObjectName);
            }

            writer.WritePropertyName("Properties");
            writer.WriteStartObject();

            Record.ComponentTemplate?.Serialize(PropertySerializer);

            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        public void SerializeImportTable()
        {
            /** Write import table to JSON */

            writer.WritePropertyName("ImportTable");
            writer.WriteStartArray();

            foreach (Import Import in AssociatedAsset.Imports)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("ClassPackage");
                writer.WriteValue(Import.ClassPackage.ToString());

                writer.WritePropertyName("ClassName");
                writer.WriteValue(Import.ClassName.ToString());

                writer.WritePropertyName("OuterIndex");
                writer.WriteValue(Import.OuterIndex.Index);

                writer.WritePropertyName("ObjectName");
                writer.WriteValue(Import.ObjectName.ToString());

                writer.WriteEndObject();
            }

            writer.WriteEnd();
        }

        public void SaveToDisk()
        {
            writer.WriteEndObject();

            File.WriteAllText(JsonFilePath, sb.ToString());
        }
    }
}
