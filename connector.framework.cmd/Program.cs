using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.ConnectorFramework;
using Microsoft.EnterpriseManagement.Monitoring;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  class Program
  {
    private static CommandArgument importCmdArgument, exportCmdArgument, createConnectorCmdArgument;
    private static CommandLineApplication commandLineApplication;
    private static CommandOption managementServerOption, connectorNameOption, connectorDescriptionOption, connectorDDIsManagedOption, 
      passwordOption, usernameOption, interactiveCredentialsOption;

    private static ManagementGroup managementGroup;

    static void Main(string[] args)
    {
      commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: true);
      commandLineApplication.Command("import", ConfigureImport, throwOnUnexpectedArg: true).OnExecute(DoImport);
      commandLineApplication.Command("export", ConfigureExport, throwOnUnexpectedArg: true).OnExecute(DoExport);
      commandLineApplication.Command("createConnector", ConfigureCreateConnector, throwOnUnexpectedArg: true).OnExecute(DoCreateConnector);
      managementServerOption = commandLineApplication.Option("-m |--managementServer <server_fqdn>", "SCOM Management Server address. Local host is used if not specified.", CommandOptionType.SingleValue);
      passwordOption = commandLineApplication.Option("-p |--password <password>", "Password.", CommandOptionType.SingleValue);
      usernameOption = commandLineApplication.Option("-u |--username <user_name>", "User name.", CommandOptionType.SingleValue);
      interactiveCredentialsOption = commandLineApplication.Option("-i |--interactiveCredentials", "Interactively ask for user name and password.", CommandOptionType.NoValue);
      commandLineApplication.HelpOption("-? | -h | --help");
      commandLineApplication.VersionOption("-v", "SCOM Connector Framework Command Line Tool 1.0");

      commandLineApplication.OnExecute(DefaultMain);
      commandLineApplication.Execute(args);
    }

    private static void ConfigureCreateConnector(CommandLineApplication createConnectorCommand)
    {
      createConnectorCmdArgument = createConnectorCommand.Argument("DisplayName", "Display name of a new connector.", multipleValues: false);
      connectorNameOption = createConnectorCommand.Option("--connectorName <name>", "Connector name.", CommandOptionType.SingleValue);
      connectorDescriptionOption = createConnectorCommand.Option("--connectorDescription <description>", "Connector description.", CommandOptionType.SingleValue);
      connectorDDIsManagedOption = createConnectorCommand.Option("--connectorDiscoveryDataIsManaged <true_or_false>", "Specifies id instances inserted via the connector should be managed or not. True if not specified.", CommandOptionType.SingleValue);
      createConnectorCommand.HelpOption("-? | -h | --help");
    }

    private static int DoCreateConnector()
    {
      if (DoConnectManagementGroup() != 0)
        return -1;
      string cDisplayName = createConnectorCmdArgument.Value;
      if (string.IsNullOrEmpty(cDisplayName))
      {
        Console.WriteLine("Connector Displa Name must not be empty.");
        return -1;
      }
      string cName = connectorNameOption.Value() ?? cDisplayName;
      string cDescription = connectorDescriptionOption.Value() ?? "";
      bool DDIM = connectorDDIsManagedOption.HasValue() ? bool.Parse(connectorDDIsManagedOption.Value()) : true;
      Guid cId = Guid.NewGuid();

      MonitoringConnector myConnnector = managementGroup.ConnectorFramework.Setup(new ConnectorInfo()
      {
        Name = cName,
        DisplayName = cDisplayName,
        Description = cDescription,
        DiscoveryDataIsManaged = DDIM
      }, cId);
      if (!myConnnector.Initialized)
        myConnnector.Initialize();

      Console.WriteLine($"Connector created successfully. Connector Id: {myConnnector.Id}");

      return 0;
    }

    private static int DoExport()
    {
      Console.WriteLine($"DoExport: {exportCmdArgument.Value}, {managementServerOption.Value()}");
      if (DoConnectManagementGroup() != 0)
        return -1;
      return 0;
    }

    private static int DoImport()
    {
      Console.WriteLine($"DoImport: {importCmdArgument.Value}");
      if (DoConnectManagementGroup() != 0)
        return -1;
      ImportFile sourceData = JsonConvert.DeserializeObject<ImportFile>(File.ReadAllText(importCmdArgument.Value), Converter.Settings);
      sourceData.BindToManagementGroup(managementGroup);
      IList<VerificationResult> verificationResults = sourceData.Verify();
      bool stop = false;
      bool approved = true;
      foreach(VerificationResult vr in verificationResults.Where(v=>v.VerificationAction != VerificationAction.ApprovalRequired))
      {
        Console.WriteLine($"{vr.VerificationAction} | {vr.VerificationOutcome} : {vr.Message}");
        stop |= vr.VerificationAction == VerificationAction.Stop;
      }
      if (stop)
      {
        Console.WriteLine("Some errors don't allow to proceed further. Fix and try again.");
        return -1;
      }  
      foreach (VerificationResult vr in verificationResults.Where(v => v.VerificationAction == VerificationAction.ApprovalRequired))
      {
        Console.WriteLine($"{vr.VerificationAction} | {vr.VerificationOutcome} : {vr.Message}");
        Console.Write("Enter [Y]es or [N]o: ");
        string inp;
        do
        {
          inp = Console.ReadLine();
        } while (string.IsNullOrEmpty(inp) && inp.Length < 1);
        approved &= inp.ToUpperInvariant()[0] == 'Y';
      }
      if (!approved)
      {
        Console.WriteLine("Operation cancelled");
        return -1;
      }

      PerformDiscovery(managementGroup, sourceData);

      return 0;
    }

    private static void PerformDiscovery(ManagementGroup mg, ImportFile sourceData)
    {
      // connector
      MonitoringConnector myConnector = sourceData.Connector?.MonitoringConnector;
      if (myConnector == null)
      {
        // use default
        myConnector = mg.ConnectorFramework.GetConnector(mg.ConnectorFramework.GetDefaultConnectorId());
      }

      // class
      ManagementPackClass myClass = sourceData.ClassAndInstaces.Class.ManagementPackClass;

      // instances
      
      foreach (Instance instanceDef in sourceData.ClassAndInstaces.InstanceCollection)
      {
        IncrementalDiscoveryData incrementalDiscovery = new IncrementalDiscoveryData();
        CreatableEnterpriseManagementObject newInstance = new CreatableEnterpriseManagementObject(mg, myClass);
        foreach (KeyValuePair<Guid, PropertyAndValue> propPair in instanceDef.PropertyMap)
          if (propPair.Value.ValueParsed)
            newInstance[propPair.Value.ManagementPackProperty].Value = propPair.Value.ValueData;
        if (instanceDef.Host != null)
          throw new NotSupportedException();
        if (instanceDef.Operation == Operation.AddUpdate && instanceDef.ManagingActionPoint != null)
          throw new NotSupportedException();
        if (instanceDef.Operation == Operation.AddUpdate)
          incrementalDiscovery.Add(newInstance);
        if (instanceDef.Operation == Operation.Remove)
          incrementalDiscovery.Remove(newInstance);

        bool objectExists = false;
        try
        {
          mg.EntityObjects.GetObject<EnterpriseManagementObject>(newInstance.Id, ObjectQueryOptions.Default);
          objectExists = true;
        }
        catch (ObjectNotFoundException) { }

        if (instanceDef.Operation == Operation.AddUpdate && objectExists)
          incrementalDiscovery.Overwrite(myConnector);
        else
          incrementalDiscovery.Commit(myConnector);
      }
    }

    private static void ConfigureImport(CommandLineApplication importCommand)
    {
      importCmdArgument = importCommand.Argument("filename.json", "Path to a JSON file with object instance information.", multipleValues: false);
      importCommand.HelpOption("-? | -h | --help");
    }

    private static void ConfigureExport(CommandLineApplication exportCommand)
    {
      exportCmdArgument = exportCommand.Argument("filename.json", "Path to a JSON file where to save object instance information.", multipleValues: false);
      exportCommand.HelpOption("-? | -h | --help");
    }

    private static int DefaultMain()
    {
      Console.WriteLine("Exec");
      return 0;
    }

    private static int DoConnectManagementGroup()
    {
      try
      {
        string managementServerName = (managementServerOption.HasValue() && !string.IsNullOrWhiteSpace(managementServerOption.Value())) ? managementServerOption.Value() : "localhost";
        ManagementGroupConnectionSettings connectionSettings = new ManagementGroupConnectionSettings(managementServerName);
        // interactive credentials
        if (interactiveCredentialsOption.HasValue())
        {
          if (ReadPassword(out string domain, out string username, out SecureString password) == 0)
          {
            if (!string.IsNullOrWhiteSpace(domain))
              connectionSettings.Domain = domain;
            connectionSettings.UserName = username;
            connectionSettings.Password = password;
          }
          else
            return -1;
        } 
        else
        {
          // if interactive specified, it will take precidence 
          if (passwordOption.HasValue() && !string.IsNullOrWhiteSpace(passwordOption.Value()))
          {
            SecureString spwd = new SecureString();
            string nspwd = passwordOption.Value();
            for (int i = 0; i < nspwd.Length; i++)
              spwd.AppendChar(nspwd[i]);
            connectionSettings.Password = spwd;
          }
          if (usernameOption.HasValue() && !string.IsNullOrWhiteSpace(usernameOption.Value()))
          {
            string origUname = usernameOption.Value();
            int domainSplitIdx = origUname.IndexOf('\\');
            if (domainSplitIdx >= 0)
            {
              connectionSettings.Domain = origUname.Substring(0, domainSplitIdx);
              connectionSettings.UserName = origUname.Substring(domainSplitIdx + 1, origUname.Length - domainSplitIdx - 1);
            }
            else
              connectionSettings.UserName = origUname;
          }
        }
        managementGroup = new ManagementGroup(connectionSettings);
        return 0;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        return -1;
      }
    }

    private static int ReadPassword(out string domain, out string username, out SecureString password)
    {
      CREDUI_INFO info = new CREDUI_INFO
      {
        pszMessageText = "Enter SCOM management group credentials",
        hbmBanner = IntPtr.Zero,
        hwndParent = IntPtr.Zero,
        pszCaptionText = "SCOM Management Group authentication"
      };
      info.cbSize = Marshal.SizeOf(info);
      StringBuilder sb_password = new StringBuilder(100);
      StringBuilder sb_username = new StringBuilder(100);
      bool save = false;
      CREDUI_FLAGS flags = CREDUI_FLAGS.ALWAYS_SHOW_UI | CREDUI_FLAGS.CREDUI_FLAGS_GENERIC_CREDENTIALS | CREDUI_FLAGS.CREDUI_FLAGS_DO_NOT_PERSIST |
        CREDUI_FLAGS.COMPLETE_USERNAME;
      CredUIReturnCodes pcResult = Win32CredentialsManagerAPI.CredUIPromptForCredentialsW(ref info, "", IntPtr.Zero, 0, sb_username, 100, sb_password, 100, ref save, flags);
      if (pcResult == CredUIReturnCodes.NO_ERROR)
      {
        StringBuilder sb_domain = new StringBuilder(100);
        StringBuilder sb_account = new StringBuilder(100);

        CredUIReturnCodes parResult = Win32CredentialsManagerAPI.CredUIParseUserNameW(sb_username.ToString(), sb_account, 100, sb_domain, 100);
        if (parResult == CredUIReturnCodes.NO_ERROR)
        {
          domain = sb_domain.ToString();
          username = sb_account.ToString();
          password = new SecureString();
          for (int i = 0; i < sb_password.Length; i++)
            password.AppendChar(sb_password[i]);
          return 0;
        }
        if (parResult == CredUIReturnCodes.ERROR_INVALID_ACCOUNT_NAME && !string.IsNullOrWhiteSpace(sb_username.ToString()))
        {
          // nothing to parse
          domain = "";
          username = sb_username.ToString().Trim(new char[] { '\\' }); // as it is
          password = new SecureString();
          for (int i = 0; i < sb_password.Length; i++)
            password.AppendChar(sb_password[i]);
          return 0;
        }
      }
      // any unsuccessfull fail here
      (domain, username, password) = ("", "", null);
      return -1;
    }
  }
}
