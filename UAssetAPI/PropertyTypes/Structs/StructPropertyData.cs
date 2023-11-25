﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UAssetAPI.CustomVersions;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace UAssetAPI.PropertyTypes.Structs
{
    public class StructPropertyData : PropertyData<List<PropertyData>> // List
    {
        [JsonProperty]
        public FName StructType = null;
        [JsonProperty]
        public bool SerializeNone = true;
        [JsonProperty]
        public Guid StructGUID = Guid.Empty; // usually set to 0

        public StructPropertyData(FName name) : base(name)
        {
            Value = new List<PropertyData>();
        }

        public StructPropertyData(FName name, FName forcedType) : base(name)
        {
            StructType = forcedType;
            Value = new List<PropertyData>();
        }

        public StructPropertyData()
        {

        }

        private static readonly FString CurrentPropertyType = new FString("StructProperty");
        public override FString PropertyType { get { return CurrentPropertyType; } }

        private void ReadOnce(AssetBinaryReader reader, Type T, long offset)
        {
            var data = Activator.CreateInstance(T, Name) as PropertyData;
            if (data == null) return;
            data.Offset = offset;
            data.Ancestry.Initialize(Ancestry, Name);
            data.Read(reader, false, 0);
            Value = new List<PropertyData> { data };
        }

        private void ReadNTPL(AssetBinaryReader reader)
        {
            List<PropertyData> resultingList = new List<PropertyData>();
            PropertyData data = null;

            var unversionedHeader = new FUnversionedHeader(reader);
            while ((data = MainSerializer.Read(reader, Ancestry, StructType, unversionedHeader, true)) != null)
            {
                resultingList.Add(data);
            }

            Value = resultingList;
        }

        public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0)
        {
            if (includeHeader && !reader.Asset.HasUnversionedProperties) // originally !isForced
            {
                StructType = reader.ReadFName();
                if (reader.Asset.ObjectVersion >= ObjectVersion.VER_UE4_STRUCT_GUID_IN_PROPERTY_TAG) StructGUID = new Guid(reader.ReadBytes(16));
                PropertyGuid = reader.ReadPropertyGuid();
            }

            if (reader.Asset.Mappings != null && (StructType == null || StructType.Value.Value == "Generic") && reader.Asset.Mappings.TryGetPropertyData(Name, Ancestry, reader.Asset, out UsmapStructData strucDat1))
            {
                StructType = FName.DefineDummy(reader.Asset, strucDat1.StructType);
            }

            if (reader.Asset.HasUnversionedProperties && StructType?.Value?.Value == null)
            {
                throw new InvalidOperationException("Unable to determine struct type for struct " + Name.Value.Value + " in class " + Ancestry.Parent.Value.Value);
            }

            RegistryEntry targetEntry = null;
            string structTypeVal = StructType?.Value?.Value;
            if (structTypeVal != null) MainSerializer.PropertyTypeRegistry.TryGetValue(structTypeVal, out targetEntry);
            bool hasCustomStructSerialization = targetEntry != null && targetEntry.HasCustomStructSerialization;

            if (structTypeVal == "FloatRange")
            {
                // FloatRange is a special case; it can either be manually serialized as two floats (TRange<float>) or as a regular struct (FFloatRange), but the first is overridden to use the same name as the second
                // The best solution is to just check and see if the next bit is an FName or not

                int nextFourBytes = reader.ReadInt32();
                reader.BaseStream.Position -= sizeof(int);
                hasCustomStructSerialization = !(reader.Asset.HasUnversionedProperties || (nextFourBytes >= 0 && nextFourBytes < reader.Asset.GetNameMapIndexList().Count && reader.Asset.GetNameReference(nextFourBytes).Value == "LowerBound"));
            }
            if (structTypeVal == "RichCurveKey" && reader.Asset.ObjectVersion < ObjectVersion.VER_UE4_SERIALIZE_RICH_CURVE_KEY) hasCustomStructSerialization = false;
            if (structTypeVal == "MovieSceneTrackIdentifier" && reader.Asset.GetCustomVersion<FEditorObjectVersion>() < FEditorObjectVersion.MovieSceneMetaDataSerialization) hasCustomStructSerialization = false;
            if (structTypeVal == "MovieSceneFloatChannel" && reader.Asset.GetCustomVersion<FSequencerObjectVersion>() < FSequencerObjectVersion.SerializeFloatChannelCompletely && reader.Asset.GetCustomVersion<FFortniteMainBranchObjectVersion>() < FFortniteMainBranchObjectVersion.SerializeFloatChannelShowCurve) hasCustomStructSerialization = false;

            if (leng1 == 0)
            {
                SerializeNone = false;
                Value = new List<PropertyData>();
                return;
            }

            if (targetEntry != null && hasCustomStructSerialization)
            {
                ReadOnce(reader, targetEntry.PropertyType, reader.BaseStream.Position);
            }
            else
            {
                ReadNTPL(reader);
            }
        }

        public override void ResolveAncestries(UnrealPackage asset, AncestryInfo ancestrySoFar)
        {
            var ancestryNew = (AncestryInfo)ancestrySoFar.Clone();
            ancestryNew.SetAsParent(StructType);

            if (Value != null)
            {
                foreach (var entry in Value) entry?.ResolveAncestries(asset, ancestryNew);
            }
            base.ResolveAncestries(asset, ancestrySoFar);
        }

        private int WriteOnce(AssetBinaryWriter writer)
        {
            if (Value.Count > 1) throw new InvalidOperationException("Structs with type " + StructType.Value.Value + " cannot have more than one entry");

            if (Value.Count == 0)
            {
                // populate fallback zero entry 
                if (Value == null) Value = new List<PropertyData>();
                Value.Clear();
                Value.Add(MainSerializer.TypeToClass(StructType, Name, Ancestry, Name, writer.Asset, null, 0, 0, false));
            }
            Value[0].Offset = writer.BaseStream.Position;
            return Value[0].Write(writer, false);
        }

        private int WriteNTPL(AssetBinaryWriter writer)
        {
            int here = (int)writer.BaseStream.Position;
            if (Value != null)
            {
                List<PropertyData> allDat = Value;
                MainSerializer.GenerateUnversionedHeader(ref allDat, StructType, writer.Asset)?.Write(writer);
                foreach (var t in allDat)
                {
                    MainSerializer.Write(t, writer, true);
                }
            }
            if (!writer.Asset.HasUnversionedProperties) writer.Write(new FName(writer.Asset, "None"));
            return (int)writer.BaseStream.Position - here;
        }

        internal bool DetermineIfSerializeWithCustomStructSerialization(UnrealPackage Asset, out RegistryEntry targetEntry)
        {
            targetEntry = null;
            string structTypeVal = StructType?.Value?.Value;
            if (structTypeVal != null) MainSerializer.PropertyTypeRegistry.TryGetValue(structTypeVal, out targetEntry);
            bool hasCustomStructSerialization = targetEntry != null && targetEntry.HasCustomStructSerialization;

            if (structTypeVal == "FloatRange") hasCustomStructSerialization = Value.Count == 1 && Value[0] is FloatRangePropertyData;
            if (structTypeVal == "RichCurveKey" && Asset.ObjectVersion < ObjectVersion.VER_UE4_SERIALIZE_RICH_CURVE_KEY) hasCustomStructSerialization = false;
            if (structTypeVal == "MovieSceneTrackIdentifier" && Asset.GetCustomVersion<FEditorObjectVersion>() < FEditorObjectVersion.MovieSceneMetaDataSerialization) hasCustomStructSerialization = false;
            if (structTypeVal == "MovieSceneFloatChannel" && Asset.GetCustomVersion<FSequencerObjectVersion>() < FSequencerObjectVersion.SerializeFloatChannelCompletely && Asset.GetCustomVersion<FFortniteMainBranchObjectVersion>() < FFortniteMainBranchObjectVersion.SerializeFloatChannelShowCurve) hasCustomStructSerialization = false;
            return hasCustomStructSerialization;
        }

        public override int Write(AssetBinaryWriter writer, bool includeHeader)
        {
            if (includeHeader && !writer.Asset.HasUnversionedProperties)
            {
                writer.Write(StructType);
                if (writer.Asset.ObjectVersion >= ObjectVersion.VER_UE4_STRUCT_GUID_IN_PROPERTY_TAG) writer.Write(StructGUID.ToByteArray());
                writer.WritePropertyGuid(PropertyGuid);
            }

            bool hasCustomStructSerialization = DetermineIfSerializeWithCustomStructSerialization(writer.Asset, out RegistryEntry targetEntry);
            if (targetEntry != null && hasCustomStructSerialization) return WriteOnce(writer);
            if (Value.Count == 0 && !SerializeNone) return 0;
            return WriteNTPL(writer);
        }

        public override bool IsZero(UnrealPackage asset)
        {
            return !DetermineIfSerializeWithCustomStructSerialization(asset, out _) && base.IsZero(asset);
        }

        public override void FromString(string[] d, UAsset asset)
        {
            if (d[4] != null && d[4] != "Generic") StructType = FName.FromString(asset, d[4]);
            if (StructType == null) StructType = FName.DefineDummy(asset, "Generic");
        }

        protected override void HandleCloned(PropertyData res)
        {
            StructPropertyData cloningProperty = (StructPropertyData)res;
            cloningProperty.StructType = (FName)this.StructType.Clone();
            cloningProperty.StructGUID = new Guid(this.StructGUID.ToByteArray());

            List<PropertyData> newData = new List<PropertyData>(this.Value.Count);
            for (int i = 0; i < this.Value.Count; i++)
            {
                newData.Add((PropertyData)this.Value[i].Clone());
            }
            cloningProperty.Value = newData;
        }
    }
}