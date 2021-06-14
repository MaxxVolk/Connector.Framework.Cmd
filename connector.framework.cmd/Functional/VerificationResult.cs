using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  public enum VerificationOutcome { Informational, Warning, Critical }
  public enum VerificationAction { DisplayOnly, ApprovalRequired, Stop }
  public class VerificationResult
  {
    public VerificationOutcome VerificationOutcome { get; set; }
    public VerificationAction VerificationAction { get; set; }
    public string Message { get; set; }
  }
}
