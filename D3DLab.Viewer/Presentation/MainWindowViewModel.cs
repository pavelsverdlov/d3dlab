using D3DLab.ECS;
using D3DLab.ECS.Context;
using D3DLab.ECS.Shaders;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Debugger;
using D3DLab.Viewer.Presentation.TDI.ComponentList;
using D3DLab.Viewer.Presentation.TDI.Scene;
using D3DLab.Viewer.Presentation.TDI.SystemList;
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

namespace D3DLab.Viewer.Presentation {
    class MainWindowViewModel : BaseNotify, IViewportNotifier, IRenderUpdater, IDropFiles, ITabStateChanged {
        public IDockingTabManager Docking { get; }
        public SystemsViewModel Systems { get; }
        public SceneViewModel Scene { get; }
        public ComponetsViewModel Componets { get; }

        public ICommand OpenSceneInWindow { get; }
        public ICommand CameraFocusToAll { get; }
        


        readonly ViewportSubscriber subscriber;
        
        public MainWindowViewModel(IDockingTabManager docking,
            SystemsViewModel systemsVM, SceneViewModel sceneVM, ComponetsViewModel componetsVM) {
            OpenSceneInWindow = new WpfActionCommand(OnOpenSceneInWindow);
            CameraFocusToAll = new WpfActionCommand(OnCameraFocusToAll); 
            Docking = docking;
            Systems = systemsVM;
            Scene = sceneVM;
            Componets = componetsVM;

            subscriber = new ViewportSubscriber(this);

            var notificator = new EngineNotificator();
            notificator.Subscribe(subscriber);

            var context = new ContextStateProcessor();
            context.AddState(0, x => new GenneralContextState(x, notificator));
            context.SwitchTo(0);

            Scene.SetContext(context, notificator);

            systemsVM.SetCurrentRenderUpdater(this);
            componetsVM.SetCurrentRenderUpdater(this);
        }

        void OnCameraFocusToAll() {
            var file = @"D:\Storage_D\trash\_Hubby\2020-02-21_00004-001_-25-Crown_cad_57f03e2c.NestingEnqueue.obj";

            Dropped(new[] { file });
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
            Scene.SurfaceCreated((System.Windows.Forms.Form)sender);

           
        }

        #region IViewportNotifier
        
        public void AddSystem(IGraphicSystem sys) {
            Systems.AddSystem(sys);
        }
        public void GraphicEntityChange(GraphicEntityDecorator entity) {
            Componets.Change(entity);
        }
        public void FrameRendered(IEnumerable<GraphicEntityDecorator> en) {
            Componets.Refresh(en);
            Docking.Update();
        }

        #endregion

        #region IRenderUpdater
        
        public void Update() {
            Scene.Scene.Window.InputManager.PushCommand(new Std.Engine.Core.Input.Commands.ForceRenderCommand());
        }

        #endregion


        public void Dropped(string[] files) {
            foreach(var file in files) {
                var f = new FileInfo(file);
                switch (f.Extension) {
                    case ".obj":
                        var reader = new FileFormats.GeometryFormats._OBJ.ObjReader();
                        reader.Read(f);

                        Scene.LoadGameObject(reader.FullGeometry, reader.MaterialFilePath, f.Name);

                        break;
                }
            }
        }

        #region Tab stated changes

        public void Closed(UserControl control) {
            Docking.TabClosed(control);
        }

        #endregion

    }
}
