using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using D3DLab.Core.Host;
using D3DLab.Core.Input;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.WinForms;
using SharpDX;
using Application = System.Windows.Application;
using D3DLab.Core.Test;
using System.Linq;
using D3DLab.Core.Context;
using D3DLab.Std.Engine.Core;

//https://habrahabr.ru/post/199378/
namespace D3DLab.Core {

    public class Scene : IDisposable {
        static double total = TimeSpan.FromSeconds(1).TotalMilliseconds;
        static double time = (total / 60);

        public ContextStateProcessor Context { get; private set; }
        private readonly FormsHost host;
        private readonly FrameworkElement overlay;
        private SharpDevice sharpDevice;
        private readonly Stopwatch sw;
        private HelixToolkit.Wpf.SharpDX.EffectsManager effectsManager;
        private readonly IEntityRenderNotify notify;
        readonly D3DLab.Core.Context.Viewport port;

        public Scene(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, IEntityRenderNotify notify) {
            this.host = host;
            this.overlay = overlay;
            host.HandleCreated += OnHandleCreated;
            host.Unloaded += OnUnloaded;
            this.Context = context;
            this.notify = notify;

            sw = new Stopwatch();
            port = new D3DLab.Core.Context.Viewport();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            //CompositionTarget.Rendering -= OnCompositionTargetRendering;
            Dispose();
        }
        private void OnHandleCreated(WinFormsD3DControl obj) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                Init(obj);  
                //                RenderLoop.Run(obj, () => OnCompositionTargetRendering(null,null));
                Task.Run(() => {
                    while (true) {
                        OnCompositionTargetRendering(obj, null);
                    }
                });
                //CompositionTarget.Rendering += OnCompositionTargetRendering;
            }));
        }

        CurrentInputObserver input;

        public void Init(WinFormsD3DControl control) {
            sharpDevice = new HelixToolkit.Wpf.SharpDX.WinForms.SharpDevice(control);
            effectsManager = new HelixToolkit.Wpf.SharpDX.EffectsManager(sharpDevice.Device);

            input = new CurrentInputObserver(Application.Current.MainWindow, new WPFInputPublisher(Application.Current.MainWindow));

            var sysmanager = Context.GetSystemManager();

            sysmanager.CreateSystem<CameraSystem>().ctx = port;
            sysmanager.CreateSystem<UpdateRenderTechniqueSystem>().ctx = port;

            sysmanager.CreateSystem<TargetingSystem>().ctx = port;
            sysmanager.CreateSystem<MovementSystem>().ctx = port;
            sysmanager.CreateSystem<VisualRenderSystem>().ctx = port;

            ViewportBuilder.Build(Context.GetEntityManager());
            CameraBuilder.BuildOrthographicCamera(Context.GetEntityManager());
            LightBuilder.BuildDirectionalLight(Context.GetEntityManager());
        }

        private void OnCompositionTargetRendering(WinFormsD3DControl control, EventArgs e) {
            sw.Restart();

            var inputstapshot = input.GetInputSnapshot();

            var sm = Context.GetSystemManager();
            var em = Context.GetEntityManager();
            
            port.Graphics = new Graphics() {
                SharpDevice = sharpDevice,
                EffectsManager = effectsManager,
            };
            port.World = new World(control, host.ActualWidth, host.ActualHeight);
            port.World.UpdateInputState();

            var inputInfo = Context.GetEntityManager().GetEntities()
               .Single(x => x.GetComponent<ViewportBuilder.PerfomanceComponent>() != null)
               .GetComponent<ViewportBuilder.InputInfoComponent>();

            inputInfo.EventCount = inputstapshot.Events.Count;

            var illuminationSettings = new IlluminationSettings();

            illuminationSettings.Ambient = 1;
            illuminationSettings.Diffuse = 1;
            illuminationSettings.Specular = 1;
            illuminationSettings.Shine = 1;
            illuminationSettings.Light = 1;

            var snapshot = new SceneSnapshot(em, inputstapshot);

            using (var queryForCompletion = new SharpDX.Direct3D11.Query(sharpDevice.Device, new SharpDX.Direct3D11.QueryDescription() { Type = SharpDX.Direct3D11.QueryType.Event, Flags = SharpDX.Direct3D11.QueryFlags.None })) {

                var bgColor = new Color4(0.2f, 0.2f, 0.2f, 1);
                sharpDevice.Apply();
                sharpDevice.Clear(bgColor);

                //  lightRenderContext.ClearLights();
                var variables = port.Graphics.Variables(Techniques.RenderLines);
                variables.LightAmbient.Set(new Color4((float)illuminationSettings.Ambient).ChangeAlpha(1f));
                variables.IllumDiffuse.Set(new Color4((float)illuminationSettings.Diffuse).ChangeAlpha(1f));
                variables.IllumShine.Set((float)illuminationSettings.Shine);
                variables.IllumSpecular.Set(new Color4((float)illuminationSettings.Specular).ChangeAlpha(1f));

                try {
                    foreach (var sys in sm.GetSystems()) {
                        sys.Execute(em, inputstapshot);
                    }
                } catch (Exception ex) {
                    ex.ToString();
                    throw ex;
                }
                sharpDevice.Device.ImmediateContext.End(queryForCompletion);

                int value;
                while (!sharpDevice.Device.ImmediateContext.GetData(queryForCompletion, out value) || (value == 0))
                    Thread.Sleep(1);
            }
            sharpDevice.Present();

            var perfomance = em.GetEntities()
                .Single(x => x.GetComponent<ViewportBuilder.PerfomanceComponent>() != null)
                .GetComponent<ViewportBuilder.PerfomanceComponent>();

            sw.Stop();

            perfomance.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            perfomance.FPS = (int)total / sw.ElapsedMilliseconds;

            notify.NotifyRender(em.GetEntities().ToArray());
        }
        
        public void Dispose() {
            HelixToolkit.Wpf.SharpDX.Disposer.RemoveAndDispose(ref sharpDevice);
            effectsManager.Dispose();
            host.HandleCreated -= OnHandleCreated;
            host.Unloaded -= OnUnloaded;
        }

        
    }
}
