using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.ECS.Shaders;
using D3DLab.Toolkit;
using D3DLab.Toolkit.Host;
using D3DLab.Toolkit.Input.Commands;
using D3DLab.Debugger.D3D;
using D3DLab.Debugger.ECSDebug;
using D3DLab.Debugger.Presentation.TDI.ComponentList;
using D3DLab.Debugger.Presentation.TDI.Scene;
using D3DLab.Debugger.Presentation.TDI.SystemList;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Debugger.Presentation {
    class DebuggerMainWindowViewModel : BaseNotify, IDebuggerMainViewModel, IViewportNotifier, IRenderUpdater, IDropFiles, ITabStateChanged {
        public IDockingTabManager Docking { get; }
        public SystemsViewModel Systems { get; }
        public ComponetsViewModel Componets { get; }

        public ICommand OpenSceneInWindow { get; }
        public ICommand ShowHideOctreeBoxesCommand { get; }
        


        readonly ViewportSubscriber subscriber;
        IContextState context;
        EngineNotificator notificator;
        public DebuggerMainWindowViewModel(IDockingTabManager docking,
            SystemsViewModel systemsVM, ComponetsViewModel componetsVM) {
            OpenSceneInWindow = new WpfActionCommand(OnOpenSceneInWindow);
            ShowHideOctreeBoxesCommand = new WpfActionCommand(OnShowHideOctreeBoxes); 
            Docking = docking;
            Systems = systemsVM;
            Componets = componetsVM;

            subscriber = new ViewportSubscriber(this);

            systemsVM.SetCurrentRenderUpdater(this);
            componetsVM.SetCurrentRenderUpdater(this);
        }

        public void SetContext(IContextState context, EngineNotificator notificator) {
            this.context = context;
            this.notificator = notificator;
            notificator.Subscribe(subscriber);
        }

        void OnShowHideOctreeBoxes() {
            var om = context.GetOctreeManager();
            if (om.IsDrawingBoxesEnable) {
                om.DisableDrawingBoxes();
            } else {
                om.EnableDrawingBoxes();
            }
        }

        void OnOpenSceneInWindow() {
            var ww = new System.Windows.Forms.Form();
            //ww.Handle;
            ww.Load += Ww_Load;
            ww.Show();


            //var win = new Window();
            //win.Owner = Application.Current.MainWindow; 
            //win.Show();
        }

        private void Ww_Load(object sender, EventArgs e) {
            //Scene.SurfaceCreated((WinFormsD3DControl)sender);
        }

        #region IViewportNotifier
        
        public void AddSystem(IGraphicSystem sys) {
            Systems.AddSystem(sys);
        }
        public void GraphicEntityChange(GraphicEntityDecorator entity) {
           // Componets.Change(entity);
        }
        public void FrameRendered(IEnumerable<GraphicEntityDecorator> en) {
          //  Componets.Refresh(en);
            Docking.Update();
        }

        #endregion

        #region IRenderUpdater
        
        public void Update() {
            //Scene.Scene.Surface.InputManager.PushCommand(new ForceRenderCommand());
        }

        #endregion

        public void Dropped(string[] files) {
            //foreach(var file in files) {
            //    var f = new FileInfo(file);
            //    switch (f.Extension) {
            //        case ".obj":
            //            var parser = new FileFormats.GeoFormats._OBJ.Utf8ByteOBJParser();
            //            using (var reader = new FileFormats.MemoryMappedFileReader(f)){
            //                parser.Read(reader.ReadSpan());
            //            }
            //            FileInfo material = null;
            //            try {
            //                material = parser.HasMTL ? new FileInfo(parser.GetMaterialFilePath(f.Directory, f.Directory)) : null;
            //            } catch { }

            //            var builder = new FileFormats.GeoFormats._OBJ.UnitedGroupsBulder(parser.GeometryCache);
                        
            //           // Scene.LoadGameObject(builder.Build(), material, f.Name);

            //            break;
            //    }
            //}
        }

        #region Tab stated changes

        public void Closed(UserControl control) {
            Docking.TabClosed(control);
        }

        #endregion

    }
}
