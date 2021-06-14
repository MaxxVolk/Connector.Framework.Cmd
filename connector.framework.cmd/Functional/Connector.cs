using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  public partial class Connector
  {
    protected ManagementGroup ManagementGroup { get; set; }

    public void BindToManagementGroup(ManagementGroup managementGroup)
    {
      ManagementGroup = managementGroup;
    }

    public IList<VerificationResult> Verify()
    {
      List<VerificationResult> result = new List<VerificationResult>();

      if (!string.IsNullOrWhiteSpace(Id) && !Guid.TryParse(Id, out Guid _))
        result.Add(new VerificationResult
        {
          VerificationOutcome = VerificationOutcome.Warning,
          VerificationAction = VerificationAction.Stop,
          Message = "Connector Id is not a valid GUID."
        });
      if (!string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(DisplayName))
        result.Add(new VerificationResult
        {
          VerificationOutcome = VerificationOutcome.Warning,
          VerificationAction = VerificationAction.DisplayOnly,
          Message = "Ambigous configuration: both Id and Display name are specified for a Connector. Id will take precidence."
        });
      if (!string.IsNullOrWhiteSpace(Id) && Guid.TryParse(Id, out Guid connectorId))
      {
        try
        {
          ManagementGroup.ConnectorFramework.GetConnector(connectorId);
        }
        catch (ObjectNotFoundException)
        {
          result.Add(new VerificationResult
          {
            VerificationOutcome = VerificationOutcome.Critical,
            VerificationAction = VerificationAction.Stop,
            Message = "Connector Id is specified, but a connector with such Id doesn't exist."
          });
        }
      }
      if (!string.IsNullOrWhiteSpace(DisplayName))
      {
        if (ManagementGroup.ConnectorFramework.GetConnectors().FirstOrDefault(c => c.DisplayName.ToLowerInvariant() == DisplayName.ToLowerInvariant()) == null)
          result.Add(new VerificationResult
          {
            VerificationOutcome = VerificationOutcome.Critical,
            VerificationAction = VerificationAction.Stop,
            Message = "Connector Display Name is specified, but a connector with such name doesn't exist."
          });
      }

      return result;
    }
  }
}
