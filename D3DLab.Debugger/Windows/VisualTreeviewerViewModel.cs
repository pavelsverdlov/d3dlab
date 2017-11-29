using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Test;
using System.Windows.Data;

namespace D3DLab.Debugger.Windows {
    public interface IVisualEntity {

    }
    public interface IEntityComponent {
        string Name { get; }
        string Value { get; }

        void Refresh();
    }

    public interface IVisualTreeEntity {
        string Name { get; }
        ObservableCollection<IEntityComponent> Components { get; }
        void Add(IEntityComponent com);
        void Clear();
        void Refresh();
    }

    //public sealed class ScriptComponent : D3DLab.Core.Test.IComponent {
    //    public void Dispose() { }

    //}
    public class VisualProperty : IEntityComponent, System.ComponentModel.INotifyPropertyChanged {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private IComponent com;

        public VisualProperty(IComponent com) {
            this.com = com;
        }

        public string Name { get { return com.ToString(); } }

        public string Value { get; set; }

        public IComponent GetPropertyObject() {
            return com;
        }

        public void Refresh() {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Name)));
        }
    }

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

        public System.ComponentModel.ICollectionView Items { get; set; }
        public ObservableCollection<IVisualTreeEntity> items { get; set; }
        public IEntityComponent SelectedComponent { get; set; }

        public VisualTreeviewerViewModel() {
            items = new ObservableCollection<IVisualTreeEntity>();
            Items = CollectionViewSource.GetDefaultView(items);
            //SelectedComponent = Items.First().Properties.First();
        }

        private class VisualTreeItem : IVisualTreeEntity {
            public string Name { get { return entity.Tag; } }

            //  public System.ComponentModel.ICollectionView Components { get; set; }
            public ObservableCollection<IEntityComponent> Components { get; set; }

            readonly Entity entity;

            public VisualTreeItem(Entity entity) {
                this.entity = entity;
                Components = new ObservableCollection<IEntityComponent>();
                // Components = CollectionViewSource.GetDefaultView(components);
            }

            public void Add(IEntityComponent com) {
                Components.Add(com);
            }
            public void Clear() {
                Components.Clear();
            }

            public void Refresh() {
                foreach (var i in Components) {
                    i.Refresh();
                }
            }
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

        public void Add(Entity entity) {
            var found = items.SingleOrDefault(x => x.Name == entity.Tag);
            if (found == null) {
                found = new VisualTreeItem(entity);
                foreach (var com in entity.GetComponents()) {
                    found.Add(new VisualProperty(com));
                }
                items.Add(found);
            } else {
                found.Clear();
                foreach (var com in entity.GetComponents()) {
                    found.Add(new VisualProperty(com));
                }
            }
        }

        public void Refresh() {
            foreach (var item in items) {
                item.Refresh();
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
