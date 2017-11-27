using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using D3DLab.Core.Components;
using D3DLab.Core.Components.Behaviors;
using D3DLab.Core.Components.Render;
using D3DLab.Core.Entities;
using D3DLab.Core.Host;
using D3DLab.Core.Input;
using D3DLab.Core.Render;
using D3DLab.Core.Render.Components;
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

    public interface I3DobjectLoader {
        void LoadObj(Stream content);
    }

    public class D3DEngine : ID3DEngine, IDisposable, I3DobjectLoader {
        private readonly Context context;
        private readonly FormsHost host;
        private SharpDevice sharpDevice;
        private HelixToolkit.Wpf.SharpDX.EffectsManager effectsManager;

        public ViewportNotificator Notificator { get; private set; }

        public D3DEngine(FormsHost host) {
            this.host = host;
            host.HandleCreated += OnHandleCreated;
            host.Unloaded += OnUnloaded;
            Notificator = new ViewportNotificator();

            context = new Context(this);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            CompositionTarget.Rendering -= OnCompositionTargetRendering;
            Dispose();
        }
        private void OnHandleCreated(WinFormsD3DControl obj) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                Init(obj);
                //                RenderLoop.Run(obj, () => OnCompositionTargetRendering(null,null));
                Task.Run(() => {
                    var total = TimeSpan.FromSeconds(1).TotalMilliseconds;
                    var time = (total / 60);
                    var sw = new Stopwatch();
                    while (true) {
                        sw.Restart();
                        OnCompositionTargetRendering(null, null);
                        sw.Stop();
                        // Debug.WriteLine("{0} FPS", (int)total / sw.ElapsedMilliseconds);
                        //                        sw.Stop();
                        //                        if (sw.ElapsedMilliseconds < time) {
                        //                            Thread.Sleep((int) (time- sw.ElapsedMilliseconds));
                        //                        }
                        //Application.DoEvents();
                    }
                });
                //CompositionTarget.Rendering += OnCompositionTargetRendering;
            }));
        }

        public void Init(WinFormsD3DControl control) {
            sharpDevice = new HelixToolkit.Wpf.SharpDX.WinForms.SharpDevice(control);
            effectsManager = new HelixToolkit.Wpf.SharpDX.EffectsManager(sharpDevice.Device);
            
            context.AddSystem(new CameraInputSystem(control));
            context.CreateSystem<UpdateRenderTechniqueSystem>();
            context.CreateSystem<VisualRenderSystem>();

            ViewportBuilder.Build(context);
            CameraBuilder.BuildOrthographicCamera(context);
            LightBuilder.BuildDirectionalLight(context);
            //VisualModelBuilder.Build(context);
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e) {
            context.Graphics = new Graphics() {
                SharpDevice = sharpDevice,
                EffectsManager = effectsManager,
            };
            context.World = new World();

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
                }
                sharpDevice.Device.ImmediateContext.End(queryForCompletion);

                int value;
                while (!sharpDevice.Device.ImmediateContext.GetData(queryForCompletion, out value) || (value == 0))
                    Thread.Sleep(1);
            }
            sharpDevice.Present();

            Notificator.NotifyRender();
        }

        private void RenderTest(WinFormsD3DControl form) {
            // В этот список мы будем добавлять все созданные по ходу дела ресурсы
            // дабы не забыть их освободить
            var unmanagedResources = new List<IDisposable>();

            // Создадим окно. В классе RenderForm, описанном в SharpDX, уже реализован MessagePump,
            // так что это избавит нас от лишней рутины и позволит сконцентрироваться на главном.
            // var form = new RenderForm("Example");

            // Опишем параметры SwapChain - набора буферов для отображения результирующей картинки
            var desc = new SwapChainDescription() {
                BufferCount = 1,
                ModeDescription = new ModeDescription(
                                   form.ClientSize.Width,
                                   form.ClientSize.Height,
                                   new Rational(0, 1),
                                   Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Инициализируем устройство и SwapChain
            // И получим контекст устройства
            SharpDX.Direct3D11.Device device;
            SwapChain swapChain;

            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.None,
                new[] { FeatureLevel.Level_10_0 },
                desc,
                out device,
                out swapChain);

            var context = device.ImmediateContext;

            unmanagedResources.Add(device);
            unmanagedResources.Add(swapChain);
            unmanagedResources.Add(context);

            // Игнорируем все события окна
            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);
            unmanagedResources.Add(factory);

            // Получаем буфер для рендеринга итоговой картинки
            var backBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0);
            unmanagedResources.Add(backBuffer);

            // Устанавливаем этот буфер как наш выход для рендера
            var renderView = new RenderTargetView(device, backBuffer);
            unmanagedResources.Add(renderView);

            // Скомпилируем эффект из файла в байткод
            ShaderBytecode bytecode = ShaderBytecode.CompileFromFile("shader.fx", "fx_5_0");
            unmanagedResources.Add(bytecode);

            // Загрузим скомпилированный эффект в видеокарту
            var renderEffect = new Effect(device, bytecode);
            unmanagedResources.Add(renderEffect);

            // Выберем технику рендера (у нас она одна)
            // Мы можем описать в файле эффекта несколько техник,
            // например для разного уровня аппаратной поддержки
            var renderTechnique = renderEffect.GetTechniqueByName("SimpleRedRender");
            unmanagedResources.Add(renderTechnique);

            // Выберем входную сигнатуру данных первого прохода
            var renderPassSignature = renderTechnique.GetPassByIndex(0).Description.Signature;
            unmanagedResources.Add(renderPassSignature);

            // Определяем формат вершинного буфера для входной сигнатуры первого прохода
            var inputLayout = new InputLayout(
                device,
                renderPassSignature,
                new[]
                    {
                        // Собственно у нас весь буфер состоит из элементов одного типа - 
                        // позиции вершины.
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    });
            unmanagedResources.Add(inputLayout);

            // Создадим вершинный буфер из трех вершин для одного треугольника
            // Я использовал числа меньшие, чем в статье, чтобы влезло на экран
            var vertices = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, new[]
                                  {
                                      new Vector3(0.0f, 0.5f, 0f),
                                      new Vector3(-0.5f, -0.5f, 0f),
                                      new Vector3(0.5f, -0.5f, 0f)
                                  });
            unmanagedResources.Add(vertices);

            // Создадим индексный буфер, описывающий один треугольник
            var indices = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, new uint[] { 0, 2, 1 });
            unmanagedResources.Add(indices);

            // Установим формат вершинного буфера
            context.InputAssembler.InputLayout = inputLayout;

            // Установим тип примитивов в индексом буфере - список треугольников
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            // Установим текущим вершинный буфер
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, 12, 0));

            // Установим текущим индексный буфер с описанием треугольника, и укажем тип данных индекса (32-бит unsigned int)
            context.InputAssembler.SetIndexBuffer(indices, Format.R32_UInt, 0);

            // Устаналиваем вьюпорт (координаты и размер области вывода растеризатора)
            context.Rasterizer.SetViewports(new[] { new ViewportF(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f) });

            // Установим RenderTargetView в который рендерить картинку
            context.OutputMerger.SetTargets(renderView);

            // Цикл, пока не закроем окно
            RenderLoop.Run(form, () => {
                // Очистим RenderTargetView темно-синим цветом
                // Почему не черный - когда черный цвет, не всегда понятно, действительно ли на экран ничего
                // не выводится, или у нас некорректно работает пиксельный шейдер и черные объекты сливаются с фоном.
                // Если же фон не черный - мы всегда увидим, если что-то вообще рендерится.
                context.ClearRenderTargetView(renderView, Colors.DarkSlateBlue.ToColor4());

                // Вызовем команду отрисовки индексированного примитива для каждого прохода,
                // описанного в эффекте (в данном случае он у нас только один)
                for (int i = 0; i < renderTechnique.Description.PassCount; i++) {
                    // Установим текущим описание прохода под номером i нашего эффекта
                    renderTechnique.GetPassByIndex(i).Apply(context);

                    // Отрендерим индексированный примитив, используя установленные ранее буферы и проход
                    // 3 - это количество индексов из буфера, то есть в нашем случае все.
                    context.DrawIndexed(3, 0, 0);
                }

                // Отобразим результат (переключим back и front буферы)
                swapChain.Present(0, PresentFlags.None);
            });

            // Освободим ресурсы
            foreach (var unmanagedResource in unmanagedResources) {
                unmanagedResource.Dispose();
            }
        }

        public void Dispose() {
            HelixToolkit.Wpf.SharpDX.Disposer.RemoveAndDispose(ref sharpDevice);
            effectsManager.Dispose();
            host.HandleCreated -= OnHandleCreated;
            host.Unloaded -= OnUnloaded;
        }

        public void LoadObj(Stream content) {
            HelixToolkit.Wpf.SharpDX.ObjReader readerA = new HelixToolkit.Wpf.SharpDX.ObjReader();
            var res = readerA.Read(content);

            var dic = new Dictionary<string, HelixToolkit.Wpf.SharpDX.MeshBuilder>();
            foreach (var gr in readerA.Groups) {
                var key = gr.Name.Split(' ')[0];
                HelixToolkit.Wpf.SharpDX.MeshBuilder value;
                if (!dic.TryGetValue(key, out value)) {
                    value = new HelixToolkit.Wpf.SharpDX.MeshBuilder(true, false);
                    dic.Add(key, value);
                }
                value.Append(gr.MeshBuilder);
            }

            var index = 0;
            var builder = new MeshBuilder(true, false);
            foreach (var item in dic) {
                builder.Append(item.Value);
            }
            VisualModelBuilder.Build(context, builder.ToMeshGeometry3D(),"duck" + Guid.NewGuid().ToString());

        }
    }
}
