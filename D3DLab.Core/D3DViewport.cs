using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System.Drawing;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using D3DLab.Core.D3D._11;
using D3DLab.Core.Host;
using D3DLab.Core.Render;
using D3DLab.Core.Visual3D;
using HelixToolkit.Wpf;
using SharpDX.Windows;
using Camera = D3DLab.Core.Render.Camera;
using Control = System.Windows.Forms.Control;
using D3D11 = SharpDX.Direct3D11;
using Matrix = SharpDX.Matrix;

namespace D3DLab.Core {
  
    public sealed class D3DViewport {
        public List<Visual3DElement> Children { get; private set; }

        private readonly Camera camera;
        private readonly IRenderer renderer;
        private readonly System.Windows.Forms.Control renderForm;

        public D3DViewport() {
            Children = new List<Visual3DElement>();
            var window = new RenderForm("My first SharpDX game");
            window.ClientSize = new System.Drawing.Size(1280, 720);
            window.AllowUserResizing = false;
            window.Text = "D3DLab";
            window.Icon = null;
            window.AllowUserResizing = true;

            renderer = new D3D11Renderer(window.Handle);
            renderer.Attach();

            renderForm = window;
        }
        /// <summary>
        /// for WPF
        /// </summary>
        public D3DViewport(System.Windows.Forms.Control renderForm) {
            Children = new List<Visual3DElement>();
            camera = new Camera();
            camera.Position = new Vector3(0, 0, 10.0f);
            camera.Target = Vector3.Zero;
            renderer = new D3D11Renderer(renderForm.Handle);
            renderer.Attach();
            this.renderForm = renderForm;

            renderForm.Click += RenderForm_Click;
        }

        private void RenderForm_Click(object sender, EventArgs e) {
        }

        public void Run() {
            //winforms
            //            RenderLoop.Run(renderForm, OnRenderCallback);
            System.Windows.Media.CompositionTarget.Rendering += OnRendering;


            HelixToolkit.Wpf.ObjReader reader = new ObjReader();
            var res = reader.Read(@"C:\Storage\trash\MA\implant_bar_-45-46-47-bar_cad_2016-10-18_17-44-44.NestingEnqueue.obj");

            var mapper = new Dictionary<string,List<HelixToolkit.Wpf.MeshBuilder>>();

            foreach (var gr in reader.Groups) {
                var names = gr.Name.Split(new [] { ' '}, StringSplitOptions.RemoveEmptyEntries);
                var key = names[0];
                List<MeshBuilder> list;
                if (!mapper.TryGetValue(key, out list)) {
                    list = new List<MeshBuilder>();
                    mapper.Add(key, list);
                }
                list.Add(gr.MeshBuilder);
            }


//            var visual = new Visual3DElement();
//            visual.Geometry = new GeometryModel(new[] {
//                                            new Vector3(-0.5f, 0.5f, 0.0f),
//                                            new Vector3(0.5f, 0.5f, 0.0f),
//                                            new Vector3(0.0f, -0.5f, 0.0f),
//
//                                            new Vector3(-0.2f, 0.2f, 0.0f),
//                                            new Vector3(0.2f, 0.2f, 0.0f),
//                                            new Vector3(0.0f, -0.2f, 0.0f)
//                                        }, new[]{
//                                            0,1,2,
//                                            3,4,5
//                                        }, new Color4[] {
//                                             SharpDX.Color.Red,
//                                             SharpDX.Color.Green,
//                                             SharpDX.Color.Blue,
//
//                                             SharpDX.Color.Green,
//                                             SharpDX.Color.Green,
//                                             SharpDX.Color.Green
//                                        });
//            visual.Rotation = Vector3.Zero;
//            Children.Add(visual);

            foreach (var item in mapper) {
                var visual = new Visual3DElement();
                var mb = new HelixToolkit.Wpf.MeshBuilder(false,false);
                foreach (var builder in item.Value) {
                    mb.Append(builder);
                }
                var mesh = mb.ToMesh();

                var center = GetCenter(mesh.Bounds);
                var tr = new TranslateTransform3D(-new Vector3D(center.X, center.Y, center.Z));

                for (int i = 0; i < mesh.Positions.Count; i++) {
                    mesh.Positions[i] = tr.Transform(mesh.Positions[i]);
                }

                Color4[] colors= new Color4[mesh.TriangleIndices.Count];
                for (int i = 0; i < mesh.TriangleIndices.Count; i++) {
                    colors[i] = SharpDX.Color.Green;
                }
                
                visual.Geometry = new GeometryModel(mesh.Positions.Select(x=>new Vector3((float)x.X,(float)x.Y,(float)x.Z)).ToArray(), mesh.TriangleIndices.ToArray(), colors);
                visual.Rotation = Vector3.Zero;
                Children.Add(visual);
            }
        }

        public static Vector3 GetCenter(BoundingBox bounds) {
            var centerX = (bounds.Minimum.X + bounds.Maximum.X) / 2;
            var centerY = (bounds.Minimum.Y + bounds.Maximum.Y) / 2;
            var centerZ = (bounds.Minimum.Z + bounds.Maximum.Z) / 2;
            return new Vector3(centerX, centerY, centerZ);
        }
        public static Vector3 GetCenter(System.Windows.Media.Media3D.Rect3D bounds) {
            var centerX = bounds.X + bounds.SizeX / 2;
            var centerY = bounds.Y + bounds.SizeY / 2;
            var centerZ = bounds.Z + bounds.SizeZ / 2;
            return new Vector3((float)centerX, (float)centerY, (float)centerZ);
        }

        private void OnRendering(object sender, EventArgs e) {
            float Width = 1280;
            float Height = 720;
            // To understand this part, please read the prerequisites resources
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix.PerspectiveFovRH(0.78f, Width / Height, 0.01f, 1.0f);


            var children = Children.ToList();
            foreach (var element in children) {
            //    element.Rotation = new Vector3(element.Rotation.X + 0.01f, element.Rotation.Y + 0.01f, element.Rotation.Z);


                // Beware to apply rotation before translation 
//                var worldMatrix = Matrix.RotationYawPitchRoll(element.Rotation.Y, element.Rotation.X, element.Rotation.Z) *
//                                  Matrix.Translation(element.Position);
//
//                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                element.Render(renderer);
            }
        }

        public void Stop() {
            System.Windows.Media.CompositionTarget.Rendering -= OnRendering;
            renderForm.Dispose();
            renderer.Dispose();
        }
    }


    public class Game : IDisposable {
        private System.Windows.Forms.Control renderForm;

        int Width = 1280;
        int Height = 720;

        private D3D11.Device d3dDevice;
        private D3D11.DeviceContext d3dDeviceContext;
        private SwapChain swapChain;
        private D3D11.RenderTargetView renderTargetView;
        private Viewport viewport;

        // Shaders
        private D3D11Renderer.D3D11ShaderCompiler shader;
        private D3D11.VertexShader vertexShader;
        private D3D11.PixelShader pixelShader;
        private ShaderSignature inputSignature;
        private D3D11.InputLayout inputLayout;

        private D3D11.InputElement[] inputElements = new D3D11.InputElement[] {
            new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, D3D11.InputClassification.PerVertexData, 0),
            new D3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, D3D11.InputClassification.PerVertexData, 0)
        };

        // Triangle vertices
        //        public static VertexPositionColor[] vertices = new VertexPositionColor[] {
        //            new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.0f), SharpDX.Color.Red),
        //            new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.0f), SharpDX.Color.Green),
        //            new VertexPositionColor(new Vector3(0.0f, -0.5f, 0.0f), SharpDX.Color.Blue)
        //
        //        };
        private D3D11.Buffer triangleVertexBuffer;

        /// <summary>
        /// Create and initialize a new game.
        /// </summary>
        public Game() {

            // Set window properties
            var window = new RenderForm("My first SharpDX game");
            window.ClientSize = new System.Drawing.Size(Width, Height);
            window.AllowUserResizing = false;
            window.Text = "D3DLab";
            window.Icon = null;
            window.AllowUserResizing = true;

            renderForm = window;

            shader = new D3D11Renderer.D3D11ShaderCompiler();

            InitializeDeviceResources();
            InitializeShaders();
            InitializeTriangle();

        }
        public Game(System.Windows.Forms.Control control) {
            renderForm = control;
            InitializeDeviceResources();
            InitializeShaders();
            // InitializeTriangle();
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        public void Run() {
            // Start the render loop
            //RenderLoop.Run(renderForm, RenderCallback);
            System.Windows.Media.CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object sender, EventArgs e) {
            var visual = new Visual3DElement();
            visual.Geometry = new GeometryModel(new[] {
                                            new Vector3(-0.5f, 0.5f, 0.0f),
                                            new Vector3(0.5f, 0.5f, 0.0f),
                                            new Vector3(0.0f, -0.5f, 0.0f)
                                        }, new int[0], new Color4[] {
                                             SharpDX.Color.Red,
                                            SharpDX.Color.Green,
                                             SharpDX.Color.Blue
                                        });

            Draw(visual.Geometry.GetRenderData());
        }

        //        public void RenderCallback() {
        //            Draw();
        //        }

        private void InitializeDeviceResources() {
            ModeDescription backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            // Descriptor for the swap chain
            SwapChainDescription swapChainDesc = new SwapChainDescription() {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };

            // Create device and swap chain
            D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;

            viewport = new Viewport(0, 0, Width, Height);
            d3dDeviceContext.Rasterizer.SetViewport(viewport);

            // Create render target view for back buffer
            using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0)) {
                renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);
            }
        }

        private void InitializeShaders() {
            // Compile the vertex shader code
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug)) {
                // Read input signature from shader code
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                vertexShader = new D3D11.VertexShader(d3dDevice, vertexShaderByteCode);
            }

            // Compile the pixel shader code
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug)) {
                pixelShader = new D3D11.PixelShader(d3dDevice, pixelShaderByteCode);
            }

            // Set as current vertex and pixel shaders
            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);

            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            // Create the input layout from the input signature and the input elements
            inputLayout = new D3D11.InputLayout(d3dDevice, inputSignature, inputElements);

            // Set input layout to use
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        private void InitializeTriangle() {

        }

        /// <summary>
        /// Draw the game.
        /// </summary>
        public void Draw(VertexPositionColor[] vertices) {
            // Create a vertex buffer, and use our array with vertices as data
            triangleVertexBuffer = D3D11.Buffer.Create(d3dDevice, D3D11.BindFlags.VertexBuffer, vertices);
            // Set render targets
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);

            // Clear the screen
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));

            // Set vertex buffer
            d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<VertexPositionColor>(), 0));

            // Draw the triangle
            d3dDeviceContext.Draw(vertices.Count(), 0);

            // Swap front and back buffer
            swapChain.Present(1, PresentFlags.None);


        }

        public void Dispose() {
            inputLayout.Dispose();
            inputSignature.Dispose();
            triangleVertexBuffer.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();
        }
    }
}
