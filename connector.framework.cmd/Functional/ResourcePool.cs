using Microsoft.EnterpriseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd.Functional
{
  public partial class ResourcePool
  {
    protected ManagementGroup ManagementGroup { get; set; }

    public void BindToManagementGroup(ManagementGroup managementGroup)
    {
      ManagementGroup = managementGroup;
    }
  }
}
