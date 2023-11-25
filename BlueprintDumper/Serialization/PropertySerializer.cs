using Newtonsoft.Json;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI;

namespace BlueprintDumper.Serialization
{
    public class PropertySerializer
    {
        private JsonWriter writer;

        private UAsset AssociatedAsset;

        public PropertySerializer(JsonWriter writer, UAsset AssociatedAsset) 
        {
            this.writer = writer;
            this.AssociatedAsset = AssociatedAsset;
        }

        public JsonWriter GetWriter()
        {
            return writer;
        }

        static public string[] ComponentClassesToApplyWhitelist = {
            "ChildActorComponent",
            "StaticMeshComponent",
            "InstancedStaticMeshComponent",
            "HierarchicalInstancedStaticMeshComponent",
        };

        static public string[] PropertyWhitelist = {
            /** UHierarchicalInstancedStaticMeshComponent */
            //"SortedInstances",
            //"NumBuiltInstances",
            //"BuiltInstanceBounds",
            //"CacheMeshExtendedBounds",
            //"InstanceCountToRender",

            /** UInstancedStaticMeshComponent */
            "InstancingRandomSeed",
            "InstanceStartCullDistance",
            "InstanceEndCullDistance",
            "PerInstanceSMData",  // bulk serialized
            "bHasPerInstanceHitProxies",

            /** UStaticMeshComponent */
            "StaticMesh",
            "OverrideMaterials",

            /** UChildActorComponent */
            "ChildActorClass",

            /** UParticleSystemComponent */
            "Template",
            "WarmupTime",
            "SecondsBeforeInactive",

            /** USceneComponent */
            "RelativeLocation",
            "RelativeRotation",
            "RelativeScale3D",
            "Mobility",

            /** UActorComponent */
            "ComponentTags",
            "PrimaryComponentTick",
        };

        private void CustomStructSerialization(PropertyData InProperty)
        {
            if (InProperty is VectorPropertyData VectorProperty)
            {
                writer.WritePropertyName("X");
                writer.WriteValue(VectorProperty.Value.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(VectorProperty.Value.Y);
                writer.WritePropertyName("Z");
                writer.WriteValue(VectorProperty.Value.Z);

                return;
            }

            else if (InProperty is Vector4PropertyData Vector4Property)
            {
                writer.WritePropertyName("X");
                writer.WriteValue(Vector4Property.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(Vector4Property.Y);
                writer.WritePropertyName("Z");
                writer.WriteValue(Vector4Property.Z);
                writer.WritePropertyName("W");
                writer.WriteValue(Vector4Property.W);

                return;
            }

            else if (InProperty is Vector2DPropertyData Vector2DProperty)
            {
                writer.WritePropertyName("X");
                writer.WriteValue(Vector2DProperty.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(Vector2DProperty.Y);

                return;
            }

            else if (InProperty is PlanePropertyData PlaneProperty)
            {
                writer.WritePropertyName("X");
                writer.WriteValue(PlaneProperty.Value.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(PlaneProperty.Value.Y);
                writer.WritePropertyName("Z");
                writer.WriteValue(PlaneProperty.Value.Z);
                writer.WritePropertyName("W");
                writer.WriteValue(PlaneProperty.Value.W);

                return;
            }

            else if (InProperty is RotatorPropertyData RotatorProperty)
            {
                writer.WritePropertyName("Pitch");
                writer.WriteValue(RotatorProperty.Value.Pitch);
                writer.WritePropertyName("Yaw");
                writer.WriteValue(RotatorProperty.Value.Yaw);
                writer.WritePropertyName("Roll");
                writer.WriteValue(RotatorProperty.Value.Roll);

                return;
            }

            else if (InProperty is QuatPropertyData QuatProperty)
            {
                writer.WritePropertyName("X");
                writer.WriteValue(QuatProperty.Value.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(QuatProperty.Value.Y);
                writer.WritePropertyName("Z");
                writer.WriteValue(QuatProperty.Value.Z);
                writer.WritePropertyName("W");
                writer.WriteValue(QuatProperty.Value.W);

                return;
            }

            else if (InProperty is IntPointPropertyData IntPointProperty)
            {
                writer.WritePropertyName("X");
                writer.WriteValue(IntPointProperty.Value[0]);
                writer.WritePropertyName("Y");
                writer.WriteValue(IntPointProperty.Value[1]);

                return;
            }

            else if (InProperty is ColorPropertyData ColorProperty)
            {
                writer.WritePropertyName("A");
                writer.WriteValue(ColorProperty.Value.A);
                writer.WritePropertyName("B");
                writer.WriteValue(ColorProperty.Value.B);
                writer.WritePropertyName("G");
                writer.WriteValue(ColorProperty.Value.G);
                writer.WritePropertyName("R");
                writer.WriteValue(ColorProperty.Value.R);

                return;
            }

            else if (InProperty is LinearColorPropertyData LinearColorProperty)
            {
                writer.WritePropertyName("A");
                writer.WriteValue(LinearColorProperty.Value.A);
                writer.WritePropertyName("B");
                writer.WriteValue(LinearColorProperty.Value.B);
                writer.WritePropertyName("G");
                writer.WriteValue(LinearColorProperty.Value.G);
                writer.WritePropertyName("R");
                writer.WriteValue(LinearColorProperty.Value.R);

                return;
            }

            else if (InProperty is BoxPropertyData BoxProperty)
            {
                writer.WritePropertyName("Min");
                writer.WriteStartObject();
                writer.WritePropertyName("X");
                writer.WriteValue(BoxProperty.Value[0].Value.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(BoxProperty.Value[0].Value.Y);
                writer.WritePropertyName("Z");
                writer.WriteValue(BoxProperty.Value[0].Value.Z);
                writer.WriteEndObject();

                writer.WritePropertyName("Max");
                writer.WriteStartObject();
                writer.WritePropertyName("X");
                writer.WriteValue(BoxProperty.Value[1].Value.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(BoxProperty.Value[1].Value.Y);
                writer.WritePropertyName("Z");
                writer.WriteValue(BoxProperty.Value[1].Value.Z);
                writer.WriteEndObject();

                writer.WritePropertyName("IsValid");
                writer.WriteValue(BoxProperty.IsValid);

                return;
            }

            else if (InProperty is Box2DPropertyData Box2DProperty)
            {
                writer.WritePropertyName("Min");
                writer.WriteStartObject();
                writer.WritePropertyName("X");
                writer.WriteValue(Box2DProperty.Value[0].X);
                writer.WritePropertyName("Y");
                writer.WriteValue(Box2DProperty.Value[0].Y);

                writer.WritePropertyName("Max");
                writer.WriteStartObject();
                writer.WritePropertyName("X");
                writer.WriteValue(Box2DProperty.Value[1].X);
                writer.WritePropertyName("Y");
                writer.WriteValue(Box2DProperty.Value[1].Y);

                writer.WritePropertyName("IsValid");
                writer.WriteValue(Box2DProperty.IsValid);

                return;
            }

            Console.WriteLine($"Can't resolve FProperty {InProperty.Name.ToString()} (type is {InProperty.PropertyType})");
        }

        public void SerializePropertyValue(PropertyData InProperty, bool IsArrayElement = false)
        {
            if (!IsArrayElement)
                writer.WritePropertyName(InProperty.Name.ToString());

            if (InProperty is ArrayPropertyData ArrayProperty)
            {
                writer.WriteStartArray();
                foreach (PropertyData data in ArrayProperty.Value)
                    SerializePropertyValue(data, true);
                writer.WriteEnd();

                return;
            }

            else if (InProperty is BoolPropertyData BoolProperty)
            {
                writer.WriteValue(BoolProperty.Value);
                return;
            }

            else if (InProperty is DelegatePropertyData DeletageProperty)
            {
                throw new NotImplementedException("Serialization for FDelegateProperty is not implemented");
            }

            else if (InProperty is EnumPropertyData EnumProperty)
            {
                writer.WriteValue(EnumProperty.Value.ToString());
                return;
            }

            else if (InProperty is InterfacePropertyData InterfaceProperty)
            {
                throw new NotImplementedException("Serialization for FInterfaceProperty is not implemented");
            }

            else if (InProperty is MapPropertyData MapProperty)
            {
                throw new NotImplementedException("Serialization for FMapProperty is not implemented");
            }

            else if (InProperty is MulticastInlineDelegatePropertyData MulticastInlineDelegateProperty)
            {
                throw new NotImplementedException("Serialization for FMulticastInlineDelegateProperty is not implemented");
            }

            else if (InProperty is MulticastDelegatePropertyData MulticastDelegateProperty)
            {
                throw new NotImplementedException("Serialization for FMulticastDelegateProperty is not implemented");
            }

            else if (InProperty is MulticastSparseDelegatePropertyData MulticastSparseDelegateProperty)
            {
                throw new NotImplementedException("Serialization for FMulticastSparseDelegateProperty is not implemented");
            }

            else if (InProperty is NamePropertyData NameProperty)
            {
                writer.WriteValue(NameProperty.Value.ToString());
                return;
            }

            else if (InProperty is BytePropertyData ByteProperty)
            {
                if (ByteProperty.ByteType == BytePropertyType.Byte)
                    writer.WriteValue(ByteProperty.Value);

                else if (ByteProperty.ByteType == BytePropertyType.FName)
                    writer.WriteValue(ByteProperty.EnumValue.ToString());

                return;
            }

            else if (InProperty is DoublePropertyData DoubleProperty)
            {
                writer.WriteValue(DoubleProperty.Value);
                return;
            }

            else if (InProperty is FloatPropertyData FloatProperty)
            {
                writer.WriteValue(FloatProperty.Value);
                return;
            }

            else if (InProperty is Int16PropertyData Int16Property)
            {
                writer.WriteValue(Int16Property.Value);
                return;
            }

            else if (InProperty is Int64PropertyData Int64Property)
            {
                writer.WriteValue(Int64Property.Value);
                return;
            }

            else if (InProperty is Int8PropertyData Int8Property)
            {
                writer.WriteValue(Int8Property.Value);
                return;
            }

            else if (InProperty is IntPropertyData IntProperty)
            {
                writer.WriteValue(IntProperty.Value);
                return;
            }

            else if (InProperty is UInt16PropertyData UInt16Property)
            {
                writer.WriteValue(UInt16Property.Value);
                return;
            }

            else if (InProperty is UInt32PropertyData UInt32Property)
            {
                writer.WriteValue(UInt32Property.Value);
                return;
            }

            else if (InProperty is UInt64PropertyData UInt64Property)
            {
                writer.WriteValue(UInt64Property.Value);
                return;
            }

            else if (InProperty is ObjectPropertyData ObjectProperty)
            {
                writer.WriteValue(ObjectProperty.Value.Index);
                if (ObjectProperty.Value.IsImport())
                    Program.ReferencedAssets.Add(ObjectProperty.Value);

                // DEBUG PURPOSES ONLY
                if (DumpSettings.bSerializeDebugInfo && !IsArrayElement)
                {
                    string ObjectName = "";
                    writer.WritePropertyName("DEBUG_PURPOSES_ONLY_ObjectName");

                    if (ObjectProperty.Value.IsImport())
                        ObjectName = ObjectProperty.Value.ToImport(Program.Asset).ObjectName.ToString();
                    else if (ObjectProperty.Value.IsExport())
                        ObjectName = ObjectProperty.Value.ToExport(Program.Asset).ObjectName.ToString();

                    writer.WriteValue(ObjectName);
                }

                return;
            }

            else if (InProperty is SoftObjectPropertyData SoftObjectProperty)
            {
                writer.WriteValue(SoftObjectProperty.Value.AssetPath.AssetName.ToString());
                return;
            }

            else if (InProperty is SoftClassPathPropertyData SoftClassPathProperty)
            {
                throw new NotImplementedException("Serialization for FSoftClassPathProperty is not implemented");
            }

            else if (InProperty is SetPropertyData SetProperty)
            {
                throw new NotImplementedException("Serialization for FSetProperty is not implemented");
            }

            else if (InProperty is StrPropertyData StrProperty)
            {
                if (StrProperty.Value == null)
                {
                    writer.WriteValue("");
                    return;
                }

                writer.WriteValue(StrProperty.Value.ToString());
                return;
            }

            else if (InProperty is StructPropertyData StructProperty)
            {
                writer.WriteStartObject();

                foreach (PropertyData data in StructProperty.Value)
                {
                    if (data.HasCustomStructSerialization)
                        CustomStructSerialization(data);
                    else
                        SerializePropertyValue(data);
                }

                writer.WriteEndObject();

                return;
            }

            else if (InProperty is TextPropertyData TextProperty)
            {
                writer.WriteValue(TextProperty.Value.ToString());
                return;
            }

            Console.WriteLine($"Can't resolve FProperty {InProperty.Name.ToString()} (type is {InProperty.PropertyType})");
            writer.WriteNull();
        }

        public static void InstancedStaticMesh_BulkSerialize(NormalExport InstancedStaticMeshComponent, JsonWriter writer)
        {
            writer.WritePropertyName("PerInstanceSMData");
            writer.WriteStartArray();

            byte[] Extra = InstancedStaticMeshComponent.Extras;
            MemoryStream stream = new MemoryStream(Extra);
            UnrealBinaryReader reader = new UnrealBinaryReader(stream);

            reader.BaseStream.Seek(16, SeekOrigin.Begin);
            int NumberOfInstances = reader.ReadInt32();

            for (int i = 0; i < NumberOfInstances; i++)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Transform");
                writer.WriteStartObject();

                String[] Planes = {
                    "XPlane",
                    "YPlane",
                    "ZPlane",
                    "WPlane"
                };

                String[] Coefficients = {
                    "W",
                    "X",
                    "Y",
                    "Z"
                };

                foreach (String Plane in Planes)
                {
                    writer.WritePropertyName(Plane);
                    writer.WriteStartObject();

                    float X = reader.ReadSingle();
                    float Y = reader.ReadSingle();
                    float Z = reader.ReadSingle();
                    float W = reader.ReadSingle();

                    writer.WritePropertyName("W");
                    writer.WriteValue(W);
                    writer.WritePropertyName("X");
                    writer.WriteValue(X);
                    writer.WritePropertyName("Y");
                    writer.WriteValue(Y);
                    writer.WritePropertyName("Z");
                    writer.WriteValue(Z);

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteEnd();
        }
    }
}
