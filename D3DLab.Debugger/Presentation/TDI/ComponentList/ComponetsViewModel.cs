using D3DLab.ECS;
using D3DLab.Debugger.ECSDebug;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Debugger.Presentation.TDI.ComponentList {
    class ComponetsViewModel : BaseNotify, IRenderUpdater, ITreeItemActions, IPropertyTabManager {
        string consoleText;

        public string ConsoleText {
            get {
                return consoleText;
            }

            set {
                consoleText = value;
                SetPropertyChanged(nameof(ConsoleText));
            }
        }

        string title;
        private BaseWPFCommand<IVisualTreeEntityItem> _renderModeSwither;

        public string Title {
            get {
                return title;
            }

            set {
                title = value;
                SetPropertyChanged(nameof(Title));
            }
        }

        public int ItemsCount => items.Count;

        public System.ComponentModel.ICollectionView Items { get; set; }
        ObservableCollection<IVisualTreeEntityItem> items;
        public IVisualComponentItem SelectedComponent { get; set; }

        public BaseWPFCommand<IVisualTreeEntityItem> RenderModeSwither {
            get => _renderModeSwither;
            set {
                _renderModeSwither = value;
                SetPropertyChanged(nameof(RenderModeSwither));
            }
        }

        readonly Dictionary<ElementTag, IVisualTreeEntityItem> hash;
        readonly IDockingTabManager dockingManager;
        IRenderUpdater updater;

        public ComponetsViewModel(IDockingTabManager dockingManager) {
            items = new ObservableCollection<IVisualTreeEntityItem>();
            Items = CollectionViewSource.GetDefaultView(items);
            hash = new Dictionary<ElementTag, IVisualTreeEntityItem>();
            this.dockingManager = dockingManager;
            //SelectedComponent = Items.First().Properties.First();
            //OpenShaderEditor = new OpenPropertiesEditorCommand();
            //OpenPropertiesEditor = new OpenPropertiesEditorCommand();
            dockingManager.OpenComponetsTab(this);
        }

        public void SetCurrentRenderUpdater(IRenderUpdater updater) {
            this.updater = updater;
        }

        public void Update() {
            //GameWindow.InputManager.PushCommand(new Std.Engine.Core.Input.Commands.ForceRenderCommand());
        }

        public void Change(GraphicEntityDecorator entity) {
            if (entity.IsDestroyed) {
                if (hash.ContainsKey(entity.Tag)) {
                    items.Remove(hash[entity.Tag]);
                    hash.Remove(entity.Tag);
                }
                return;
            }

            var found = items.SingleOrDefault(x => x.Name == entity.Tag);
            if (found == null) {
                found = new VisualTreeItem(entity, this);
                foreach (var com in entity.GetComponents()) {
                    found.Add(new VisualComponentItem(com, this));
                }
                items.Add(found);
                hash.Add(found.Name, found);
            } else {
                found.Clear();
                var coms = entity.GetComponents();
                foreach (var com in coms) {
                    found.Add(new VisualComponentItem(com, this));//{ RenderModeSwither = _renderModeSwither }
                }
            }
        }

        public void Refresh(IEnumerable<GraphicEntityDecorator> entities) {
            Title = $"Visual Tree [entities {entities.Count()}]";
            foreach (var en in entities) {
                if (hash.ContainsKey(en.Tag)) {
                    var item = hash[en.Tag];
                    var existed = new HashSet<ElementTag>();
                    var coms = en.GetComponents();
                    foreach (var com in coms) {
                        if (com == null) { continue; }
                        if (!item.TryRefresh(com)) {
                            item.Add(new VisualComponentItem(com, this));
                        }
                        existed.Add(com.Tag);
                    }
                    //clear removed
                    foreach (var com in item.Components.ToArray()) {
                        if (!existed.Contains(com.Guid)) {
                            item.Remove(com);
                        }
                    }
                } else {

                }
            }
            SetPropertyChanged(nameof(ItemsCount));
        }

        public void Execute(string code) {
            var console = ConsoleText.Trim();
            ConsoleText = console;
            //var executer = new ScriptExetuter();

            //executer.Execute(new ScriptEnvironment { CurrentWatch = items }, console).ContinueWith(x => {
            //    var text = "";
            //    if (x.Exception != null) {
            //        text = x.Exception.InnerException.Message;
            //    } else {
            //        text = x.Result.ToString();
            //    }
            //    ConsoleText += Environment.NewLine + text;
            //});
        }

        #region IItemActions

        public void Removed(VisualTreeItem item) {
            hash.Remove(item.Name);
            items.Remove(item);
        }

        #endregion

        void IPropertyTabManager.Open(IVisualComponentItem item) {
            var i = new EditingPropertiesComponentItem(item);
            dockingManager.OpenPropertiesTab(i, updater);
        }
    }
}
