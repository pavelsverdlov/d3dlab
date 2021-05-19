using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

using McMaster.NETCore.Plugins;

namespace D3DLab.Plugin {
    public class PluginProxy {
        public List<LoadedPlugin> Plugins { get; }

        readonly string directory;
        readonly string pluginFormatName;

        public PluginProxy(string directory, string pluginFormatName = "*Plugin.dll") {
            Plugins = new List<LoadedPlugin>();
            this.directory = directory;
            this.pluginFormatName = pluginFormatName;
        }

        public void Load() {
            Plugins.Clear();

            var type = typeof(IPlugin);
            foreach (var pluginDll in Directory.GetFiles(directory, pluginFormatName, SearchOption.AllDirectories)) {
                if (File.Exists(pluginDll) && pluginDll.EndsWith("Plugin.dll")) {
                    try {
                        var loader = PluginLoader.CreateFromAssemblyFile(
                            pluginDll,
                            sharedTypes: new[] { type },
                            config => config.DefaultContext = AssemblyLoadContext.Default);

                        var types = loader.LoadDefaultAssembly().GetTypes();

                        foreach (var pluginType in types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract)) {
                            if (Activator.CreateInstance(pluginType) is IPlugin plugin) {
                                Plugins.Add(new LoadedPlugin(plugin, new FileInfo(pluginDll)));
                            }
                        }
                    } catch {
                        //ignore 
                    }
                }
            }

        }
    }
}