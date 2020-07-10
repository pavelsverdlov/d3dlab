using D3DLab.Debugger;
using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.Toolkit;
using D3DLab.Toolkit.Host;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Infrastructure;
using D3DLab.Viewer.Modules;
using D3DLab.Viewer.Presentation.Componets;
using D3DLab.Viewer.Presentation.FileDetails;
using D3DLab.Viewer.Presentation.LoadedPanel;
using D3DLab.Viewer.Presentation.OpenFiles;

using SharpDX.DXGI;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows.Data;
using System.Windows.Input;

using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation {
    class GraphicsInfo : BaseNotify {
        private double fps;
        private string adapter;

        public string Adapter { get => adapter; set => Update(ref adapter, value); }
        public double Fps { get => fps; set => Update(ref fps, value); }
    }
    class MainWindowViewModel : BaseNotify, IDropFiles, IFileLoader, ISelectedObjectTransformation, IEntityRenderSubscriber {

        #region selected object cmd

        public ICommand ShowHideSelectedObjectCommand { get; }
        public ICommand RemoveSelectedObjectCommand { get; }
        public ICommand OpenDetailsSelectedObjectCommand { get; }
        public ICommand OpenFolderSelectedObjectCommand { get; }
        public ICommand LockSelectedObjectCommand { get; }
        public ICommand ShowBoundsSelectedObjectCommand { get; }

        #endregion

        public ICommand OpenFilesCommand { get; }
        public ICommand OpenDebuggerWindow { get; }
        public ICommand HostLoadedCommand { get; }
        public ICollectionView LoadedObjects { get; }

        public IActionModule Module { get; }
        public GraphicsInfo GraphicsInfo { get; }

        readonly ObservableCollection<LoadedObjectItem> loadedObjects;
        readonly ContextStateProcessor context;
        readonly EngineNotificator notificator;
        readonly MainWindow mainWin;
        readonly DebuggerPopup debugger;
        readonly AppSettings settings;
        readonly DialogManager dialogs;
        WFScene d3dScene;

        public MainWindowViewModel(MainWindow mainWin, DebuggerPopup debugger, AppSettings settings, DialogManager dialogs) {
            GraphicsInfo = new GraphicsInfo();

            RemoveSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnRemoveSelectedObject);
            ShowHideSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnShowHideSelectedObject);
            OpenDetailsSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnOpenDetailsSelectedObject);
            ShowBoundsSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnShowBoundsSelectedObject);
            OpenFolderSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnOpenFolderSelectedObject);

            OpenFilesCommand = new WpfActionCommand(OnOpenFilesCommand);

            OpenDebuggerWindow = new WpfActionCommand(OnOpenDebuggerWindow);
            HostLoadedCommand = new WpfActionCommand<FormsHost>(OnHostLoaded);
            loadedObjects = new ObservableCollection<LoadedObjectItem>();
            LoadedObjects = CollectionViewSource.GetDefaultView(loadedObjects);

            notificator = new EngineNotificator();

            notificator.Subscribe(this);

            context = new ContextStateProcessor();
            context.AddState(0, x => GenneralContextState.Full(x,
                new AxisAlignedBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000)),
                notificator));
            context.SwitchTo(0);

            debugger.SetContext(context, notificator);
            this.mainWin = mainWin;
            this.debugger = debugger;
            this.settings = settings;
            this.dialogs = dialogs;

            Module = new Modules.Transform.TransformModuleViewModel(this);
        }



        void OnHostLoaded(FormsHost host) {
            d3dScene = new WFScene(host, host.Overlay, context, notificator);
            d3dScene.Loaded += D3dScene_Loaded;
        }

        private void D3dScene_Loaded() {
            GraphicsInfo.Adapter = d3dScene.GetAdapterDescription().Description;
        }

        public override void OnUnloaded() {
            debugger.Dispose();
            d3dScene.Dispose();

            notificator.Clear();

            context.Dispose();

            base.OnUnloaded();
        }


        void OnShowBoundsSelectedObject(LoadedObjectItem obj) {
            if (obj.IsBoundsShowed) {
                obj.ShowBoundingBox(context);
            } else {
                obj.HideBoundingBox(context);
            }
        }
        void OnShowHideSelectedObject(LoadedObjectItem item) {
            if (item.IsVisible) {
                item.Visual.Show(context.GetEntityManager());
            } else {
                item.Visual.Hide(context.GetEntityManager());
            }
        }
        void OnRemoveSelectedObject(LoadedObjectItem item) {
            item.Visual.Cleanup(context);
            loadedObjects.Remove(item);
        }
        void OnOpenDetailsSelectedObject(LoadedObjectItem item) {
            dialogs.ObjDetails.Open(vm => {
                vm.Fill(item.Visual, context);
            });
        }
        void OnOpenFolderSelectedObject(LoadedObjectItem obj) {
            Process.Start("explorer.exe", $"/select,\"{obj.File.FullName}\"");
        }

        void OnOpenDebuggerWindow() {
            debugger.Show();
        }
        void OnOpenFilesCommand() {
            dialogs.OpenFiles.Open();
        }


        public void Dropped(string[] files) {
            var loader = new VisualObjectLoader();
            foreach (var file in files) {
                var loaded = loader.LoadFromFiles(file, d3dScene);
                loadedObjects.Add(new LoadedObjectItem(loaded, new FileInfo(file)));
            }
            settings.SaveRecentFilePaths(files);
        }

        void IFileLoader.Load(string[] files) {
            Dropped(files);
        }

        void ISelectedObjectTransformation.Transform(Matrix4x4 matrix) {
            //if(LoadedObjects.CurrentItem is LoadedObjectItem item) {
            //    item.Visual.Move(context.GetEntityManager(), matrix);
            //}
            foreach (var i in loadedObjects) {
                i.Transform(context.GetEntityManager(), matrix);
            }
        }
        void ISelectedObjectTransformation.ShowTransformationAxis(Vector3 axis) {
            WorldAxisTypes type = WorldAxisTypes.X;
            if (axis == Vector3.UnitX) {
                type = WorldAxisTypes.X;
            } else if (axis == Vector3.UnitY) {
                type = WorldAxisTypes.Y;
            } else if (axis == Vector3.UnitZ) {
                type = WorldAxisTypes.Z;
            } else {
                return;
            }
            foreach (var i in loadedObjects) {
                i.Visual.ShowWorldAxis(context, type);
            }            
        }
        void ISelectedObjectTransformation.HideTransformationAxis(Vector3 axis) {
            WorldAxisTypes type = WorldAxisTypes.X;
            if (axis == Vector3.UnitX) {
                type = WorldAxisTypes.X;
            } else if (axis == Vector3.UnitY) {
                type = WorldAxisTypes.Y;
            } else if (axis == Vector3.UnitZ) {
                type = WorldAxisTypes.Z;
            } else {
                return;
            }
            foreach (var i in loadedObjects) {
                i.Visual.HideWorldAxis(context, type);
            }
        }
        void ISelectedObjectTransformation.HideAllTransformationAxis() {
            foreach (var i in loadedObjects) {
                i.Visual.HideWorldAxis(context, WorldAxisTypes.All);
            }
        }


        void IEntityRenderSubscriber.Render(System.Collections.Generic.IEnumerable<GraphicEntity> entities) {
            GraphicsInfo.Fps = d3dScene.GetPerfomanceState().FPS;
        }
        
    }
}
