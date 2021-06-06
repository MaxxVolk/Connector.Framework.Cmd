using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  public class ClassAndInstancesReference
  {
    [JsonProperty("Class", NullValueHandling = NullValueHandling.Ignore)]
    public ClassReference Class { get; set; }

    [JsonProperty("InstanceCollection", NullValueHandling = NullValueHandling.Ignore)]
    public List<InstanceCollection> InstanceCollection { get; set; }
  }
}
