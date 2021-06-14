using Microsoft.EnterpriseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  public partial class ClassAndInstancesReference
  {
    protected ManagementGroup ManagementGroup { get; set; }

    public void BindToManagementGroup(ManagementGroup managementGroup)
    {
      ManagementGroup = managementGroup;
      Class?.BindToManagementGroup(managementGroup);
      foreach (Instance instance in InstanceCollection ?? new List<Instance>())
        instance.BindToManagementGroup(managementGroup);
    }

    public IEnumerable<VerificationResult> Verify()
    {
      List<VerificationResult> result = new List<VerificationResult>();

      if (Class == null)
        result.Add(new VerificationResult
        {
          VerificationAction = VerificationAction.Stop,
          VerificationOutcome = VerificationOutcome.Critical,
          Message = "Class type must be specified."
        });
      else
        result.AddRange(Class.Verify());

      if (InstanceCollection == null)
        result.Add(new VerificationResult
        {
          VerificationAction = VerificationAction.DisplayOnly,
          VerificationOutcome = VerificationOutcome.Warning,
          Message = "No class instances specified, no action will be taken."
        });
      else
        foreach (Instance instance in InstanceCollection ?? new List<Instance>())
          result.AddRange(instance.Verify(Class));

      return result;
    }
  }
}
