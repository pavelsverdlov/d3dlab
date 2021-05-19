using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace D3DLab.Plugin
{
    public interface IPluginContext {
        public DirectoryInfo PluginDirectory { get; }
        public IEnumerable<IPluginLoadedObjectDetails> Objects { get; }
        public IPluginScene Scene { get; }
        //Window Window { get; }
    }
}