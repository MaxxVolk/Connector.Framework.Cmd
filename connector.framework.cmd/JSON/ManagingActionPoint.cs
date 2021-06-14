using Newtonsoft.Json;
using System;
using System.Collections.Generic;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  public partial class ManagingActionPoint
  {
    [JsonProperty("HealthService", NullValueHandling = NullValueHandling.Ignore)]
    public string HealthService { get; set; }

    [JsonProperty("ResourcePool", NullValueHandling = NullValueHandling.Ignore)]
    public ResourcePool ResourcePool { get; set; }

    internal IList<VerificationResult> Verify()
    {
      List<VerificationResult> results = new List<VerificationResult>();

      return results;
    }
  }
}
