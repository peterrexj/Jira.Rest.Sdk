using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;

namespace Jira.Rest.Sdk.Extensions;

public class DescriptionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(object);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);

        if (token.Type == JTokenType.Null)
        {
            return null;
        }

        if (token.Type == JTokenType.Object && token["type"] != null && token["version"] != null && token["content"] != null)
        {
            return token.ToObject<Dtos.Description>();
        }
        else if (token.Type == JTokenType.String)
        {
            return token.ToString();
        }
        else
        {
            return token.ToObject<object>();
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is Dtos.Description description)
        {
            serializer.Serialize(writer, description);
        }
        else if (value is string descriptionUpdate)
        {
            writer.WriteValue(descriptionUpdate);
        }
        else
        {
            serializer.Serialize(writer, value);
        }
    }
}
