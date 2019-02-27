using D3DLab.SDX.Engine.Rendering;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Std.Engine.Core.Shaders;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D3DLab.SDX.Engine.Components {

    public abstract class D3DRenderComponent : GraphicComponent, IRenderableComponent {
        public bool CanRender { get; set; }

        public D3DRasterizerState RasterizerState { get; set; }
        public PrimitiveTopology PrimitiveTopology { get; set; }

        [IgnoreDebuging]
        [Obsolete("NOT USED YET")]
        public SharpDX.Direct3D11.Buffer TransformWorldBuffer { get; set; }
        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> VertexBuffer { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> IndexBuffer { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<DepthStencilState> DepthStencilState { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<BlendState> BlendingState { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<InputLayout> Layout { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<VertexShader> VertexShader { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<PixelShader> PixelShader { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<GeometryShader> GeometryShader { get; private set; }

        public int VertexSize { get; set; }

        readonly DisposeWatcher disposer;
        public D3DRenderComponent() {
            CanRender = true;
            IsModified = true;
            disposer = new DisposeWatcher();
            VertexBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            IndexBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            DepthStencilState = new DisposableSetter<DepthStencilState>(disposer);
            BlendingState = new DisposableSetter<BlendState>(disposer);
            Layout = new DisposableSetter<InputLayout>(disposer);
            VertexShader = new DisposableSetter<VertexShader>(disposer);
            PixelShader = new DisposableSetter<PixelShader>(disposer);
            GeometryShader = new DisposableSetter<GeometryShader>(disposer);
        }

        public override void Dispose() {
            Disposer.DisposeAll(
                disposer,
                TransformWorldBuffer);

            base.Dispose();
            CanRender = false;
        }


        public void SetStates(BlendState blend, DepthStencilState depth) {
            DepthStencilState.Set(depth);
            BlendingState.Set(blend);
        }

    }

}
