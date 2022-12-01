using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiClient.App.Models
{
    public static class AppContext
    {
        public static class Globals
        {
            public const string CFG_PATH = @"Assets\Cfg";
            public static class Paths
            {
                public static Ambience.Variable OutPut;
                public static Ambience.Variable Cache;
                public static Ambience.Variable Resources;
                public static Ambience.Variable Logs;

            }
            public static Dictionary<string, Ambience.File> CachedFiles { get; set; }
            public static Ambience.Variable ProjectName;
            public static Ambience.Variable Dir;
            public static Ambience.Variable Logging;
            public static Ambience.Variable Aes_Key;
            public static Ambience.Variable Aes_Iv;

        }
        public static class Ambience
        {
            public struct Variable
            {
                public object Value { get; set; }
                public string Name { get; set; }
                public void Refresh() => Refresher.Invoke();
                public void Load() => Loader.Invoke();
                public Action Refresher;
                public Action Loader;
            }
            public struct File : IDisposable
            {
                public object Content;
                public bool IsCached { get => Globals.CachedFiles.ContainsKey(Name); }
                public File(bool CacheIt) : this()
                {
                    if (CacheIt) this.Store();
                }

                public string Name;
                public string Path;
                public void Refresh() => System.IO.File.ReadAllText(Path + Name);

                public void Store() => Globals.CachedFiles.Add(Name, this);
                public void Drop() => Globals.CachedFiles.Remove(Name);
                public void Load() => System.IO.File.WriteAllText(Path + Name, Content as string);
                public void Dispose() => Drop();
            }
        }
    } 
}