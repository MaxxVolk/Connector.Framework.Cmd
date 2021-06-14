using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  public partial class ClassReference
  {
    protected ManagementGroup ManagementGroup { get; set; }

    public void BindToManagementGroup(ManagementGroup managementGroup)
    {
      ManagementGroup = managementGroup;
    }

    public List<VerificationResult> Verify()
    {
      List<VerificationResult> result = new List<VerificationResult>();

      if (string.IsNullOrWhiteSpace(ClassId) && (string.IsNullOrWhiteSpace(ClassManagementPack) || string.IsNullOrWhiteSpace(ClassName)))
        result.Add(new VerificationResult
        {
          VerificationAction = VerificationAction.Stop,
          VerificationOutcome = VerificationOutcome.Critical,
          Message = "Class must be specified. Use Id or ClassManagementPack and ClassName parameters to specify a class."
        });
      else
      {
        if (!string.IsNullOrWhiteSpace(ClassId) && !string.IsNullOrWhiteSpace(ClassManagementPack) && !string.IsNullOrWhiteSpace(ClassName))
          result.Add(new VerificationResult
          {
            VerificationAction = VerificationAction.DisplayOnly,
            VerificationOutcome = VerificationOutcome.Warning,
            Message = "Ambigous class reference. Use Id or ClassManagementPack and ClassName parameters to specify a class. When both specified, Id will take precedence."
          });
        if (!string.IsNullOrWhiteSpace(ClassId) && !Guid.TryParse(ClassId, out Guid _))
          result.Add(new VerificationResult
          {
            VerificationOutcome = VerificationOutcome.Warning,
            VerificationAction = VerificationAction.Stop,
            Message = "ClassId property doesn't contain a valid GUID."
          });
        if (string.IsNullOrWhiteSpace(ClassManagementPack) ^ string.IsNullOrWhiteSpace(ClassName))
          result.Add(new VerificationResult
          {
            VerificationOutcome = VerificationOutcome.Warning,
            VerificationAction = VerificationAction.Stop,
            Message = "When defining class by MP name and class name, both properties must be specified."
          });
        if (!string.IsNullOrWhiteSpace(ClassManagementPack) && !string.IsNullOrWhiteSpace(ClassName))
        {
          try
          {
            ManagementPack mp = ManagementGroup.ManagementPacks.GetManagementPacks(new ManagementPackCriteria($"Name='{ClassManagementPack}'")).FirstOrDefault();
            _ManagementPackClass = ManagementGroup.EntityTypes.GetClass(ClassName, mp ?? throw new ObjectNotFoundException());
          }
          catch (ObjectNotFoundException)
          {
            result.Add(new VerificationResult
            {
              VerificationOutcome = VerificationOutcome.Critical,
              VerificationAction = VerificationAction.Stop,
              Message = "MP name and Class name are specified, but a class cannot be found."
            });
          }
        }
        if (!string.IsNullOrWhiteSpace(ClassId) && !Guid.TryParse(ClassId, out Guid classId))
        {
          try
          {
            _ManagementPackClass = ManagementGroup.EntityTypes.GetClass(classId);
          }
          catch (ObjectNotFoundException)
          {
            result.Add(new VerificationResult
            {
              VerificationOutcome = VerificationOutcome.Critical,
              VerificationAction = VerificationAction.Stop,
              Message = "ClassId is specified, but a class with such Id doesn't exist."
            });
          }
        }
      }

      return result;
    }

    private ManagementPackClass _ManagementPackClass;
    public ManagementPackClass ManagementPackClass => _ManagementPackClass;
  }
}
