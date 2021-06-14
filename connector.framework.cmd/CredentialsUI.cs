using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct CREDUI_INFO
  {
    public int cbSize;
    public IntPtr hwndParent;
    public string pszMessageText;
    public string pszCaptionText;
    public IntPtr hbmBanner;
  }

  public enum CredUIReturnCodes: int
  {
    NO_ERROR = 0,
    ERROR_CANCELLED = 1223,
    ERROR_NO_SUCH_LOGON_SESSION = 1312,
    ERROR_NOT_FOUND = 1168,
    ERROR_INVALID_ACCOUNT_NAME = 1315,
    ERROR_INSUFFICIENT_BUFFER = 122,
    ERROR_INVALID_PARAMETER = 87,
    ERROR_INVALID_FLAGS = 1004,
    ERROR_BAD_ARGUMENTS = 160
  }

  /// <summary>
  /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnnetsec/html/dpapiusercredentials.asp
  /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/secauthn/security/creduipromptforcredentials.asp
  /// </summary>
  [Flags]
  public enum CREDUI_FLAGS : uint
  {
    /// <summary>
    /// The caller is requesting that the credential provider return the user name and password in plain text.
    /// This value cannot be combined with SECURE_PROMPT.
    /// </summary>
    CREDUIWIN_GENERIC = 0x1,
    /// <summary>
    /// The Save check box is displayed in the dialog box.
    /// </summary>
    CREDUIWIN_CHECKBOX = 0x2,
    CREDUI_FLAGS_DO_NOT_PERSIST = 0x00002,
    CREDUI_FLAGS_PASSWORD_ONLY_OK = 0x00200,
    CREDUI_FLAGS_GENERIC_CREDENTIALS = 0x40000,
    CREDUI_FLAGS_KEEP_USERNAME = 0x100000,

    REQUEST_ADMINISTRATOR = 0x4,
    EXCLUDE_CERTIFICATES = 0x8,

    /// <summary>
    /// Only credential providers that support the authentication package specified by the authPackage parameter should be enumerated.
    /// This value cannot be combined with CREDUIWIN_IN_CRED_ONLY.
    /// </summary>
    CREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
    /// <summary>
    /// Only the credentials specified by the InAuthBuffer parameter for the authentication package specified by the authPackage parameter should be enumerated.
    /// If this flag is set, and the InAuthBuffer parameter is NULL, the function fails.
    /// This value cannot be combined with CREDUIWIN_AUTHPACKAGE_ONLY.
    /// </summary>
    CREDUIWIN_IN_CRED_ONLY = 0x20,

    SHOW_SAVE_CHECK_BOX = 0x40,
    ALWAYS_SHOW_UI = 0x80,

    /// <summary>
    /// Credential providers should enumerate only administrators. This value is intended for User Account Control (UAC) purposes only. We recommend that external callers not set this flag.
    /// </summary>
    CREDUIWIN_ENUMERATE_ADMINS = 0x100,
    /// <summary>
    /// Only the incoming credentials for the authentication package specified by the authPackage parameter should be enumerated.
    /// </summary>
    CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,

    VALIDATE_USERNAME = 0x400,
    COMPLETE_USERNAME = 0x800,

    /// <summary>
    /// The credential dialog box should be displayed on the secure desktop. This value cannot be combined with CREDUIWIN_GENERIC.
    /// Windows Vista: This value is not supported until Windows Vista with SP1.
    /// </summary>
    CREDUIWIN_SECURE_PROMPT = 0x1000,

    SERVER_CREDENTIAL = 0x4000,
    EXPECT_CONFIRMATION = 0x20000,
    GENERIC_CREDENTIALS = 0x40000,
    USERNAME_TARGET_CREDENTIALS = 0x80000,

    /// <summary>
    /// The credential provider should align the credential BLOB pointed to by the refOutAuthBuffer parameter to a 32-bit boundary, even if the provider is running on a 64-bit system.
    /// </summary>
    CREDUIWIN_PACK_32_WOW = 0x10000000,
  }

  public static partial class Win32CredentialsManagerAPI
  {
    [DllImport("credui.dll", CharSet = CharSet.Unicode, EntryPoint = "CredUIPromptForCredentialsW", SetLastError = true)]
    internal static extern CredUIReturnCodes CredUIPromptForCredentialsW(ref CREDUI_INFO creditUR,
            string targetName,
            IntPtr reserved1,
            int iError,
            StringBuilder userName,
            int maxUserName,
            StringBuilder password,
            int maxPassword,
            [MarshalAs(UnmanagedType.Bool)] ref bool pfSave,
            CREDUI_FLAGS flags);

    [DllImport("credui.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    internal static extern CredUIReturnCodes CredUICmdLinePromptForCredentialsW(
            string targetName,
            IntPtr reserved1,
            int iError,
            StringBuilder userName,
            int maxUserName,
            StringBuilder password,
            int maxPassword,
            [MarshalAs(UnmanagedType.Bool)] ref bool pfSave,
            CREDUI_FLAGS flags);

    [DllImport("credui.dll", EntryPoint = "CredUIParseUserNameW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    internal static extern CredUIReturnCodes CredUIParseUserNameW(string userName, StringBuilder user, int userMaxChars, StringBuilder domain, int domainMaxChars);

  }
}
