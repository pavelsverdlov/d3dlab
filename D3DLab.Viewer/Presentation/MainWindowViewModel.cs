using D3DLab.Debugger;
using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.Toolkit;
using D3DLab.Toolkit.Host;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Presentation.FileDetails;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Windows.Data;
using System.Windows.Input;

using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation {
    class ObjectItem {
        public string Name => Visual.Description;
        public bool IsVisible { get; set; }

        public readonly LoadedVisualObject Visual;
        public ObjectItem(LoadedVisualObject loaded) {
            this.Visual = loaded;
            IsVisible = true;
        }
    }
    class MainWindowViewModel : BaseNotify, IDropFiles {

        #region selected object cmd

        public ICommand ShowHideSelectedObjectCommand { get; }
        public ICommand RemoveSelectedObjectCommand { get; }
        public ICommand OpenDetailsSelectedObjectCommand { get; }
        public ICommand OpenFolderSelectedObjectCommand { get; }
        public ICommand LockSelectedObjectCommand { get; }

        #endregion


        public ICommand OpenDebuggerWindow { get; }

        public ICommand HostLoadedCommand { get; }
        public ICollectionView LoadedObjects { get; }

        readonly ObservableCollection<ObjectItem> loadedObjects;
        readonly ContextStateProcessor context;
        readonly EngineNotificator notificator;
        readonly DebuggerPopup debugger;
        WFScene scene;

        public MainWindowViewModel(DebuggerPopup debugger) {
            RemoveSelectedObjectCommand = new WpfActionCommand<ObjectItem>(OnRemoveSelectedObject);
            ShowHideSelectedObjectCommand = new WpfActionCommand<ObjectItem>(OnShowHideSelectedObject);
            OpenDetailsSelectedObjectCommand = new WpfActionCommand<ObjectItem>(OnOpenDetailsSelectedObject);

            OpenDebuggerWindow = new WpfActionCommand(OnOpenDebuggerWindow);
            HostLoadedCommand = new WpfActionCommand<FormsHost>(OnHostLoaded);
            loadedObjects = new ObservableCollection<ObjectItem>();
            LoadedObjects = CollectionViewSource.GetDefaultView(loadedObjects);

            notificator = new EngineNotificator();

            context = new ContextStateProcessor();
            context.AddState(0, x => GenneralContextState.Full(x,
                new AxisAlignedBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000)),
                notificator));
            context.SwitchTo(0);

            debugger.SetContext(context, notificator);
            this.debugger = debugger;
        }

        void OnShowHideSelectedObject(ObjectItem item) {
            if (item.IsVisible) {
                item.Visual.Show(context.GetEntityManager());
            } else {
                item.Visual.Hide(context.GetEntityManager());
            }
        }

        void OnRemoveSelectedObject(ObjectItem item) {
            item.Visual.Cleanup(context.GetEntityManager());
            loadedObjects.Remove(item);
        }
        void OnOpenDetailsSelectedObject(ObjectItem item) {
            ObjDetailsPopup.Open(item.Visual, context);
        }

        void OnOpenDebuggerWindow() {
            debugger.Show();
        }

        void OnHostLoaded(FormsHost host) {
            scene = new WFScene(host, host.Overlay, context, notificator);
        }

        

        public void Dropped(string[] files) {
            var loader = new VisualObjectLoader();
            foreach (var loaded in loader.LoadFromFiles(files, scene)) {
                loadedObjects.Add(new ObjectItem(loaded));
            }
        }

    }
}
