using Cross.Sign.Nethereum.Model;
using Newtonsoft.Json;
using System;

public class PersonalSignConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(PersonalSign);

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var ps = (PersonalSign)value;
        writer.WriteValue(ps.Message); // ✅ 문자열만 출력
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return new PersonalSign(reader.Value?.ToString());
    }
}