using D3DLab.Debugger.Infrastructure;
using D3DLab.Debugger.Model;
using D3DLab.Debugger.Presentation.PropertiesEditor;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace D3DLab.Debugger.Windows {

    public sealed class VisualTreeviewerPopup {
        private VisualTreeviewer win;
        public VisualTreeviewerViewModel ViewModel { get; }
        public VisualTreeviewerPopup() {
            win = new VisualTreeviewer();
            ViewModel = (VisualTreeviewerViewModel)win.DataContext;
        }
        public void Show() {
            win.Show();
        }
    }

    public sealed class VisualTreeviewerViewModel : System.ComponentModel.INotifyPropertyChanged {
        public class OpenShaderEditorCommand : BaseWPFCommand<IVisualTreeEntityItem> {
            public override void Execute(IVisualTreeEntityItem item) {
                var shaders = item.Components.Select(x => x.GetOriginComponent()).OfType<IShaderEditingComponent>();
                if (shaders.Any()) {
                    var single = shaders.Single();
                    var win = new ShaderEditorPopup();
                    win.ViewModel.LoadShader(single);
                    win.Show();
                }
            }
        }
        public class OpenPropertiesEditorCommand : BaseWPFCommand<IVisualComponentItem> {
            public override void Execute(IVisualComponentItem item) {
                var win = new PropertiesEditorPopup();
                win.ViewModel.Analyze(item);
                win.Show();
            }
        }


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        string consoleText;

        public string ConsoleText {
            get {
                return consoleText;
            }

            set {
                consoleText = value;
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(ConsoleText)));
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
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Title)));
            }
        }
        public System.ComponentModel.ICollectionView Items { get; set; }
        public ObservableCollection<IVisualTreeEntityItem> items { get; set; }
        public IVisualComponentItem SelectedComponent { get; set; }

        public BaseWPFCommand<IVisualTreeEntityItem> RenderModeSwither {
            get => _renderModeSwither;
            set {
                _renderModeSwither = value;
                PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(RenderModeSwither)));
            }
        }

        private readonly Dictionary<ElementTag, IVisualTreeEntityItem> hash;

        public VisualTreeviewerViewModel() {
            items = new ObservableCollection<IVisualTreeEntityItem>();
            Items = CollectionViewSource.GetDefaultView(items);
            hash = new Dictionary<ElementTag, IVisualTreeEntityItem>();
            //SelectedComponent = Items.First().Properties.First();
            //OpenShaderEditor = new OpenPropertiesEditorCommand();
            //OpenPropertiesEditor = new OpenPropertiesEditorCommand();
        }

        private class VisualTreeItem : IVisualTreeEntityItem {
            public ICommand RenderModeSwither { get; set; }

            public Visibility CanEditShader { get; private set; }
            public ElementTag Name { get { return entity.Tag; } }

            //  public System.ComponentModel.ICollectionView Components { get; set; }
            public ObservableCollection<IVisualComponentItem> Components { get; set; }

            //public ICommand OpenShaderEditor { get; }
            //public ICommand OpenPropertiesEditor { get; }

            private readonly Dictionary<ElementTag, IVisualComponentItem> hash;

            readonly GraphicEntity entity;

            public VisualTreeItem(GraphicEntity entity) {
                this.entity = entity;
                Components = new ObservableCollection<IVisualComponentItem>();
                hash = new Dictionary<ElementTag, IVisualComponentItem>();
                // Components = CollectionViewSource.GetDefaultView(components);
                //OpenShaderEditor = new OpenShaderEditorCommand();
                //OpenPropertiesEditor = new OpenPropertiesEditorCommand();
            }

            public void Add(IVisualComponentItem com) {
                Components.Add(com);
                hash.Add(com.Guid, com);

                CanEditShader = com.GetOriginComponent() is IShaderEditingComponent ? Visibility.Visible : Visibility.Collapsed;
            }
            public void Clear() {
                Components.Clear();
            }
            public void Remove(IVisualComponentItem com) {
                Components.Remove(com);
                hash.Remove(com.Guid);
                CanEditShader = !(com.GetOriginComponent() is IShaderEditingComponent) ? Visibility.Collapsed : Visibility.Visible;
            }

            public bool TryRefresh(IGraphicComponent com) {
                if (!hash.ContainsKey(com.Tag)) {
                    return false;
                }
                hash[com.Tag].Refresh();
                return true;
            }

            //public void Refresh() {
            //    foreach (var i in Components) {

            //        i.Refresh();
            //    }
            //}
        }

        /*
        private static IEnumerable<IVisualTreeEntity> Fill() {
            var props = new[] {
                new VisualProperty{ Name = "name1", Value = "value1" },
                new VisualProperty{ Name = "name2", Value = "value1" },
                new VisualProperty{ Name = "name3", Value = "value1" },
            };
            return new[] {
                new VisualTreeItem {
                    Name = "Header",
                    Components = new ObservableCollection<IEntityComponent>(props)
                },
                  new VisualTreeItem {
                    Name = "Header1",
                    Components = new ObservableCollection<IEntityComponent>(props)
                },
                    new VisualTreeItem {
                    Name = "Header2",
                    Components = new ObservableCollection<IEntityComponent>(props)
                }
            };
        }*/

        public void Add(GraphicEntity entity) {
            var found = items.SingleOrDefault(x => x.Name == entity.Tag);
            if (found == null) {
                found = new VisualTreeItem(entity) { RenderModeSwither = _renderModeSwither  };
                foreach (var com in entity.GetComponents()) {
                    found.Add(new VisualComponentItem(com));
                }
                items.Add(found);
                hash.Add(found.Name, found);
            } else {
                found.Clear();
                foreach (var com in entity.GetComponents()) {
                    found.Add(new VisualComponentItem(com) );//{ RenderModeSwither = _renderModeSwither }
                }
            }
        }

        public void Refresh(IEnumerable<GraphicEntity> entities) {
            Title = $"Visual Tree [entities {entities.Count()}]";
            foreach (var en in entities) {
                if (hash.ContainsKey(en.Tag)) {
                    var item = hash[en.Tag];
                    var existed = new HashSet<ElementTag>();
                    var coms = en.GetComponents();
                    foreach (var com in coms) {
                        if (!item.TryRefresh(com)) {
                            item.Add(new VisualComponentItem(com));
                        }
                        existed.Add(com.Tag);
                    }
                    //clear removed
                    foreach (var com in item.Components.ToArray()) {
                        if (!existed.Contains(com.Guid)) {
                            item.Remove(com);
                        }
                    }
                }
            }
        }

        public void Execute(string code) {
            var console = ConsoleText.Trim();
            ConsoleText = console;
            var executer = new ScriptExetuter();

            executer.Execute(new ScriptEnvironment { CurrentWatch = items }, console).ContinueWith(x => {
                var text = "";
                if (x.Exception != null) {
                    text = x.Exception.InnerException.Message;
                } else {
                    text = x.Result.ToString();
                }
                ConsoleText += Environment.NewLine + text;
            });
        }
    }
}
