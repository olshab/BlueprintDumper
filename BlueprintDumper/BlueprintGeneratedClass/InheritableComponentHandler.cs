using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using BlueprintDumper.Utils;
using BlueprintDumper.Components;

namespace BlueprintDumper.BlueprintGeneratedClass
{
    public class UInheritableComponentHandler
    {
        public List<FComponentOverrideRecord> Records { get; private set; }

        public UInheritableComponentHandler(NormalExport ICH_Export)
        {
            Records = new List<FComponentOverrideRecord>();

            ArrayPropertyData? RecordsData = ICH_Export.FindPropertyByName<ArrayPropertyData>("Records");
            if (RecordsData is not null)
            {
                foreach (StructPropertyData RecordStruct in RecordsData.Value)
                    Records.Add(new FComponentOverrideRecord(RecordStruct));
            }
        }
    }

    public class FComponentOverrideRecord
    {
        public Import? ComponentClass { get; private set; }
        public UActorComponent? ComponentTemplate { get; private set; }
        public FComponentKey ComponentKey { get; private set; }

        public FComponentOverrideRecord(StructPropertyData RecordStruct)
        {
            UAsset Asset = (UAsset)RecordStruct.Ancestry.Parent.Asset;

            /** ComponentClass */
            ObjectPropertyData? ComponentClassObject = RecordStruct.FindPropertyByName<ObjectPropertyData>("ComponentClass");

            if (ComponentClassObject is not null && !ComponentClassObject.IsNull())
                ComponentClass = ComponentClassObject.ToImport(Asset);

            /** ComponentTemplate */
            ObjectPropertyData? ComponentTemplateObject = RecordStruct.FindPropertyByName<ObjectPropertyData>("ComponentTemplate");

            if (ComponentTemplateObject is not null && !ComponentTemplateObject.IsNull())
                ComponentTemplate = USCS_Node.GetActorComponent((NormalExport)ComponentTemplateObject.ToExport(Asset));

            /** ComponentKey */
            StructPropertyData ComponentKeyStruct = RecordStruct.FindPropertyByNameChecked<StructPropertyData>("ComponentKey");
            ComponentKey = new FComponentKey(ComponentKeyStruct);
        }
    }

    public class FComponentKey
    {
        public Import? OwnerClass { get; private set; }
        public string SCSVariableName { get; private set; }
        public Guid AssociatedGuid { get; private set; }

        public FComponentKey(StructPropertyData KeyStruct)
        {
            UAsset Asset = (UAsset)KeyStruct.Ancestry.Parent.Asset;

            /** OwnerClass */
            ObjectPropertyData? OwnerClassObject = KeyStruct.FindPropertyByName<ObjectPropertyData>("OwnerClass");

            if (OwnerClassObject is not null && !OwnerClassObject.IsNull())
                OwnerClass = OwnerClassObject.ToImport(Asset);

            /** SCSVariableName */
            NamePropertyData SCSVariableNameData = KeyStruct.FindPropertyByNameChecked<NamePropertyData>("SCSVariableName");
            SCSVariableName = SCSVariableNameData.Value.ToString();

            /** AssociatedGuid */
            StructPropertyData AssociatedGuidData = KeyStruct.FindPropertyByNameChecked<StructPropertyData>("AssociatedGuid");
            AssociatedGuid = ((GuidPropertyData)AssociatedGuidData.Value[0]).Value;
        }
    }
}
