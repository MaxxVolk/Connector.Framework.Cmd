using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  public static class VerificationHelper
  {
    public static void AppendVerification(this IList<VerificationResult> result, VerificationAction action, VerificationOutcome severity, string message)
    {
      result.Add(new VerificationResult
      {
        Message = message,
        VerificationAction = action,
        VerificationOutcome = severity
      });
    }

    public static void AppendVerification(this List<VerificationResult> result, VerificationAction action, VerificationOutcome severity, string message)
    {
      result.Add(new VerificationResult
      {
        Message = message,
        VerificationAction = action,
        VerificationOutcome = severity
      });
    }
  }
}
