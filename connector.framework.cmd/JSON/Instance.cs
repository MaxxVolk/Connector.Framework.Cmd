using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  public partial class Instance
  {
    [JsonProperty("Children", NullValueHandling = NullValueHandling.Ignore)]
    public List<ClassAndInstancesReference> Children { get; set; }

    [JsonProperty("Host", NullValueHandling = NullValueHandling.Ignore)]
    public InstanceReference Host { get; set; }

    [JsonProperty("ManagingActionPoint", NullValueHandling = NullValueHandling.Ignore)]
    public ManagingActionPoint ManagingActionPoint { get; set; }

    [JsonProperty("Operation", NullValueHandling = NullValueHandling.Ignore)]
    public Operation Operation { get; set; }

    [JsonProperty("Properties", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, PropertyValue> Properties { get; set; }
  }
}
