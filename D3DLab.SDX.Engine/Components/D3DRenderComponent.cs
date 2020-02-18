using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.SDX.Engine.Rendering;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;

namespace D3DLab.SDX.Engine.Components {
    public interface ID3DTransformWorldRenderComponent {
        DisposableSetter<SharpDX.Direct3D11.Buffer> TransformWorldBuffer { get; }
    }
    public abstract class D3DRenderComponent : GraphicComponent, IRenderableComponent,
        ID3DTransformWorldRenderComponent {
        public bool CanRender { get; set; }

        /// <summary>
        /// SharpDX.RasterizerState must not be keeped in componet it should created new one each time on frame by descriptor.
        /// That is why component has only RasterizerStateDescription not SharpDX.RasterizerState object
        /// </summary>
        public D3DRasterizerState RasterizerStateDescription { get; set; }
        public PrimitiveTopology PrimitiveTopology { get; set; }

        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> TransformWorldBuffer { get; set; }
        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> VertexBuffer { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> IndexBuffer { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<DepthStencilState> DepthStencilState { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<BlendState> BlendingState { get; private set; }

        #region MOVED TO RenderTechniqueSystem | REMOVE FROM HERE
        
        [IgnoreDebuging]
        public DisposableSetter<InputLayout> Layout { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<VertexShader> VertexShader { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<PixelShader> PixelShader { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<GeometryShader> GeometryShader { get; private set; } 

        #endregion


        protected readonly DisposeObserver disposer;
        public D3DRenderComponent() {
            CanRender = true;
            IsModified = true;
            disposer = new DisposeObserver();
            TransformWorldBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            VertexBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            IndexBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            DepthStencilState = new DisposableSetter<DepthStencilState>(disposer);
            BlendingState = new DisposableSetter<BlendState>(disposer);
            //Layout = new DisposableSetter<InputLayout>(disposer);
            //VertexShader = new DisposableSetter<VertexShader>(disposer);
            //PixelShader = new DisposableSetter<PixelShader>(disposer);
            //GeometryShader = new DisposableSetter<GeometryShader>(disposer);
        }

        public override void Dispose() {
            base.Dispose();
            disposer.Dispose();
            CanRender = false;
            IsModified = false;
        }

        public virtual void ClearBuffers() {
            disposer.DisposeObservables();
            CanRender = true;
            IsModified = true;
        }

        //public void SetStates(BlendState blend, DepthStencilState depth) {
        //    DepthStencilState.Set(depth);
        //    BlendingState.Set(blend);
        //}

    }

}
