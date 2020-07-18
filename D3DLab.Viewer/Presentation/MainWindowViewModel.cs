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
using D3DLab.Viewer.Presentation.TopPanel.SaveAll;

using SharpDX.DXGI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation {
    class GraphicsInfo : BaseNotify {
        private double fps;
        private double milliseconds;
        private string? adapter;

        public string? Adapter { get => adapter; set => Update(ref adapter, value); }
        public double Fps { get => fps; set => Update(ref fps, value); }
        public double Milliseconds { get => milliseconds; set => Update(ref milliseconds, value); }
    }

    class AppOutput : BaseNotify, IAppLoggerSubscriber {
        public ObservableCollection<string> Text { get; private set; }
        const int maxLines = 10;

        public AppOutput() {
            Text = new ObservableCollection<string>();
        }

        public void Debug(string message) {
            Write(message);
        }
        public void Error(Exception exception) {
            Write(exception.Message);
        }
        public void Error(Exception exception, string message) {
            Write(exception.Message);
        }
        public void Error(string message) {
            Write(message);
        }
        public void Info(string message) {
            Write(message);
        }
        public void Warn(string message) {
            Write(message);
        }

        void Write(string message) {
            Application.Current.Dispatcher.InvokeAsync(() => {
                var now = DateTime.Now;
                Text.Insert(0,$"[{now.Hour}:{now.Minute}:{now.Second}] {message}");
                if (Text.Count > maxLines) {
                    Text.RemoveAt(Text.Count-1);
                }
            });
        }
        
    }

    class MainWindowViewModel : BaseNotify, IDropFiles,
        IFileLoader, ISelectedObjectTransformation, ISaveLoadedObject,
        IEntityRenderSubscriber {

        #region selected object cmd

        public ICommand ShowHideSelectedObjectCommand { get; }
        public ICommand RemoveSelectedObjectCommand { get; }
        public ICommand OpenDetailsSelectedObjectCommand { get; }
        public ICommand OpenFolderSelectedObjectCommand { get; }
        public ICommand LockSelectedObjectCommand { get; }
        public ICommand ShowBoundsSelectedObjectCommand { get; }
        public ICommand RefreshSelectedObjectCommand { get; }
        public ICommand FlatshadingSelectedObjectCommand { get; }
        public ICommand WireframeSelectedObjectCommand { get; }

        #endregion

        public ICommand OpenFilesCommand { get; }
        public ICommand OpenDebuggerWindow { get; }
        public ICommand HostLoadedCommand { get; }
        public ICommand SaveAllCommand { get; }
        public ICommand CameraFocusToAllCommand { get; }

        public ICollectionView LoadedObjects { get; }

        public IActionModule Module { get; }
        public GraphicsInfo GraphicsInfo { get; }
        public AppOutput Output { get; }

        readonly ObservableCollection<LoadedObjectItem> loadedObjects;
        readonly ContextStateProcessor context;
        readonly EngineNotificator notificator;
        readonly MainWindow mainWin;
        readonly DebuggerPopup debugger;
        readonly AppSettings settings;
        readonly DialogManager dialogs;
        readonly AppLogger logger;
        WFScene d3dScene;

        public MainWindowViewModel(MainWindow mainWin, DebuggerPopup debugger, 
            AppSettings settings, DialogManager dialogs, AppLogger logger) {
            GraphicsInfo = new GraphicsInfo();
            Output = new AppOutput();

            logger.Subscrube(Output);

            RemoveSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnRemoveSelectedObject);
            ShowHideSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnShowHideSelectedObject);
            OpenDetailsSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnOpenDetailsSelectedObject);
            ShowBoundsSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnShowBoundsSelectedObject);
            OpenFolderSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnOpenFolderSelectedObject);
            RefreshSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnRefreshSelectedObject);
            FlatshadingSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnFlatshadingSelectedObject);
            WireframeSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnWireframeSelectedObject);

            OpenFilesCommand = new WpfActionCommand(OnOpenFilesCommand);

            OpenDebuggerWindow = new WpfActionCommand(OnOpenDebuggerWindow);
            HostLoadedCommand = new WpfActionCommand<FormsHost>(OnHostLoaded);
            SaveAllCommand = new WpfActionCommand(OnSaveAll);
            CameraFocusToAllCommand = new WpfActionCommand(OnCameraFocusToAll);

            loadedObjects = new ObservableCollection<LoadedObjectItem>();
            LoadedObjects = CollectionViewSource.GetDefaultView(loadedObjects);

            notificator = new EngineNotificator();

            notificator.Subscribe(this);

            context = new ContextStateProcessor();
            context.AddState(0, x => GenneralContextState.Full(x,
                new AxisAlignedBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000)),
                notificator, logger));
            context.SwitchTo(0);

            debugger.SetContext(context, notificator);
            this.mainWin = mainWin;
            this.debugger = debugger;
            this.settings = settings;
            this.dialogs = dialogs;
            this.logger = logger;

            // Module = new Modules.Transform.TransformModuleViewModel(this);
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
        void OnRefreshSelectedObject(LoadedObjectItem obj) {
            var loader = new VisualObjectImporter();
            loader.Reload(obj.File, obj.Visual, d3dScene);
        }
        void OnFlatshadingSelectedObject(LoadedObjectItem item) {
            if (item.IsFlatshadingEnabled) {
                item.Visual.TurnFlatshadingOff(context);
            } else {
                item.Visual.TurnFlatshadingOn(context);
            }
        }
        void OnWireframeSelectedObject(LoadedObjectItem item) {
            if (item.IsWireframeEnabled) {
                item.Visual.TurnWireframeOff(context);
            } else {
                item.Visual.TurnWireframeOn(context);
            }
        }


        void OnOpenDebuggerWindow() {
            debugger.Show();
        }
        void OnOpenFilesCommand() {
            dialogs.OpenFiles.Open();
        }
        void OnSaveAll() {
            dialogs.SaveAll.Open();
        }
        void OnCameraFocusToAll() {
            d3dScene.ZoomToAllObjects();
        }

        public void Dropped(string[] files) {
            var loader = new VisualObjectImporter();
            foreach (var file in files) {
                try {
                    var loaded = loader.ImportFromFiles(file, d3dScene);
                    loadedObjects.Add(new LoadedObjectItem(loaded, new FileInfo(file)));
                }catch(Exception ex) {
                    logger.Error(ex);
                }
            }
            settings.SaveRecentFilePaths(files);
            d3dScene.ZoomToAllObjects();
        }

        void IFileLoader.Load(string[] files) {
            try {
                IsBusy = true;
                Dropped(files);
            } finally {
                IsBusy = false;
            }
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


        void IEntityRenderSubscriber.Render(IEnumerable<GraphicEntity> entities) {
            var state = d3dScene.GetPerfomanceState();
            GraphicsInfo.Fps = state.FPS;
            GraphicsInfo.Milliseconds = state.ElapsedMilliseconds;
        }


        IEnumerable<LoadedObjectItem> ISaveLoadedObject.AvaliableToSave => loadedObjects;
        void ISaveLoadedObject.Save(IEnumerable<LoadedObjectItem> items) {
            var exporter = new VisualObjectExporter();
            foreach (var item in items) {
                exporter.Export(item.Visual, item.File, d3dScene);
            }
        }
    }
}
