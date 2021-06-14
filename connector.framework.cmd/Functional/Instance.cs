using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  public partial class Instance
  {
    protected ManagementGroup ManagementGroup { get; set; }
    protected Dictionary<Guid, PropertyAndValue> PropertyMap = null;

    public void BindToManagementGroup(ManagementGroup managementGroup)
    {
      ManagementGroup = managementGroup;
      foreach (ClassAndInstancesReference instance in Children ?? new List<ClassAndInstancesReference>())
        instance.BindToManagementGroup(managementGroup);
      Host?.BindToManagementGroup(managementGroup);
      ManagingActionPoint?.BindToManagementGroup(managementGroup);
    }

    public IList<VerificationResult> Verify(ClassReference classDef)
    {
      List<VerificationResult> result = new List<VerificationResult>();

      foreach (ClassAndInstancesReference child in Children ?? new List<ClassAndInstancesReference>())
        result.AddRange(child.Verify());

      if (Operation == Operation.NotSpecified)
        result.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, "Operation for this instance is not cpecified. Use 'AddUpdate' or 'Remove' to specify instance operation.");

      ManagementPackClass currentClass = classDef.ManagementPackClass;
      if (currentClass == null)
        result.AppendVerification(VerificationAction.Stop, VerificationOutcome.Warning, "Class is not defined for the current instance. Cannot assess if Host property should be required or not.");
      else
      {
        // Class is hosted, but no host specified: STOP
        if (currentClass.Hosted && Host == null)
          result.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, "Current class is hosted, but host reference is not defined for the current instance.");

        // class is unhosted, but host is specified. Why?!: WARNING
        if (!currentClass.Hosted && Host != null)
          result.AppendVerification(VerificationAction.DisplayOnly, VerificationOutcome.Warning, "Current class is unhosted, but host reference is specified. Host reference will not be used.");

        // Class is hosted and host is specified: all good, but let's virify and parse the host data.
        if (currentClass.Hosted && Host != null)
          result.AddRange(Host.Verify());
      }

      if (currentClass == null)
        result.AppendVerification(VerificationAction.DisplayOnly, VerificationOutcome.Warning, "Class is not defined for the current instance. Cannot assess if ManagingActionPoint property can be defined or should not.");
      else
      {
        if (currentClass.Hosted && ManagingActionPoint != null)
          result.AppendVerification(VerificationAction.DisplayOnly, VerificationOutcome.Warning, "Current class is hosted, but Managing Action Point is specified. It's not appliacable to a hosted class and will be ignored.");
        if (!currentClass.Hosted && ManagingActionPoint == null)
          result.AppendVerification(VerificationAction.ApprovalRequired, VerificationOutcome.Warning, "Current class is unhosted, but Managing Action Point is not specified. In this case All Management Server Resource Pool will be used by dafault. Confirm?");
        if (!currentClass.Hosted && ManagingActionPoint != null)
          result.AddRange(ManagingActionPoint.Verify());
      }
      if (currentClass == null)
        result.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, "Class is not defined for the current instance. Cannot assess types for defined properties.");
      else
      {
        if (Properties == null)
          result.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, "No properties are defined.");
        else
        {
          PropertyMap = BuildPropertyMap(currentClass, result);

        }
      }

      return result;
    }

    private Dictionary<Guid, PropertyAndValue> BuildPropertyMap(ManagementPackClass currentClass, List<VerificationResult> errorList = null)
    {
      // NB!!! Names in 'Properties' CAN have duplicates despite it's a disctionary, because:
      // 1. Guids can be written in different cases,
      // 2. Same with property and class names.
      Dictionary<Guid, PropertyAndValue> result = new Dictionary<Guid, PropertyAndValue>();
      IList<ManagementPackProperty> allProperties = currentClass.GetProperties(BaseClassTraversalDepth.Recursive, PropertyExtensionMode.None);
      // prepare a dictionary of all peroperties for the class and all parent classes
      foreach (ManagementPackProperty mpProperty in allProperties)
        result.Add(mpProperty.Id, new PropertyAndValue { ManagementPackProperty = mpProperty, ValueAssigned = false, Value = null, ValueParsed = false, ValueData = null }); // no matched JSON propery by default
      // for each propery in JSON, let's find matching property in the MP property collection
      foreach (KeyValuePair<string, PropertyValue> jsonProperty in Properties)
      {
        if (Guid.TryParse(jsonProperty.Key ?? "", out Guid jId))
        {
          // property is defined by Id, Id is unique
          if (result.ContainsKey(jId))
          {
            result[jId].Value = jsonProperty.Value;
            result[jId].ValueAssigned = true;
          }
          else
            errorList.AppendVerification(VerificationAction.DisplayOnly, VerificationOutcome.Warning, $"Source data contains a property with Id {jId}, but the target class (or it's parent classes) don't have a propery with such Id.");
          continue;
        }
        int pathSeparator = jsonProperty.Key.IndexOf('\\');
        if (pathSeparator < 0) // not found, just name, no class name
        {
          IEnumerable<Guid> targetsByName = result.Where(r => r.Value.ManagementPackProperty.Name.ToUpperInvariant() == jsonProperty.Key.ToUpperInvariant()).Select(o => o.Key);
          if (targetsByName.Any())
          {
            if (targetsByName.Count() == 1)
            {
              result[targetsByName.First()].Value = jsonProperty.Value;
              result[jId].ValueAssigned = true;
            }
            else
              errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Warning, $"The target class and its parent class(es) contains multiple properties named {jsonProperty.Key}. Value binding is ambigous.");
          }
          else
            errorList.AppendVerification(VerificationAction.DisplayOnly, VerificationOutcome.Warning, $"Source data contains a property with name {jsonProperty.Key}, but the target class (or it's parent classes) don't have a propery with such name.");
        }
        else
        // name and class name
        {
          string className = jsonProperty.Key.Substring(0, pathSeparator).ToUpperInvariant();
          string propertyName = jsonProperty.Key.Substring(pathSeparator + 1, jsonProperty.Key.Length - pathSeparator).ToUpperInvariant();
          IEnumerable<Guid> targetByNameAndClass = result.Where(r => r.Value.ManagementPackProperty.Name.ToUpperInvariant() == propertyName && r.Value.ManagementPackProperty.ParentElement.Name.ToUpperInvariant() == className).Select(o => o.Key);
          if (targetByNameAndClass.Any())
          {
            result[targetByNameAndClass.First()].Value = jsonProperty.Value;
            result[jId].ValueAssigned = true;
          }
          else
            errorList.AppendVerification(VerificationAction.DisplayOnly, VerificationOutcome.Warning, $"Source data contains a property with class\\name {jsonProperty.Key}, but the target class (or it's parent classes) don't have a propery with such name.");
        }
      }

      // Test required and key fields exstence.
      if (!result.All(r=>!r.Value.ManagementPackProperty.Required || (r.Value.ManagementPackProperty.Required && r.Value.ValueAssigned)))
        errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Some required properties for the '{currentClass.DisplayName ?? currentClass.Name}' class are missing.");

      if (!result.All(r => !r.Value.ManagementPackProperty.Key || (r.Value.ManagementPackProperty.Key && r.Value.ValueAssigned)))
        errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Some key properties for the '{currentClass.DisplayName ?? currentClass.Name}' class are missing.");

      // parse values and check types
      foreach (KeyValuePair<Guid, PropertyAndValue> propItem in result)
      {
        PropertyAndValue pnv = propItem.Value;
        if (pnv.ValueAssigned)
          switch (pnv.ManagementPackProperty.Type)
          {
            case ManagementPackEntityPropertyTypes.binary:
              errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"The {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name} is binary. Not supported.");
              break;
            case ManagementPackEntityPropertyTypes.@bool:
              if (pnv.Value.Bool != null)
              {
                pnv.ValueData = pnv.Value.Bool ?? false;
                pnv.ValueParsed = true;
              }
              else if (!string.IsNullOrWhiteSpace(pnv.Value.String) && bool.TryParse(pnv.Value.String, out bool strBool))
              {
                pnv.ValueData = strBool;
                pnv.ValueParsed = true;
              }
              else if (pnv.Value.Integer != null)
              {
                pnv.ValueData = pnv.Value.Integer >= 1;
                pnv.ValueParsed = true;
              }
              else
                errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Failed to cast {pnv.Value} as boolean for for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name}.");
              break;
            case ManagementPackEntityPropertyTypes.datetime:
              if (!string.IsNullOrWhiteSpace(pnv.Value.String) && DateTime.TryParse(pnv.Value.String, null, System.Globalization.DateTimeStyles.AssumeLocal, out DateTime strDate))
              {
                pnv.ValueData = strDate;
                pnv.ValueParsed = true;
              }
              else
                errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Failed to cast {pnv.Value} as date and time for for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name}.");
              break;
            case ManagementPackEntityPropertyTypes.@decimal:
              if (!string.IsNullOrWhiteSpace(pnv.Value.String) && decimal.TryParse(pnv.Value.String, out decimal strDecimal))
              {
                pnv.ValueData = strDecimal;
                pnv.ValueParsed = true;
              }
              else if (pnv.Value.Double != null)
              {
                pnv.ValueData = Convert.ToDecimal(pnv.Value.Double);
                pnv.ValueParsed = true;
              }
              else if (pnv.Value.Integer != null)
              {
                pnv.ValueData = Convert.ToDecimal(pnv.Value.Integer);
                pnv.ValueParsed = true;
              }
              else
                errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Failed to cast {pnv.Value} as decimal for for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name}.");
              break;
            case ManagementPackEntityPropertyTypes.@double:
              if (pnv.Value.Double != null)
              {
                pnv.ValueData = pnv.Value.Double;
                pnv.ValueParsed = true;
              } else if (!string.IsNullOrWhiteSpace(pnv.Value.String) && double.TryParse(pnv.Value.String, out double strDouble))
              {
                pnv.ValueData = strDouble;
                pnv.ValueParsed = true;
              }
              else if (pnv.Value.Integer != null)
              {
                pnv.ValueData = Convert.ToDouble(pnv.Value.Integer);
                pnv.ValueParsed = true;
              }
              else
                errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Failed to cast {pnv.Value} as double for for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name}.");
              break;
            case ManagementPackEntityPropertyTypes.@enum:
              if (!string.IsNullOrWhiteSpace(pnv.Value.String))
              {
                IList<ManagementPackEnumeration> scomEnum = ManagementGroup.EntityTypes.GetChildEnumerations(pnv.ManagementPackProperty.EnumType.Id, Microsoft.EnterpriseManagement.Common.TraversalDepth.OneLevel);
                ManagementPackEnumeration myValue = scomEnum.FirstOrDefault(e => e.Name == pnv.Value.String || e.DisplayName == pnv.Value.String);
                if (myValue != null)
                {
                  pnv.ValueData = myValue;
                  pnv.ValueParsed = true;
                }
                else
                  errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"'{pnv.Value}' is not a valid enum value for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name}.");
              }
              else
                errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Failed to cast {pnv.Value} as enum for for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name}.");
              break;
            case ManagementPackEntityPropertyTypes.guid:
              if (!string.IsNullOrWhiteSpace(pnv.Value.String) && Guid.TryParse(pnv.Value.String, out Guid strGuid))
              {
                pnv.ValueData = strGuid;
                pnv.ValueParsed = true;
              }
              else
                errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Failed to cast {pnv.Value} as guid for for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name}.");
              break;
            case ManagementPackEntityPropertyTypes.@int:
              if (pnv.Value.Integer != null)
              {
                pnv.ValueData = pnv.Value.Integer;
                pnv.ValueParsed = true;
              }
              else if (!string.IsNullOrWhiteSpace(pnv.Value.String) && int.TryParse(pnv.Value.String, out int strInt))
              {
                pnv.ValueData = strInt;
                pnv.ValueParsed = true;
              }
              else
                errorList.AppendVerification(VerificationAction.Stop, VerificationOutcome.Critical, $"Failed to cast {pnv.Value} as int for for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name}.");
              break;
            case ManagementPackEntityPropertyTypes.richtext:
            case ManagementPackEntityPropertyTypes.@string:
              if (pnv.Value.ValueType == PropertyValueType.String || pnv.Value.ValueType == PropertyValueType.Null)
              {
                pnv.ValueData = pnv.Value.String; // allow nulls
                pnv.ValueParsed = true;
                if (pnv.ValueData == null)
                  errorList.AppendVerification(VerificationAction.DisplayOnly, VerificationOutcome.Informational, $"No value specidied for the {pnv.ManagementPackProperty.DisplayName ?? pnv.ManagementPackProperty.Name} property. However, null is a valid string value.");
              }
              break;
          }
      }

      return result;
    }
  }

  public class PropertyAndValue
  {
    public ManagementPackProperty ManagementPackProperty { get; set; }
    public bool ValueAssigned { get; set; } = false;
    public PropertyValue Value { get; set; }
    public bool ValueParsed { get; set; } = false;
    public object ValueData { get; set; }
  }
}
