using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  public partial class InstanceReference
  {
    [JsonProperty("Class", NullValueHandling = NullValueHandling.Ignore)]
    public ClassReference Class { get; set; }

    [JsonProperty("ClassInstanceKey", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, PropertyValue> ClassInstanceKey { get; set; }

    [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; set; }

    internal IEnumerable<VerificationResult> Verify()
    {
      //TODO:  
      List<VerificationResult> results = new List<VerificationResult>();

      return results;
    }
  }
}
