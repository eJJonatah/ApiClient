using static ApiClient.App.Controllers.Management;
using static ApiClient.App.Models.AppContext.Globals;
using static ApiClient.App.Models.AppContext;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ApiClient
{
    public class Setup
    {
        public static Dictionary<string, string> CfgImport()
        {
            using (var cfgFile = Files.Get(CFG_PATH))
            {
                var cfgData = new Dictionary<string, string>();
                foreach (var line in cfgFile.Content.ToString().Split('\n'))
                {
                    var lineSplitted = line.Split('=');
                    cfgData.Add(lineSplitted[0], lineSplitted[1]);
                }
                return cfgData;
            }
        }
        public static Tuple<string, string> GetLineCfg(string varName)
        {
            var cfgVar = (
                            from var in CfgImport()
                            where var.Key == varName
                            select var
                         ).First();

            return new Tuple<string, string>(cfgVar.Key, cfgVar.Value);
        }
        public static void AttributeCfg(Dictionary<string, string> cfgData)
        {
            Paths.OutPut.Value    = cfgData[Paths.OutPut.Name];
            Paths.Cache.Value     = cfgData[Paths.Cache.Name];
            Paths.Resources.Value = cfgData[Paths.Resources.Name];
            Paths.Logs.Value      = cfgData[Paths.Logs.Name];
            Logging.Value         = cfgData[Logging.Name];
            Aes_Key.Value         = cfgData[Aes_Key.Name];
            Aes_Iv.Value          = cfgData[Aes_Iv.Name];
        }
        public static class Configuration
        {
            //Support method, to call the constructor
            public static void Run() { } 
            static Configuration()
            {
                Initialize();
                AttributeCfg(
                    cfgData: CfgImport()
                );
            }
            public static void Initialize()
            {
                Action<Ambience.Variable> defaultRefresh = (Ambience.Variable currentVar) =>
                {
                    currentVar.Value = GetLineCfg(currentVar.Name).Item2;
                };
                Paths.OutPut    = new Ambience.Variable() { Name = "OutPut",      Refresher = () => defaultRefresh(Paths.OutPut) };
                Paths.Cache     = new Ambience.Variable() { Name = "Cache",       Refresher = () => defaultRefresh(Paths.Cache) };
                Paths.Resources = new Ambience.Variable() { Name = "Resources",   Refresher = () => defaultRefresh(Paths.Resources) };
                Paths.Logs      = new Ambience.Variable() { Name = "Logs",        Refresher = () => defaultRefresh(Paths.Logs) };
                Logging         = new Ambience.Variable() { Name = "Logging",     Refresher = () => defaultRefresh(Logging) };
                Aes_Key         = new Ambience.Variable() { Name = "Aes_Key",     Refresher = () => defaultRefresh(Aes_Key) };
                Aes_Iv          = new Ambience.Variable() { Name = "Aes_Iv",      Refresher = () => defaultRefresh(Aes_Iv) };
                ProjectName     = new Ambience.Variable() { Name = "ProjectName" };
                Dir             = new Ambience.Variable() { Name = "Dir" };
                CachedFiles     = new Dictionary<string, Ambience.File>();
                // Already Setted Ones
                Dir.Value = Environment.CurrentDirectory;
                ProjectName.Value = typeof(Globals).Namespace.Split('.')[0];
            }
        }
    }
}