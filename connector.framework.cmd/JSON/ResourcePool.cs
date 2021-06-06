using Newtonsoft.Json;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  public class ResourcePool
  {
    [JsonProperty("DisplayName", NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayName { get; set; }

    [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }
  }
}
