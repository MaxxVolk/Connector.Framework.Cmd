using Newtonsoft.Json;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  public class ManagingActionPoint
  {
    [JsonProperty("HealthService", NullValueHandling = NullValueHandling.Ignore)]
    public string HealthService { get; set; }

    [JsonProperty("ResourcePool", NullValueHandling = NullValueHandling.Ignore)]
    public ResourcePool ResourcePool { get; set; }
  }
}
