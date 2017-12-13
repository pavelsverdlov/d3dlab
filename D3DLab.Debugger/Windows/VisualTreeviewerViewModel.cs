using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Test;
using System.Windows.Data;
using D3DLab.Core.Common;

namespace D3DLab.Debugger.Windows {
    public interface IVisualEntity {

    }
    public interface IEntityComponent {
        Guid Guid { get; }
        string Name { get; }
        string Value { get; }

        void Refresh();
    }

    public interface IVisualTreeEntity {
        string Name { get; }
        ObservableCollection<IEntityComponent> Components { get; }
        void Add(IEntityComponent com);
        void Remove(IEntityComponent com);
        void Clear();
        bool TryRefresh(ID3DComponent com);
        //void Refresh();
    }

    //public sealed class ScriptComponent : D3DLab.Core.Test.IComponent {
    //    public void Dispose() { }

    //}
    public class VisualProperty : IEntityComponent, System.ComponentModel.INotifyPropertyChanged {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ID3DComponent com;

        public VisualProperty(ID3DComponent com) {
            this.com = com;
        }

        public string Name { get { return com.ToString(); } }

        public Guid Guid { get { return com.Guid; } }

        public string Value { get; set; }

        public ID3DComponent GetPropertyObject() {
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

        string title;
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
        public ObservableCollection<IVisualTreeEntity> items { get; set; }
        public IEntityComponent SelectedComponent { get; set; }

        private readonly Dictionary<string, IVisualTreeEntity> hash;

        public VisualTreeviewerViewModel() {
            items = new ObservableCollection<IVisualTreeEntity>();
            Items = CollectionViewSource.GetDefaultView(items);
            hash = new Dictionary<string, IVisualTreeEntity>();
            //SelectedComponent = Items.First().Properties.First();
        }

        private class VisualTreeItem : IVisualTreeEntity {
            public string Name { get { return entity.Tag; } }

            //  public System.ComponentModel.ICollectionView Components { get; set; }
            public ObservableCollection<IEntityComponent> Components { get; set; }

            private readonly Dictionary<Guid, IEntityComponent> hash;

            readonly Entity entity;

            public VisualTreeItem(Entity entity) {
                this.entity = entity;
                Components = new ObservableCollection<IEntityComponent>();
                hash = new Dictionary<Guid, IEntityComponent>();
                // Components = CollectionViewSource.GetDefaultView(components);
            }

            public void Add(IEntityComponent com) {
                Components.Add(com);
                hash.Add(com.Guid, com);
            }
            public void Clear() {
                Components.Clear();
            }
            public void Remove(IEntityComponent com) {
                Components.Remove(com);
                hash.Remove(com.Guid);
            }

            public bool TryRefresh(ID3DComponent com) {
                if (!hash.ContainsKey(com.Guid)) {
                    return false;
                }
                hash[com.Guid].Refresh();
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

        public void Add(Entity entity) {
            var found = items.SingleOrDefault(x => x.Name == entity.Tag);
            if (found == null) {
                found = new VisualTreeItem(entity);
                foreach (var com in entity.GetComponents()) {
                    found.Add(new VisualProperty(com));
                }
                items.Add(found);
                hash.Add(found.Name, found);
            } else {
                found.Clear();
                foreach (var com in entity.GetComponents()) {
                    found.Add(new VisualProperty(com));
                }
            }
        }

        public void Refresh(IEnumerable<Entity> entities) {
            Title = $"Visual Tree [entities {entities.Count()}]";
            foreach (var en in entities) {
                if (hash.ContainsKey(en.Tag)){
                    var item = hash[en.Tag];
                    var existed = new HashSet<Guid>();
                    var coms = en.GetComponents();
                    foreach (var com in coms) {
                        if (!item.TryRefresh(com)) {
                            item.Add(new VisualProperty(com));
                        }
                        existed.Add(com.Guid);
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
