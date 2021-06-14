using System;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace connector.framework.cmd
{
  public enum Operation { AddUpdate, Remove, NotSpecified };

  internal static class Converter
  {
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
      MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
      DateParseHandling = DateParseHandling.None,
      Converters =
            {
                ClassInstanceKeyConverter.Singleton,
                OperationConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };
  }

  internal class ClassInstanceKeyConverter : JsonConverter
  {
    public override bool CanConvert(Type t) => t == typeof(PropertyValue) || t == typeof(PropertyValue?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
      switch (reader.TokenType)
      {
        case JsonToken.Null:
          return new PropertyValue { };
        case JsonToken.Integer:
          var integerValue = serializer.Deserialize<long>(reader);
          return new PropertyValue { Integer = integerValue };
        case JsonToken.Float:
          var doubleValue = serializer.Deserialize<double>(reader);
          return new PropertyValue { Double = doubleValue };
        case JsonToken.Boolean:
          var boolValue = serializer.Deserialize<bool>(reader);
          return new PropertyValue { Bool = boolValue };
        case JsonToken.String:
        case JsonToken.Date:
          var stringValue = serializer.Deserialize<string>(reader);
          return new PropertyValue { String = stringValue };
      }
      throw new Exception("Cannot unmarshal type ClassInstanceKey");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
      var value = (PropertyValue)untypedValue;
      if (value.IsNull)
      {
        serializer.Serialize(writer, null);
        return;
      }
      if (value.Integer != null)
      {
        serializer.Serialize(writer, value.Integer.Value);
        return;
      }
      if (value.Double != null)
      {
        serializer.Serialize(writer, value.Double.Value);
        return;
      }
      if (value.Bool != null)
      {
        serializer.Serialize(writer, value.Bool.Value);
        return;
      }
      if (value.String != null)
      {
        serializer.Serialize(writer, value.String);
        return;
      }
      throw new Exception("Cannot marshal type ClassInstanceKey");
    }

    public static readonly ClassInstanceKeyConverter Singleton = new ClassInstanceKeyConverter();
  }

  internal class OperationConverter : JsonConverter
  {
    public override bool CanConvert(Type t) => t == typeof(Operation) || t == typeof(Operation?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
      if (reader.TokenType == JsonToken.Null) 
        return Operation.NotSpecified;
      string value = serializer.Deserialize<string>(reader);
      switch (value)
      {
        case "AddUpdate":
          return Operation.AddUpdate;
        case "Remove":
          return Operation.Remove;
        default:
          return Operation.NotSpecified;
      }
      throw new Exception("Cannot unmarshal type Operation");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
      if (untypedValue == null)
      {
        serializer.Serialize(writer, null);
        return;
      }
      var value = (Operation)untypedValue;
      switch (value)
      {
        case Operation.AddUpdate:
          serializer.Serialize(writer, "AddUpdate");
          return;
        case Operation.Remove:
          serializer.Serialize(writer, "Remove");
          return;
      }
      throw new Exception("Cannot marshal type Operation");
    }

    public static readonly OperationConverter Singleton = new OperationConverter();
  }
}
