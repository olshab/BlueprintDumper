using BlueprintDumper.Serialization;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;

namespace BlueprintDumper.Components
{
    public class UActorComponent
    {
        public List<PropertyData> Properties { get; private set; }
        public string ClassName { get; private set; }

        protected NormalExport ComponentExport;

        protected UAsset AssociatedAsset;

        public UActorComponent(NormalExport ComponentExport)
        {
            UAsset Asset = (UAsset)ComponentExport.Asset;
            AssociatedAsset = Asset;

            /** ClassName */
            Import ClassImport = ComponentExport.ClassIndex.ToImport(AssociatedAsset);
            ClassName = ClassImport.ObjectName.ToString();

            /** Properties */
            Properties = ComponentExport.Data;

            this.ComponentExport = ComponentExport;
        }

        public virtual void Serialize(PropertySerializer Serializer)
        {
            foreach (PropertyData Property in Properties)
            {
                // TODO: property whitelist

                Serializer.SerializePropertyValue(Property);
            }
        }

        public void AddComponentTag(string TagName)
        {
            ArrayPropertyData? ComponentTags = FindPropertyByName<ArrayPropertyData>("ComponentTags");
            if (ComponentTags is null)
            {
                ComponentTags = new ArrayPropertyData(new FName(AssociatedAsset, "ComponentTags"));
                Properties.Add(ComponentTags);
            }

            List<PropertyData> TagsList = ComponentTags.Value.ToList();

            NamePropertyData ComponentTag = new NamePropertyData();
            ComponentTag.Value = new FName(AssociatedAsset, TagName);
            TagsList.Add(ComponentTag);

            ComponentTags.Value = TagsList.ToArray();
        }

        public void ClearComponentTags()
        {
            ArrayPropertyData? ComponentTags = FindPropertyByName<ArrayPropertyData>("ComponentTags");
            if (ComponentTags is null)
            {
                return;
            }

            ComponentTags.Value = new PropertyData[0];
        }

        public bool HasAnyTags(string[] Tags)
        {
            ArrayPropertyData? ComponentTags = FindPropertyByName<ArrayPropertyData>("ComponentTags");
            if (ComponentTags is null)
                return false;

            foreach (PropertyData data in ComponentTags.Value)
            {
                NamePropertyData ComponentTag = (NamePropertyData)data;

                if (Tags.Contains(ComponentTag.Value.ToString()))
                    return true;
            }

            return false;
        }

        public T? FindPropertyByName<T>(string PropertyName) where T : PropertyData
        {
            foreach (PropertyData Property in Properties)
                if (Property.Name.ToString() == PropertyName)
                    return (T)Property;

            return null;
        }

        public T FindPropertyByNameChecked<T>(string PropertyName) where T : PropertyData
        {
            foreach (PropertyData Property in Properties)
                if (Property.Name.ToString() == PropertyName)
                    return (T)Property;

            throw new Exception($"Failed to find property {PropertyName} in component {ComponentExport.ObjectName.ToString()}");
        }

        public NormalExport GetComponentExport()
        {
            return ComponentExport;
        }

        public UAsset GetAssociatedAsset()
        {
            return AssociatedAsset;
        }
    }
}
