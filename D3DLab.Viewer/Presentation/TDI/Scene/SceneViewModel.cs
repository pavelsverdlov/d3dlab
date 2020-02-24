using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Context;
using D3DLab.FileFormats.GeometryFormats.OBJ;
using D3DLab.Render;
using D3DLab.Toolkit;
using D3DLab.Toolkit.D3Objects;
using D3DLab.Utility.Math3D;
using D3DLab.Viewer.D3D;
using D3DLab.Viewer.Debugger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.TDI.Scene {

    class SceneViewModel : BaseNotify {

        public WFScene Scene { get; private set; }
        public IContextState Context => context;
        public ObservableCollection<SingleGameObject> GameObject { get; }


        ContextStateProcessor context;
        EngineNotificator notificator;
        readonly IDockingTabManager dockingManager;
        FormsHost host;

        public SceneViewModel(IDockingTabManager dockingManager) {
            this.dockingManager = dockingManager;
            GameObject = new ObservableCollection<SingleGameObject>();
            dockingManager.OpenSceneTab(this);
        }

        public void SetContext(ContextStateProcessor context, EngineNotificator notificator) {
            this.context = context;
            this.notificator = notificator;

        }
        public void SetSurfaceHost(FormsHost host) {
            this.host = host;
            host.SurfaceCreated += SurfaceCreated;
        }


        public void SurfaceCreated(System.Windows.Forms.Control obj) {
            if (Scene != null) {
                Scene.ReCreate(obj);
                return;
            }
            
            Scene =  new WFScene(context, notificator);
            Scene.Init(obj);
            Scene.InitContext();
        }



        public void LoadGameObject(GroupGeometry3D geo, string fileName) {
            var em = context.GetEntityManager();

            var box = BoundingBox.CreateFromVertices(geo.Positions.ToArray());
            var center = box.GetCenter();

            var en = EntityBuilders.BuildColored(em, geo.Positions.ToList(), geo.Indices.ToList(), V4Colors.Blue, SharpDX.Direct3D11.CullMode.Front);
            en.UpdateComponent(TransformComponent.Create(Matrix4x4.CreateTranslation(Vector3.Zero - center)));

            GameObject.Add(new SingleGameObject(en.Tag, fileName));
        }

    }
}
