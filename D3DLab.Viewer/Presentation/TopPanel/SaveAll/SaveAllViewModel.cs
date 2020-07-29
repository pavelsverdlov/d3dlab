using D3DLab.Viewer.Infrastructure;
using D3DLab.Viewer.Presentation.LoadedPanel;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.TopPanel.SaveAll {

    interface ISaveLoadedObject {
        IEnumerable<LoadedObjectItem> AvaliableToSave { get; }
        void Save(IEnumerable<LoadedObjectItem> items);
        void SaveAs(IEnumerable<LoadedObjectItem> items);
    }

    class SaveAsData {
        public FileInfo NewPath { get; } 
        public LoadedObjectItem ObjectToSave { get; }

    }

    class ItemToSave : BaseNotify {
        public string Name { get; }
        public bool IsChecked { get => isChecked; set => Update(ref isChecked, value); }

        public readonly LoadedObjectItem Object;
        private bool isChecked;

        public ItemToSave(LoadedObjectItem item) {
            Object = item;
            Name = PathHelper.GetPathWithMiddleSkipping(item.File.FullName);
            IsChecked = true;
        }
    }

    class SaveAllViewModel : BaseNotify {
        readonly ISaveLoadedObject provider;
        readonly DialogManager dialogs;
        readonly DispatcherTimer timer;
        private string fullPathPreview;

        public ObservableCollection<ItemToSave> AllLoadedObjects { get; }

        public string FullPathPreview {
            get => fullPathPreview;
            set => Update(ref fullPathPreview, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand MouseMoveCommand { get; }
        public ICommand CloseCommand { get; }
        

        public SaveAllViewModel(ISaveLoadedObject provider, DialogManager dialogs) {
            this.provider = provider;
            this.dialogs = dialogs;
            MouseMoveCommand = new WpfActionCommand<ItemToSave>(OnMouseMove);
            SaveCommand = new WpfActionCommand(OnSave);
            SaveAsCommand = new WpfActionCommand(OnSaveAs);
            SelectAllCommand = new WpfActionCommand(OnSelectAll);
            CloseCommand = new WpfActionCommand(OnClose);

            AllLoadedObjects = new ObservableCollection<ItemToSave>();
            foreach (var i in provider.AvaliableToSave) {
                AllLoadedObjects.Add(new ItemToSave(i));
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += CleanPreviewPath;
        }

        void OnClose() {
            dialogs.SaveAll.Close();
        }

        void OnSelectAll() {
            foreach (var i in AllLoadedObjects) {
                i.IsChecked = true;
            }
        }
        void OnSaveAs() {
            provider.SaveAs(AllLoadedObjects.Where(x => x.IsChecked).Select(x => x.Object));
        }
        void OnSave() {
            provider.Save(AllLoadedObjects.Where(x => x.IsChecked).Select(x => x.Object));
        }

        private void CleanPreviewPath(object sender, EventArgs e) {
            timer.Stop();
            FullPathPreview = null;
            timer.IsEnabled = false;
        }

        void OnMouseMove(ItemToSave item) {
            // timer.Stop();
            FullPathPreview = item.Object.File.FullName;
            // timer.Start();
            // timer.IsEnabled = true;
        }
    }
}
