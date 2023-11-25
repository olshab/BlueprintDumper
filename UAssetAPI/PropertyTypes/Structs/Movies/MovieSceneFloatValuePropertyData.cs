﻿using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.PropertyTypes.Structs
{
    /*
        The code within this file is modified from LongerWarrior's UEAssetToolkitGenerator project, which is licensed under the Apache License 2.0.
        Please see the NOTICE.md file distributed with UAssetAPI and UAssetGUI for more information.
    */

    public class MovieSceneFloatValuePropertyData : PropertyData<FMovieSceneFloatValue>
    {
        public MovieSceneFloatValuePropertyData(FName name) : base(name)
        {

        }

        public MovieSceneFloatValuePropertyData()
        {
            
        }

        private static readonly FString CurrentPropertyType = new FString("MovieSceneFloatValue");
        public override bool HasCustomStructSerialization { get { return true; } }
        public override FString PropertyType { get { return CurrentPropertyType; } }

        public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0)
        {
            if (includeHeader)
            {
                PropertyGuid = reader.ReadPropertyGuid();
            }

            Value = new FMovieSceneFloatValue();
            Value.Read(reader);
        }

        public override int Write(AssetBinaryWriter writer, bool includeHeader)
        {
            if (includeHeader)
            {
                writer.WritePropertyGuid(PropertyGuid);
            }

            int here = (int)writer.BaseStream.Position;

            Value.Write(writer);

            return (int)writer.BaseStream.Position - here;
        }

        public override void FromString(string[] d, UAsset asset)
        {

        }

        public override string ToString()
        {
            return "";
        }
    }
}
