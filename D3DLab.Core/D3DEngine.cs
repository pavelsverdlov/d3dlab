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

//https://habrahabr.ru/post/199378/
namespace D3DLab.Core {
    public interface IViewportControl {
        SharpDX.Matrix GetViewportTransform();
        int Width { get; }
        int Height { get; }
    }
    public interface ID3DEngine {
        ViewportNotificator Notificator { get; }
    }


   

    public class D3DEngine : ID3DEngine, IDisposable {
        public IEntityManager EntityManager { get { return context; } }

        static double total = TimeSpan.FromSeconds(1).TotalMilliseconds;
        static double time = (total / 60);

        private readonly Context context;
        private readonly FormsHost host;
        private readonly FrameworkElement overlay;
        private SharpDevice sharpDevice;
        private readonly Stopwatch sw;
        private HelixToolkit.Wpf.SharpDX.EffectsManager effectsManager;

        public ViewportNotificator Notificator { get; private set; }

        public D3DEngine(FormsHost host, FrameworkElement overlay) {
            this.host = host;
            this.overlay = overlay;
            host.HandleCreated += OnHandleCreated;
            host.Unloaded += OnUnloaded;
            Notificator = new ViewportNotificator();

            context = new Context(this);

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

            input = new CurrentInputObserver(Application.Current.MainWindow, context);

            context.CreateSystem<CameraSystem>();
            context.CreateSystem<UpdateRenderTechniqueSystem>();

            context.CreateSystem<TargetingSystem>();
            context.CreateSystem<MovementSystem>();
            context.CreateSystem<VisualRenderSystem>();

            ViewportBuilder.Build(context);
            CameraBuilder.BuildOrthographicCamera(context);
            LightBuilder.BuildDirectionalLight(context);
           

        }

        private void OnCompositionTargetRendering(WinFormsD3DControl control, EventArgs e) {
            sw.Restart();

            context.Graphics = new Graphics() {
                SharpDevice = sharpDevice,
                EffectsManager = effectsManager,
            };
            context.World = new World(control, host.ActualWidth, host.ActualHeight);
            context.World.UpdateInputState();

            var inputInfo = context.GetEntities()
               .Single(x => x.GetComponent<ViewportBuilder.PerfomanceComponent>() != null)
               .GetComponent<ViewportBuilder.InputInfoComponent>();

            inputInfo.EventCount = context.Events.Count;

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
                var variables = context.Graphics.Variables(Techniques.RenderLines);
                variables.LightAmbient.Set(new Color4((float)illuminationSettings.Ambient).ChangeAlpha(1f));
                variables.IllumDiffuse.Set(new Color4((float)illuminationSettings.Diffuse).ChangeAlpha(1f));
                variables.IllumShine.Set((float)illuminationSettings.Shine);
                variables.IllumSpecular.Set(new Color4((float)illuminationSettings.Specular).ChangeAlpha(1f));

                try {
                    foreach (var sys in context.GetSystems()) {
                        sys.Execute(context, context);
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

            var perfomance = context.GetEntities()
                .Single(x => x.GetComponent<ViewportBuilder.PerfomanceComponent>() != null)
                .GetComponent<ViewportBuilder.PerfomanceComponent>();

            sw.Stop();

            perfomance.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            perfomance.FPS = (int)total / sw.ElapsedMilliseconds;
           
            Notificator.NotifyRender(context.GetEntities().ToArray());
        }
        
        public void Dispose() {
            HelixToolkit.Wpf.SharpDX.Disposer.RemoveAndDispose(ref sharpDevice);
            effectsManager.Dispose();
            host.HandleCreated -= OnHandleCreated;
            host.Unloaded -= OnUnloaded;
        }

        
    }
}
