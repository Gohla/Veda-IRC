using System;
using Newtonsoft.Json;

namespace Veda.Storage
{
    public class NullableRefConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            NullableRef nullableRef = (NullableRef)value;
            Formatting oldFormatting = writer.Formatting;
            writer.Formatting = Formatting.None;
            writer.WriteStartArray();
            if(nullableRef.HasValue)
                serializer.Serialize(writer, nullableRef.ValueObject);
            writer.WriteEndArray();
            writer.Formatting = oldFormatting;
        }

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            Type innerType = type.GenericTypeArguments[0];

            if(reader.TokenType != JsonToken.StartArray)
                return type.GetConstructor(Type.EmptyTypes).Invoke(null);

            reader.Read();
            if(reader.TokenType == JsonToken.EndArray)
                return type.GetConstructor(Type.EmptyTypes).Invoke(null);

            object obj = type.GetConstructor(new[] { innerType }).Invoke(new[] { 
                serializer.Deserialize(reader, innerType) });
            reader.Read();

            return obj;
        }

        public override bool CanConvert(Type type)
        {
            return typeof(NullableRef).IsAssignableFrom(type);
        }
    }
}
