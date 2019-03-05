using D3DLab.Debugger.Presentation.PropertiesEditor;
using D3DLab.Debugger.Windows;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace D3DLab.Debugger.Presentation.SystemList {
    public class SystemItemViewModel {
        readonly IGraphicSystem system;

        public string Header { get; }
        public bool IsShaderEditable { get; }

        public SystemItemViewModel(IGraphicSystem system) {
            this.system = system;
            Header = system.ToString().Split('.').Last();
            IsShaderEditable = system is Std.Engine.Core.Shaders.IShadersContainer;
        }

        public IGraphicSystem GetOriginSystem() {
            return system;
        }
    }

    public class ViewState {
        public int Count => items.Count;
        public ICollectionView Items { get; }
        readonly ObservableCollection<SystemItemViewModel> items;

        public ViewState() {
            items = new ObservableCollection<SystemItemViewModel>();
            Items = CollectionViewSource.GetDefaultView(items);
        }

        internal void AddItem(SystemItemViewModel item) {
            items.Add(item);
        }
    }
    public class SystemViewController {
        public ICommand OpenShaderEditor { get; }
        public ICommand OpenPropertiesEditor { get; }
        public class OpenShaderEditorSystemItemCommand : OpenShaderEditorCommand<SystemItemViewModel> {
            public OpenShaderEditorSystemItemCommand(IRenderUpdater updater) : base(updater) { }
            protected override IShadersContainer Convert(SystemItemViewModel i) {
                return (IShadersContainer)i.GetOriginSystem();
            }
        }
        public class OpenPropertiesEditorSystemItemCommand : OpenPropertiesEditorCommand<SystemItemViewModel> {
            public class EditingPropertiesComponentItem : IEditingProperties {
                readonly SystemItemViewModel item;

                public string Titile => item.Header;
                public object TargetObject => item.GetOriginSystem();

                public EditingPropertiesComponentItem(SystemItemViewModel item) {
                    this.item = item;
                }

                public void MarkAsModified() {
                    throw new NotImplementedException();
                }
            }

            public OpenPropertiesEditorSystemItemCommand(IRenderUpdater updater) : base(updater) { }

            protected override IEditingProperties Convert(SystemItemViewModel item) {
                return new EditingPropertiesComponentItem(item);
            }
        }

        public SystemViewController(IRenderUpdater updater) {
            OpenShaderEditor = new OpenShaderEditorSystemItemCommand(updater);
            OpenPropertiesEditor = new OpenPropertiesEditorSystemItemCommand(updater);
        }
    }

    public class SystemViewPresenter : IRenderUpdater {
        public IAppWindow GameWindow { get; set; }

        public ViewState State { get; }
        public SystemViewController Controller { get; }

        readonly Dictionary<int, SystemItemViewModel> hash;

        public SystemViewPresenter() {            
            State = new ViewState();
            Controller = new SystemViewController(this);
            State.Items.CurrentChanged += OnCurrentChanged;
            hash = new Dictionary<int, SystemItemViewModel>();
        }

        private void OnCurrentChanged(object sender, EventArgs e) {
            
        }

        public void AddSystem(IGraphicSystem system) {
            var item = new SystemItemViewModel(system);
            State.AddItem(item);
        }

        public void Update() {
            //GameWindow.InputManager.PushCommand(new Std.Engine.Core.Input.Commands.ForceRenderCommand());
        }
    }
}
