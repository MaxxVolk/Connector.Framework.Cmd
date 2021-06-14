using Newtonsoft.Json;
using System;
using System.Collections.Generic;

/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  /// <summary>
  /// Class is defined by either an Id (Guid) OR an MP name plus a class name.
  /// </summary>
  public partial class ClassReference
  {
    [JsonProperty("ClassId", NullValueHandling = NullValueHandling.Ignore)]
    public string ClassId { get; set; }

    [JsonProperty("ClassManagementPack", NullValueHandling = NullValueHandling.Ignore)]
    public string ClassManagementPack { get; set; }

    [JsonProperty("ClassName", NullValueHandling = NullValueHandling.Ignore)]
    public string ClassName { get; set; }
  }
}
