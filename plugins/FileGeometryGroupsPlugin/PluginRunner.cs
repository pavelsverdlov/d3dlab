using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using D3DLab.Plugin;

namespace FileGeometryGroupsPlugin {
    public class PluginRunner : APluginRunner {
        public PluginRunner() : base("File geometry group details", "allow to show/hide and filter geometry groups, determined in file.") {
        }

        protected override IPluginViewModel CreateViewModel(IPluginContext context) => new MainViewModel(context);
        protected override IPluginWindow CreateWindow()  => new MainWindow();
    }
}
