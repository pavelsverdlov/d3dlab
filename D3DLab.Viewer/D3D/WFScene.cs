using D3DLab.ECS;
using D3DLab.ECS.Components;
using D3DLab.ECS.Context;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Input;
using D3DLab.ECS.Systems;
using D3DLab.Render;
using D3DLab.Std.Engine.Core.Common;
using D3DLab.Std.Engine.Core.Utilities;
using D3DLab.Toolkit.D3Objects;
using System.Numerics;
using System.Windows;

namespace D3DLab.Viewer.D3D {
    public class WFScene : D3DScene {
        BaseInputPublisher publisher;
        CurrentInputObserver input;
        bool isHandleCreated;

        public WFScene(ContextStateProcessor context, EngineNotificator notificator) : base(context, notificator) {
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            Dispose();
        }

        public void Init(System.Windows.Forms.Control control) {//object? sender, EventArgs e
            //publisher = new WPFInputPublisher(overlay); 
            publisher = new WinFormInputPublisher(control);
            input = new CurrentInputObserver(publisher);
            Window = new WFSurface(control, null, input);
            engine = new RenderEngine(Window, Context, notify);
            engine.Run(notify);

            isHandleCreated = true;

            InitScene();
        }

        public void InitContext() {
            var em = Context.GetEntityManager();

            var cameraTag = new ElementTag("CameraEntity");
            {//entities ordering 
                Context.EntityOrder
                       .RegisterOrder<RenderSystem>(cameraTag, 0)
                       .RegisterOrder<DefaultInputSystem>(cameraTag, 0);
            }

            CameraObject.CreatePerspective<RenderSystem, CustomRenderProperties>(Context);
            LightObject.CreateAmbientLight(em, 0.4f);
            //  LightGameObject.CreatePointLight(em, Vector3.Zero + Vector3.UnitZ * 1000);
            LightObject.CreateDirectionLight(em, new Vector3(1, 4, 4).Normalized(), 0.6f);

            //var geo = GeometryBuilder.BuildGeoBox(new BoundingBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10)));


            //var reader = new ObjSpanReader();
            //reader.Read(File.OpenRead(@"D:\Zirkonzahn\NQ_Modifier\autodetect\Prepared_to_recover_-UpperJaw-partial_upper.obj"));
            //geo = reader.FullGeometry1;

            //EntityBuilders.BuildColored(em, geo.Positions, geo.Indices, V4Colors.Blue, SharpDX.Direct3D11.CullMode.Front)
            //    .UpdateComponent(TransformComponent.Create(Matrix4x4.CreateTranslation(new Vector3(-10, -10, -10))));

            //EntityBuilders.BuildColored(em, geo.Positions, geo.Indices, V4Colors.Red, SharpDX.Direct3D11.CullMode.Front)
            //    .UpdateComponent(D3DLab.Toolkit.Components.GeometryFlatShadingComponent.Create());//.UpdateAlfa(0.3f)

            //var en = EntityBuilders.BuildColored(em, geo.Positions, geo.Indices, V4Colors.Green, SharpDX.Direct3D11.CullMode.Front);
            //en.UpdateComponent(TransformComponent.Create(Matrix4x4.CreateTranslation(new Vector3(10, 10, 10))));
            //en.UpdateComponent(D3DLab.Toolkit.Components.WireframeGeometryComponent.Create());
        }

        public override void Dispose() {
            //host.Loaded -= OnHandleCreated;
            // host.Unloaded -= OnUnloaded;

            engine.Dispose();
            publisher.Dispose();
            input?.Dispose();

            base.Dispose();
        }

        internal void ReCreate(System.Windows.Forms.Control control) {
            engine.Dispose();
            publisher.Dispose();
            input?.Dispose();

            publisher = new WinFormInputPublisher(control);
            input = new CurrentInputObserver(publisher);
            Window = new WFSurface(control, null, input);
            engine = new RenderEngine(Window, Context, notify);

            foreach (var com in Context.GetComponentManager().GetComponents<IRenderableComponent>()) {
                com.ClearBuffers();
            }

            foreach (var sys in Context.GetSystemManager().GetSystems<RenderSystem>()) {
                sys.Init(engine.Graphics);
            }

            engine.Run(notify);

            //InitScene();
        }
    }
}
