using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  public class ImportFile
  {
    [JsonProperty("ClassAndInstaces", NullValueHandling = NullValueHandling.Ignore)]
    public ClassAndInstancesReference ClassAndInstaces { get; set; }

    [JsonProperty("Connector", NullValueHandling = NullValueHandling.Ignore)]
    public Connector Connector { get; set; }
  }

  public class InstanceCollection
  {
    [JsonProperty("Children", NullValueHandling = NullValueHandling.Ignore)]
    public List<ClassAndInstancesReference> Children { get; set; }

    [JsonProperty("Host", NullValueHandling = NullValueHandling.Ignore)]
    public InstanceReference Host { get; set; }

    [JsonProperty("ManagingActionPoint", NullValueHandling = NullValueHandling.Ignore)]
    public ManagingActionPoint ManagingActionPoint { get; set; }

    [JsonProperty("Operation", Required = Required.Always)]
    public Operation Operation { get; set; }

    [JsonProperty("Properties", Required = Required.Always)]
    public Dictionary<string, PropertyValue> Properties { get; set; }
  }

  public class ClassAndInstancesReference
  {
    [JsonProperty("Class", NullValueHandling = NullValueHandling.Ignore)]
    public ClassReference Class { get; set; }

    [JsonProperty("InstanceCollection", NullValueHandling = NullValueHandling.Ignore)]
    public List<InstanceCollection> InstanceCollection { get; set; }
  }

  public class InstanceReference
  {
    [JsonProperty("Class", NullValueHandling = NullValueHandling.Ignore)]
    public ClassReference Class { get; set; }

    [JsonProperty("ClassInstanceKey", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, PropertyValue> ClassInstanceKey { get; set; }

    [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }
  }

  /// <summary>
  /// Class is defined by either an Id (Guid) OR an MP name plus a class name.
  /// </summary>
  public class ClassReference
  {
    [JsonProperty("ClassId", NullValueHandling = NullValueHandling.Ignore)]
    public string ClassId { get; set; }

    [JsonProperty("ClassManagementPack", NullValueHandling = NullValueHandling.Ignore)]
    public string ClassManagementPack { get; set; }

    [JsonProperty("ClassName", NullValueHandling = NullValueHandling.Ignore)]
    public string ClassName { get; set; }
  }

  public class ManagingActionPoint
  {
    [JsonProperty("HealthService", NullValueHandling = NullValueHandling.Ignore)]
    public string HealthService { get; set; }

    [JsonProperty("ResourcePool", NullValueHandling = NullValueHandling.Ignore)]
    public ResourcePool ResourcePool { get; set; }
  }

  public class ResourcePool
  {
    [JsonProperty("DisplayName", NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayName { get; set; }

    [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }
  }

  public partial class Connector
  {
    [JsonProperty("DisplayName", NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayName { get; set; }

    [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }
  }

  public enum Operation { AddUpdate, Remove };

  public partial struct PropertyValue
  {
    public bool? Bool;
    public double? Double;
    public long? Integer;
    public string String;

    public static implicit operator PropertyValue(bool Bool) => new PropertyValue { Bool = Bool };
    public static implicit operator PropertyValue(double Double) => new PropertyValue { Double = Double };
    public static implicit operator PropertyValue(long Integer) => new PropertyValue { Integer = Integer };
    public static implicit operator PropertyValue(string String) => new PropertyValue { String = String };
    public bool IsNull => Bool == null && Double == null && Integer == null && String == null;
  }

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
      if (reader.TokenType == JsonToken.Null) return null;
      var value = serializer.Deserialize<string>(reader);
      switch (value)
      {
        case "AddUpdate":
          return Operation.AddUpdate;
        case "Remove":
          return Operation.Remove;
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
