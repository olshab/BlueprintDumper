﻿using System.Text;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetAPI.UnrealTypes
{
    /*
        The code within this file is modified from LongerWarrior's UEAssetToolkitGenerator project, which is licensed under the Apache License 2.0.
        Please see the NOTICE.md file distributed with UAssetAPI and UAssetGUI for more information.
    */

    public class FUniqueNetId
    {
        public FName Type;
        public FString Contents;

        public FUniqueNetId(FName type, FString contents)
        {
            Type = type;
            Contents = contents;
        }
    }

    public class UniqueNetIdReplPropertyData : PropertyData<FUniqueNetId>
    {
        public UniqueNetIdReplPropertyData(FName name) : base(name)
        {

        }

        public UniqueNetIdReplPropertyData()
        {

        }

        private static readonly FString CurrentPropertyType = new FString("UniqueNetIdRepl");
        public override bool HasCustomStructSerialization { get { return true; } }
        public override FString PropertyType { get { return CurrentPropertyType; } }

        public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0)
        {
            if (includeHeader)
            {
                PropertyGuid = reader.ReadPropertyGuid();
            }

            int size = reader.ReadInt32();
            if (size > 0)
            {
                Value = new FUniqueNetId(reader.ReadFName(), reader.ReadFString());
            }
            else
            {
                Value = null;
            }
        }

        public override int Write(AssetBinaryWriter writer, bool includeHeader)
        {
            if (includeHeader)
            {
                writer.WritePropertyGuid(PropertyGuid);
            }

            int here = (int)writer.BaseStream.Position;

            if (Value != null)
            {
                int length = 3 * sizeof(int);
                if (Value.Contents != null)
                {
                    length+= Value.Contents.Encoding is UnicodeEncoding ? (Value.Contents.Value.Length+1)*2 : (Value.Contents.Value.Length+1);
                }
                writer.Write(Value.Type);
                writer.Write(Value.Contents);
            }
            else
            {
                writer.Write(0);
            }           
            return (int)writer.BaseStream.Position - here;
        }
    }
}
