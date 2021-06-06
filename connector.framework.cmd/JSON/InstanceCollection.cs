using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
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
}
