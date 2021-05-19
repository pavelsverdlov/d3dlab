using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using D3DLab.Plugin;

namespace FileGeometryGroupsPlugin {
    public class PluginRunner : APluginRunner {
        public PluginRunner() : base("File geo group details", "allow show/hide and filter geo grops") {
        }

        protected override IPluginViewModel CreateViewModel(IPluginContext context) => new MainViewModel(context);
        protected override IPluginWindow CreateWindow()  => new MainWindow();
    }
}
