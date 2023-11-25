using Newtonsoft.Json;
using UAssetAPI.ExportTypes;
using UAssetAPI;
using BlueprintDumper.Serialization;

namespace BlueprintDumper.Components
{
    public class UInstancedStaticMeshComponent : UActorComponent
    {
        public List<FInstancedStaticMeshInstancedData> PerInstanceSMData;

        public UInstancedStaticMeshComponent(NormalExport ComponentExport)
            : base(ComponentExport)
        {
            PerInstanceSMData = new List<FInstancedStaticMeshInstancedData>();

            byte[] Extras = ComponentExport.Extras;
            MemoryStream stream = new MemoryStream(Extras);
            UnrealBinaryReader reader = new UnrealBinaryReader(stream);

            reader.BaseStream.Seek(16, SeekOrigin.Begin);
            int ArrayNum = reader.ReadInt32();

            for (int i = 0; i < ArrayNum; i++)
                PerInstanceSMData.Add(FInstancedStaticMeshInstancedData.Deserialize(reader));
        }

        public override void Serialize(PropertySerializer Serializer)
        {
            base.Serialize(Serializer);

            JsonWriter writer = Serializer.GetWriter();

            writer.WritePropertyName("PerInstanceSMData");
            writer.WriteStartArray();

            foreach (FInstancedStaticMeshInstancedData InstanceSMData in PerInstanceSMData)
                InstanceSMData.Serialize(writer);

            writer.WriteEndArray();

            /** Enable instances selection for ISM and HISM */
            writer.WritePropertyName("bHasPerInstanceHitProxies");
            writer.WriteValue(true);
        }
    }

    public class FInstancedStaticMeshInstancedData
    {
        public FMatrix Transform;

        public FInstancedStaticMeshInstancedData(FMatrix Transform)
        {
            this.Transform = Transform;
        }

        public static FInstancedStaticMeshInstancedData Deserialize(UnrealBinaryReader reader)
        {
            FMatrix Transform = FMatrix.Deserialize(reader);

            return new FInstancedStaticMeshInstancedData(Transform);
        }

        public void Serialize(JsonWriter writer) 
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Transform");
            Transform.Serialize(writer);

            writer.WriteEndObject();
        }
    }

    public class FMatrix
    {
        public FPlane XPlane;
        public FPlane YPlane;
        public FPlane ZPlane;
        public FPlane WPlane;

        public FMatrix(FPlane XPlane, FPlane YPlane, FPlane ZPlane, FPlane WPlane)
        {
            this.XPlane = XPlane;
            this.YPlane = YPlane;
            this.ZPlane = ZPlane;
            this.WPlane = WPlane;
        }

        public static FMatrix Deserialize(UnrealBinaryReader reader) 
        {
            FPlane XPlane = FPlane.Deserialize(reader);
            FPlane YPlane = FPlane.Deserialize(reader);
            FPlane ZPlane = FPlane.Deserialize(reader);
            FPlane WPlane = FPlane.Deserialize(reader);

            return new FMatrix(XPlane, YPlane, ZPlane, WPlane);
        }

        public void Serialize(JsonWriter writer) 
        {
            writer.WriteStartObject();

            writer.WritePropertyName("XPlane");
            XPlane.Serialize(writer);
            writer.WritePropertyName("YPlane");
            YPlane.Serialize(writer);
            writer.WritePropertyName("ZPlane");
            ZPlane.Serialize(writer);
            writer.WritePropertyName("WPlane");
            WPlane.Serialize(writer);

            writer.WriteEndObject();
        }
    }

    public class FPlane
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public FPlane(float X, float Y, float Z, float W)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.W = W;
        }

        public static FPlane Deserialize(UnrealBinaryReader reader)
        {
            float X = reader.ReadSingle();
            float Y = reader.ReadSingle();
            float Z = reader.ReadSingle();
            float W = reader.ReadSingle();

            return new FPlane(X, Y, Z, W);
        }

        public void Serialize(JsonWriter writer) 
        {
            writer.WriteStartObject();

            writer.WritePropertyName("X");
            writer.WriteValue(X);
            writer.WritePropertyName("Y");
            writer.WriteValue(Y);
            writer.WritePropertyName("Z");
            writer.WriteValue(Z);
            writer.WritePropertyName("W");
            writer.WriteValue(W);

            writer.WriteEndObject();
        }
    }
}
