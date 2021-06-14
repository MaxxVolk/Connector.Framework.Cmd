using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.ConnectorFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  public partial class ImportFile
  {
    protected ManagementGroup ManagementGroup { get; set; }
    public void BindToManagementGroup(ManagementGroup managementGroup)
    {
      ManagementGroup = managementGroup;
      Connector?.BindToManagementGroup(managementGroup);
      ClassAndInstaces?.BindToManagementGroup(managementGroup);
    }

    public IList<VerificationResult> Verify()
    {
      List<VerificationResult> result = new List<VerificationResult>();

      if (Connector == null)
        result.Add(new VerificationResult
        {
          VerificationAction = VerificationAction.ApprovalRequired,
          VerificationOutcome = VerificationOutcome.Warning,
          Message = "If no Connector is specified, then the Default Connector will be used. This option is not recommended and might be dangerous. Do you wish to proceed?"
        });
      else
        result.AddRange(Connector.Verify());

      if (ClassAndInstaces == null)
        result.Add(new VerificationResult
        {
          VerificationAction = VerificationAction.Stop,
          VerificationOutcome = VerificationOutcome.Critical,
          Message = "No class and class instances are specified, therefore no action will be taken."
        });
      else
        result.AddRange(ClassAndInstaces.Verify());

      return result;
    }
  }
}
