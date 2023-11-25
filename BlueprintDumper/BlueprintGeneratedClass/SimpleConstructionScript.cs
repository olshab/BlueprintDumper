using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using BlueprintDumper.Utils;
using BlueprintDumper.Components;

namespace BlueprintDumper.BlueprintGeneratedClass
{
    public class USimpleConstructionScript
    {
        public List<USCS_Node> RootNodes { get; private set; }
        public List<USCS_Node> AllNodes { get; private set; }
        public USCS_Node DefaultSceneRootNode { get; private set; }

        public USimpleConstructionScript(NormalExport SCS_Export)
        {
            /** RootNodes */
            RootNodes = new List<USCS_Node>();

            ArrayPropertyData? RootNodesData = SCS_Export.FindPropertyByName<ArrayPropertyData>("RootNodes");
            if (RootNodesData is not null)
            {
                foreach (ObjectPropertyData RootNodeObject in RootNodesData.Value)
                    RootNodes.Add(new USCS_Node((NormalExport)RootNodeObject.Value.ToExport(SCS_Export.Asset)));
            }

            /** AllNodes */
            AllNodes = new List<USCS_Node>();

            ArrayPropertyData? AllNodesData = SCS_Export.FindPropertyByName<ArrayPropertyData>("AllNodes");
            if (AllNodesData is not null)
            {
                foreach (ObjectPropertyData AllNodeObject in AllNodesData.Value)
                    AllNodes.Add(new USCS_Node((NormalExport)AllNodeObject.Value.ToExport(SCS_Export.Asset)));
            }

            /** DefaultSceneRootNode */
            ObjectPropertyData DefaultSceneRootObject = SCS_Export.FindPropertyByNameChecked<ObjectPropertyData>("DefaultSceneRootNode");
            DefaultSceneRootNode = new USCS_Node((NormalExport)DefaultSceneRootObject.ToExport(SCS_Export.Asset));
        }
    }

    public class USCS_Node
    {
        public Import? ComponentClass { get; set; }
        public UActorComponent? ComponentTemplate { get; private set; }
        public string? ParentComponentOrVariableName { get; private set; }
        public string? ParentComponentOwnerClassName { get; private set; }
        public List<USCS_Node> ChildNodes { get; private set; }
        public Guid VariableGuid { get; private set; }
        public string InternalVariableName { get; private set; }

        public USCS_Node(NormalExport SCS_Node_Export)
        {
            /** ComponentClass */
            ObjectPropertyData? ComponentClassObject = SCS_Node_Export.FindPropertyByName<ObjectPropertyData>("ComponentClass");

            if (ComponentClassObject is not null && !ComponentClassObject.IsNull())
                ComponentClass = ComponentClassObject.ToImport(SCS_Node_Export.Asset);

            /** ComponentTemplate */
            ObjectPropertyData? ComponentTemplateObject = SCS_Node_Export.FindPropertyByName<ObjectPropertyData>("ComponentTemplate");

            if (ComponentTemplateObject is not null && !ComponentTemplateObject.IsNull())
                ComponentTemplate = GetActorComponent((NormalExport)ComponentTemplateObject.ToExport(SCS_Node_Export.Asset));

            /** ParentComponentOrVariableName */
            NamePropertyData? ParentComponentOrVariableNameData = SCS_Node_Export.FindPropertyByName<NamePropertyData>("ParentComponentOrVariableName");

            if (ParentComponentOrVariableNameData is not null)
                ParentComponentOrVariableName = ParentComponentOrVariableNameData.Value.ToString();

            /** ParentComponentOwnerClassName */
            NamePropertyData? ParentComponentOwnerClassNameData = SCS_Node_Export.FindPropertyByName<NamePropertyData>("ParentComponentOwnerClassName");

            if (ParentComponentOwnerClassNameData is not null)
                ParentComponentOwnerClassName = ParentComponentOwnerClassNameData.Value.ToString();

            /** ChildNodes */
            ChildNodes = new List<USCS_Node>();

            ArrayPropertyData? ChildNodesData = SCS_Node_Export.FindPropertyByName<ArrayPropertyData>("ChildNodes");
            if (ChildNodesData is not null)
            {
                foreach (ObjectPropertyData ChildNodeObject in ChildNodesData.Value)
                    ChildNodes.Add(new USCS_Node((NormalExport)ChildNodeObject.Value.ToExport(SCS_Node_Export.Asset)));
            }

            /** VariableGuid */
            StructPropertyData VariableGuidData = SCS_Node_Export.FindPropertyByNameChecked<StructPropertyData>("VariableGuid");
            VariableGuid = ((GuidPropertyData)VariableGuidData.Value[0]).Value;

            /** InternalVariableName */
            NamePropertyData InternalVariableNameData = SCS_Node_Export.FindPropertyByNameChecked<NamePropertyData>("InternalVariableName");
            InternalVariableName = InternalVariableNameData.Value.ToString();
        }

        public static UActorComponent GetActorComponent(NormalExport ComponentExport)
        {
            Import ClassImport = ComponentExport.ClassIndex.ToImport(ComponentExport.Asset);
            string ClassName = ClassImport.ObjectName.ToString();

            return ClassName switch
            {
                "ChildActorComponent" => new UChildActorComponent(ComponentExport),
                "ActorSpawner" => new UActorSpawner(ComponentExport),
                "HexSpawner" => new UHexSpawner(ComponentExport),
                "InstancedStaticMeshComponent" => new UInstancedStaticMeshComponent(ComponentExport),
                "HierarchicalInstancedStaticMeshComponent" => new UInstancedStaticMeshComponent(ComponentExport),
                "FoliageInstancedStaticMeshComponent" => new UInstancedStaticMeshComponent(ComponentExport),
                "BoxComponent" => new UBoxComponent(ComponentExport), 

                _ => new UActorComponent(ComponentExport)
            };
        }
    }
}
