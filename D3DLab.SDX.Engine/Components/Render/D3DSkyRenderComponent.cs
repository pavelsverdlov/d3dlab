using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core.Components;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine.Components {
    public class D3DSkyRenderComponent : D3DRenderComponent {
        const string path = @"D3DLab.SDX.Engine.Rendering.Shaders.Custom.sky.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        static D3DSkyRenderComponent() {
            layconst = new VertexLayoutConstructor()
               .AddPositionElementAsVector3();
            //.AddNormalElementAsVector3()
            //.AddTexCoorElementAsVector2();

            var d = new CombinedShadersLoader();
            pass = new D3DShaderTechniquePass(d.Load(path, "SKY_"));
        }

        public D3DSkyRenderComponent() : base() {
            Pass = pass;
            LayoutConstructor = layconst;
            PrimitiveTopology = PrimitiveTopology.TriangleList;
            RasterizerState = new D3DRasterizerState(new RasterizerStateDescription() {
                IsAntialiasedLineEnabled = false,
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = .0f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = false,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = .0f
            });
        }

        internal override SharpDX.Direct3D11.Buffer GetVertexBuffer(GraphicsDevice graphics, IGeometryComponent geo) {
            var vertex = new SkyPoint[geo.Positions.Length];
            for (var i = 0; i < geo.Positions.Length; i++) {
                vertex[i] = new SkyPoint(geo.Positions[i]);
            }
            VertexSize = SkyPoint.Size;
            return graphics.CreateBuffer(BindFlags.VertexBuffer, vertex);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SkyPoint {
            public readonly Vector3 Position;
            public SkyPoint(Vector3 position) {
                Position = position;
            }
            public static readonly int Size = Unsafe.SizeOf<SkyPoint>();
        }
    }

}
