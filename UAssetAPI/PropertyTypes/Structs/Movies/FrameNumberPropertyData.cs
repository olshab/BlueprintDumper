﻿using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.PropertyTypes.Structs
{
    /*
        The code within this file is modified from LongerWarrior's UEAssetToolkitGenerator project, which is licensed under the Apache License 2.0.
        Please see the NOTICE.md file distributed with UAssetAPI and UAssetGUI for more information.
    */
    public class FrameNumberPropertyData : PropertyData<FFrameNumber>
    {
        public FrameNumberPropertyData(FName name) : base(name)
        {

        }

        public FrameNumberPropertyData()
        {

        }

        private static readonly FString CurrentPropertyType = new FString("FrameNumber");
        public override bool HasCustomStructSerialization { get { return true; } }
        public override FString PropertyType { get { return CurrentPropertyType; } }

        public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0)
        {
            if (includeHeader)
            {
                PropertyGuid = reader.ReadPropertyGuid();
            }

            Value = new FFrameNumber(reader.ReadInt32());
        }

        public override int Write(AssetBinaryWriter writer, bool includeHeader)
        {
            if (includeHeader)
            {
                writer.WritePropertyGuid(PropertyGuid);
            }

            writer.Write(Value.Value);
            return sizeof(int);
        }

        public override void FromString(string[] d, UAsset asset)
        {
            int.TryParse(d[0], out Value.Value);
        }
    }
}
