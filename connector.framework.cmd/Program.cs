using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace connector.framework.cmd
{
  class Program
  {
    private static CommandArgument importCmdArgument, exportCmdArgument;
    private static CommandLineApplication commandLineApplication;
    private static CommandOption managementServer;
    static void Main(string[] args)
    {
      commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: true);
      commandLineApplication.Command("import", ConfigureImport, throwOnUnexpectedArg: true).OnExecute(DoImport);
      commandLineApplication.Command("export", ConfigureExport, throwOnUnexpectedArg: true).OnExecute(DoExport);
      managementServer = commandLineApplication.Option("-m |--managementServer <server_fqdn>", "SCOM Management Server address", CommandOptionType.SingleValue);
      commandLineApplication.HelpOption("-? | -h | --help");
      commandLineApplication.VersionOption("-v", "1.0");

      commandLineApplication.OnExecute(DefaultMain);
      commandLineApplication.Execute(args);
    }

    private static int DoExport()
    {
      Console.WriteLine($"DoExport: {exportCmdArgument.Value}, {managementServer.Value()}");
      return 0;
    }

    private static int DoImport()
    {
      Console.WriteLine($"DoImport: {importCmdArgument.Value}");
      return 0;
    }

    private static void ConfigureImport(CommandLineApplication obj)
    {
      importCmdArgument = obj.Argument("filename.json", "Path to a JSON file with object instance information.", multipleValues: false);
    }

    private static void ConfigureExport(CommandLineApplication obj)
    {
      exportCmdArgument = obj.Argument("filename.json", "Path to a JSON file where to save object instance information.", multipleValues: false);
    }

    private static int DefaultMain()
    {
      Console.WriteLine("Exec");
      return 0;
    }
  }
}
