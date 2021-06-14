using Microsoft.EnterpriseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  public partial class ManagingActionPoint
  {
    protected ManagementGroup ManagementGroup { get; set; }

    public void BindToManagementGroup(ManagementGroup managementGroup)
    {
      ManagementGroup = managementGroup;
    }

    internal IList<VerificationResult> Verify()
    {
      List<VerificationResult> results = new List<VerificationResult>();

      if (HealthService != null && ResourcePool != null)
        results.AppendVerification(VerificationAction.Stop, VerificationOutcome.Warning, "HealthService and ResourcePool are mutually exlusive for ManagingActionPoint");
      if (HealthService == null && ResourcePool == null)
        results.AppendVerification(VerificationAction.Stop, VerificationOutcome.Warning, "Either HealthService or ResourcePool must be specified for ManagingActionPoint");

      return results;
    }
  }
}
