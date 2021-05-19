using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace D3DLab.Plugin {
    public interface IPluginWindow {
        event EventHandler Closed;
        object DataContext { get; set; }
        Dispatcher Dispatcher { get; }
        void Close();
        void Show();
    }
    public interface IPluginViewModel {
        void Closed();
        void Init();
    }

    public abstract class APluginRunner : IPlugin {
        public string Name { get; }
        public string Description { get; }

        IPluginWindow win;
        protected APluginRunner(string name, string description) {
            Name = name;
            Description = description;
        }


        public Task CloseAsync() {
            win?.Close();
            return Task.CompletedTask;
        }

        public Task ExecuteAsync(IPluginContext context) {

            var task = Task.CompletedTask;
            try {
                win = CreateWindow();
                // w.Owner = context.Window;
                var vm = CreateViewModel(context);
                win.DataContext = vm;
                win.Closed += (o, e) => vm.Closed();

                win.Show();

                task = win.Dispatcher.InvokeAsync(() => {
                    vm.Init();
                }).Task;
            } catch (Exception ex) {
                throw ex;
            }

            return task;
        }

        protected abstract IPluginViewModel CreateViewModel(IPluginContext context);
        protected abstract IPluginWindow CreateWindow();
    }
}
