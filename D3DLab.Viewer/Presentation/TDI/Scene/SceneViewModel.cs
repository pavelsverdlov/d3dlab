using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Context;
using D3DLab.Render;
using D3DLab.Toolkit;
using D3DLab.Toolkit.Components;
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
using D3DLab.ECS.Ext;
using WPFLab.MVVM;
using D3DLab.FileFormats.GeometryFormats;

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



        public void LoadGameObject(IFileGeometry3D geo, FileInfo texture, string fileName) {
            var em = context.GetEntityManager();
            
            var box = BoundingBox.CreateFromVertices(geo.Positions.ToArray());
            var center = box.GetCenter();
            
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#B3B598");

            GraphicEntity en;
            if (geo.TextureCoors.Any()) {
                en = EntityBuilders.BuildTextured(em, geo.Positions.ToList(), geo.Indices.ToList(), 
                        geo.TextureCoors.ToArray(), texture,
                        SharpDX.Direct3D11.CullMode.None);
            } else {
                en = EntityBuilders.BuildColored(em, geo.Positions.ToList(), geo.Indices.ToList(),
                   ToVector4(c), SharpDX.Direct3D11.CullMode.Front);
                en.UpdateComponent(GeometryFlatShadingComponent.Create());
                
            }
            en.UpdateComponent(TransformComponent.Create(Matrix4x4.CreateTranslation(Vector3.Zero - center)));
            GameObject.Add(new SingleGameObject(en.Tag, fileName));
            
            var boxlines = PolylineGameObject.Create(
                Context.GetEntityManager(),
                new ElementTag("poly"),
                Std.Engine.Core.Utilities.GeometryBuilder.BuildBox(
                    new Std.Engine.Core.Utilities.BoundingBox(box.Minimum, box.Maximum)),
                V4Colors.Blue
                );

            Context.GetEntityManager().GetEntity(boxlines.Tag)
                .UpdateComponent(TransformComponent.Create(Matrix4x4.CreateTranslation(Vector3.Zero - center)));

            GameObject.Add(boxlines);
        }


        static Vector4 ToVector4(System.Windows.Media.Color color) {
            color.Clamp();
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
    }
}
