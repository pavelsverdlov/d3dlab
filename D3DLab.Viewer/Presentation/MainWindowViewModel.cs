using D3DLab.Debugger;
using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.Plugin;
using D3DLab.Toolkit;
using D3DLab.Toolkit.Host;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Infrastructure;
using D3DLab.Viewer.Modules;
using D3DLab.Viewer.Presentation.Componets;
using D3DLab.Viewer.Presentation.FileDetails;
using D3DLab.Viewer.Presentation.LoadedPanel;
using D3DLab.Viewer.Presentation.OpenFiles;
using D3DLab.Viewer.Presentation.Plugin;
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
using System.Reflection;
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
        IEntityRenderSubscriber, IPluginHandler {



        #region selected object cmd

        public ICommand SelectedObjectSettingsOpenedCommand { get; }
        public ICommand SelectedObjectSettingsClosedCommand { get; }

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
        public ICommand ShowWorldCoordinateSystemCommand { get; }
        public ICommand ManipulatorToolEnabledCommand { get; }

        public ObservableCollection<LoadedObjectItem> LoadedObjects => loadedObjects;

        public IActionModule? Module { get => module; set => Update(ref module, value); }
        public GraphicsInfo GraphicsInfo { get; }
        public AppOutput Output { get; }
        public string WinTitle { get; }

        readonly ObservableCollection<LoadedObjectItem> loadedObjects;
        readonly ContextStateProcessor context;
        readonly EngineNotificator notificator;
        readonly MainWindow mainWin;
        readonly DebuggerPopup debugger;
        readonly AppSettings settings;
        readonly DialogManager dialogs;
        readonly AppLogger logger;
        readonly PluginProxy plugins;

        IActionModule? module;
        WFScene? d3dScene;


        public MainWindowViewModel(MainWindow mainWin, DebuggerPopup debugger,
            AppSettings settings, DialogManager dialogs, AppLogger logger) {

            this.mainWin = mainWin;
            this.debugger = debugger;
            this.settings = settings;
            this.dialogs = dialogs;
            this.logger = logger;

            var ver = FileVersionInfo.GetVersionInfo(this.GetType().Assembly.Location).FileVersion;
            WinTitle = $"D3DLab Viewer {ver}";
            GraphicsInfo = new GraphicsInfo();
            Output = new AppOutput();
            plugins = new PluginProxy(Path.Combine(AppContext.BaseDirectory, "plugins"));

            logger.Subscrube(Output);

            RemoveSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnRemoveSelectedObject);
            ShowHideSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnShowHideSelectedObject);
            OpenDetailsSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnOpenDetailsSelectedObject);
            ShowBoundsSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnShowBoundsSelectedObject);
            OpenFolderSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnOpenFolderSelectedObject);
            RefreshSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnRefreshSelectedObject);
            FlatshadingSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnFlatshadingSelectedObject);
            WireframeSelectedObjectCommand = new WpfActionCommand<LoadedObjectItem>(OnWireframeSelectedObject);
            SelectedObjectSettingsOpenedCommand = new WpfActionCommand<LoadedObjectItem>(OnSelectedObjectSettingsOpened);
            SelectedObjectSettingsClosedCommand = new WpfActionCommand<LoadedObjectItem>(OnSelectedObjectSettingsClosed);

            LockSelectedObjectCommand = null;

            OpenFilesCommand = new WpfActionCommand(OnOpenFilesCommand);

            OpenDebuggerWindow = new WpfActionCommand(OnOpenDebuggerWindow);
            HostLoadedCommand = new WpfActionCommand<FormsHost>(OnHostLoaded);
            SaveAllCommand = new WpfActionCommand(OnSaveAll);
            CameraFocusToAllCommand = new WpfActionCommand(OnCameraFocusToAll);
            ShowWorldCoordinateSystemCommand = new WpfActionCommand<bool>(OnShowWorldCoordinateSystem);
            ManipulatorToolEnabledCommand = new WpfActionCommand<bool>(OnManipulatorToolEnabled);

            loadedObjects = new ObservableCollection<LoadedObjectItem>();

            notificator = new EngineNotificator();

            notificator.Subscribe(this);

            context = new ContextStateProcessor();
            context.AddState(0, x => GenneralContextState.Full(x,
                new AxisAlignedBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000)),
                notificator, logger));
            context.SwitchTo(0);

            debugger.SetContext(context, notificator);

            plugins.Load();
        }



        void OnHostLoaded(FormsHost host) {
            d3dScene = new WFScene(host, host.Overlay, context, notificator);

           // d3dScene = new Tests.StensilTestScene(host, host.Overlay, context, notificator);

            d3dScene.Loaded += D3dScene_Loaded;
        }

        private void D3dScene_Loaded() {
            GraphicsInfo.Adapter = d3dScene.GetAdapterDescription().Description;
        }

        public override void OnUnloaded() {
            foreach (var plugin in plugins.Plugins) {
                plugin.Plugin.CloseAsync().Wait();
            }

            debugger.Dispose();
            d3dScene.Dispose();

            notificator.Clear();

            context.Dispose();

            base.OnUnloaded();
        }


        void OnSelectedObjectSettingsOpened(LoadedObjectItem item) {
            item.ActivateStaticComponents(context);
        }
        void OnSelectedObjectSettingsClosed(LoadedObjectItem item) {
            item.DeactivateStaticComponents();
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
                item.Visual.Show(context);
            } else {
                item.Visual.Hide(context);
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
            obj.Refresh(context);
        }
        void OnFlatshadingSelectedObject(LoadedObjectItem item) {
            if (item.IsFlatshadingEnabled) {
                item.HideFlatshadingMode(context);
            } else {
                item.ShowFlatshadingMode(context);
            }
        }
        void OnWireframeSelectedObject(LoadedObjectItem item) {
            if (item.IsWireframeEnabled) {
                item.ShowWireframe(context);
            } else {
                item.HideWireframe(context);
            }
        }


        void OnOpenDebuggerWindow() {
            //debugger.Show();
            dialogs.Plugins.Open(vm => {
                vm.SetPluginProxy(plugins);
            });
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
        void OnShowWorldCoordinateSystem(bool _checked) {
            if (_checked) {
                d3dScene.CoordinateSystem.Show(context);
            } else {
                d3dScene.CoordinateSystem.Hide(context);
            }
        }
        void OnManipulatorToolEnabled(bool _checked) {
            if (_checked) {
                Module = new Modules.Transform.TransformModuleViewModel(this);
            } else {
                Module = null;
            }
        }

        public void Dropped(string[] files) {
            var loader = new VisualObjectImporter();
            foreach (var file in files) {
                try {
                    var loaded = loader.ImportFromFiles(file, d3dScene);
                    loadedObjects.Add(new LoadedObjectItem(loaded, new FileInfo(file)));
                } catch (Exception ex) {
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
        Vector3 ISelectedObjectTransformation.GetCenter() {
            var any = loadedObjects.FirstOrDefault(x => x.IsVisible);
            return any == null ? Vector3.Zero : any.Visual.GetAllBounds(context).Center;
        }
        void ISelectedObjectTransformation.Finish() {
            Module = null;
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
        void ISaveLoadedObject.SaveAs(IEnumerable<LoadedObjectItem> items) {
            var exporter = new VisualObjectExporter();
            foreach (var item in items) {
                var path = WindowsDefaultDialogs.SaveFileDialog(item.File.Directory, WindowsDefaultDialogs.FileFormats.All);
                if (!string.IsNullOrWhiteSpace(path)) {
                    exporter.Export(item.Visual, new FileInfo(path), d3dScene);
                }
            }
        }

        void IPluginHandler.Handle(LoadedPlugin pl) {
           var objs = new List<IPluginLoadedObjectDetails>();

            foreach(var o in loadedObjects) {
                objs.Add(new Modules.Plugin.PluginLoadedObjectDetails(
                    o.File, 
                    o.Visual.Tags));
            }

            pl.Plugin
                .ExecuteAsync(new Modules.Plugin.PluginContext(new Modules.Plugin.PluginScene(context), mainWin, pl.File.Directory, objs));

            dialogs.Plugins.Close();
        }
    }
}
