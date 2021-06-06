using System;

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
}
