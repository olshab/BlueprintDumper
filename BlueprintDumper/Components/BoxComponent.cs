using BlueprintDumper.Serialization;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace BlueprintDumper.Components
{
    public class UBoxComponent : UActorComponent
    {
        public UBoxComponent(NormalExport ComponentExport) 
            : base(ComponentExport)
        { }

        public override void Serialize(PropertySerializer Serializer)
        {
            if (DumpSettings.bRemoveCollisonInBoxComponent)
            {
                StructPropertyData? BodyInstance = FindPropertyByName<StructPropertyData>("BodyInstance");

                if (BodyInstance is not null)
                {
                    BodyInstance.Value.Clear();

                    NamePropertyData CollisionProfile = new NamePropertyData(new FName(AssociatedAsset, "CollisionProfileName"));
                    CollisionProfile.Value = new FName(AssociatedAsset, "NoCollision");

                    BytePropertyData CollisionEnabled = new BytePropertyData(new FName(AssociatedAsset, "CollisionEnabled"));
                    CollisionEnabled.ByteType = BytePropertyType.FName;
                    CollisionEnabled.EnumType = new FName(AssociatedAsset, "ECollisionEnabled");
                    CollisionEnabled.EnumValue = new FName(AssociatedAsset, "ECollisionEnabled::NoCollision");

                    BodyInstance.Value.Add(CollisionProfile);
                    BodyInstance.Value.Add(CollisionEnabled);
                }
            }

            base.Serialize(Serializer);
        }
    }
}
