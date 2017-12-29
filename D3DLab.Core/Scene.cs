using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using D3DLab.Core.Entities;
using D3DLab.Core.Host;
using D3DLab.Core.Input;
using D3DLab.Core.Render;
using D3DLab.Core.Viewport;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.WinForms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Application = System.Windows.Application;
using Buffer = System.Buffer;
using Resource = SharpDX.DXGI.Resource;
using D3DLab.Core.Test;
using System.IO;
using System.Linq;
using D3DLab.Core.Context;

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
        private readonly IViewportRendeNotify notify;

        public Scene(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, IViewportRendeNotify notify) {
            this.host = host;
            this.overlay = overlay;
            host.HandleCreated += OnHandleCreated;
            host.Unloaded += OnUnloaded;
            this.Context = context;
            this.notify = notify;

            sw = new Stopwatch();
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

            input = new CurrentInputObserver(Application.Current.MainWindow, Context.GetInutManager());

            var sysmanager = Context.GetSystemManager();

            sysmanager.CreateSystem<CameraSystem>();
            sysmanager.CreateSystem<UpdateRenderTechniqueSystem>();

            sysmanager.CreateSystem<TargetingSystem>();
            sysmanager.CreateSystem<MovementSystem>();
            sysmanager.CreateSystem<VisualRenderSystem>();

            ViewportBuilder.Build(Context.GetEntityManager());
            CameraBuilder.BuildOrthographicCamera(Context.GetEntityManager());
            LightBuilder.BuildDirectionalLight(Context.GetEntityManager());
        }

        private void OnCompositionTargetRendering(WinFormsD3DControl control, EventArgs e) {
            sw.Restart();

            var sm = Context.GetSystemManager();
            var im = Context.GetInutManager();
            var em = Context.GetEntityManager();
            var port = new D3DLab.Core.Context.Viewport();

            port.Graphics = new Graphics() {
                SharpDevice = sharpDevice,
                EffectsManager = effectsManager,
            };
            port.World = new World(control, host.ActualWidth, host.ActualHeight);
            port.World.UpdateInputState();

            var inputInfo = Context.GetEntityManager().GetEntities()
               .Single(x => x.GetComponent<ViewportBuilder.PerfomanceComponent>() != null)
               .GetComponent<ViewportBuilder.InputInfoComponent>();

            inputInfo.EventCount = im.Events.Count;

            var illuminationSettings = new IlluminationSettings();

            illuminationSettings.Ambient = 1;
            illuminationSettings.Diffuse = 1;
            illuminationSettings.Specular = 1;
            illuminationSettings.Shine = 1;
            illuminationSettings.Light = 1;

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
                        sys.Execute(em, im, port);
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
