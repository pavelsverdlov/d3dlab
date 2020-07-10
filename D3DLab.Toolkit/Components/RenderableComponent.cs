using D3DLab.ECS;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Rendering;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    enum RenderTechniques {
        Undefined,
        TriangleColored,
        TriangleTextured,
        Lines,
        OneFrameFlatWhite,
        Background
    }


    /// <summary>
    /// Component to allow rendering
    /// </summary>
    public struct RenderableComponent : IGraphicComponent {

        #region creators

        public static RenderableComponent AsPoints() => 
            new RenderableComponent(CullMode.None, PrimitiveTopology.PointList, RenderTechniques.Background) {
                Tag = ElementTag.New(),
                IsValid = true,
                HasDepthStencil = true,
                DepthStencilStateDescription = D3DDepthStencilStateDescriptions.DepthEnabled,
                RasterizerStateDescription = new RasterizerStateDescription2() {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsMultisampleEnabled = false,

                    IsFrontCounterClockwise = false,
                    IsScissorEnabled = false,
                    IsAntialiasedLineEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = .0f,
                    SlopeScaledDepthBias = .0f
                },
                HasBlendState = true,
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateDisabled,
            };

        public static RenderableComponent AsBackground()
            => new RenderableComponent(CullMode.None, PrimitiveTopology.TriangleStrip, RenderTechniques.Background) {
                Tag = ElementTag.New(),
                IsValid = true,
                HasDepthStencil = true,
                DepthStencilStateDescription = D3DDepthStencilStateDescriptions.DepthDisabled,
                RasterizerStateDescription = new RasterizerStateDescription2() {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsMultisampleEnabled = false,

                    IsFrontCounterClockwise = false,
                    IsScissorEnabled = false,
                    IsAntialiasedLineEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = .0f,
                    SlopeScaledDepthBias = .0f
                },
                HasBlendState = true,
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled,
            };

        public static RenderableComponent AsFlatWhiteTriangleList()
             => AsTriangleList(CullMode.Front, D3DDepthStencilStateDescriptions.DepthEnabled, RenderTechniques.OneFrameFlatWhite);

        public static RenderableComponent AsTriangleColoredList(CullMode mode, DepthStencilStateDescription depth)
             => AsTriangleList(mode, depth, RenderTechniques.TriangleColored);

        public static RenderableComponent AsTriangleColoredList(CullMode mode)
            => new RenderableComponent(mode, PrimitiveTopology.TriangleList, RenderTechniques.TriangleColored) {
                Tag = ElementTag.New(),
                IsValid = true,
                HasDepthStencil = false,
                HasBlendState = true,
                RasterizerStateDescription = D3DRasterizerStateDescriptions.Default(mode),
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled,
            };

        public static RenderableComponent AsTriangleTexturedList(CullMode mode)
            => AsTriangleList(mode, D3DDepthStencilStateDescriptions.DepthEnabled, RenderTechniques.TriangleTextured);
        public static RenderableComponent AsLineList()
           => AsLineList(D3DRasterizerStateDescriptions.Lines, RenderTechniques.Lines);


        static RenderableComponent AsTriangleList(CullMode mode, DepthStencilStateDescription depth,
            RenderTechniques technique)
            => new RenderableComponent(mode, PrimitiveTopology.TriangleList, technique) {
                Tag = ElementTag.New(),
                IsValid = true,
                HasDepthStencil = true,
                HasBlendState = true,
                DepthStencilStateDescription = depth,
                RasterizerStateDescription = D3DRasterizerStateDescriptions.Default(mode),
                BlendStateDescription = D3DBlendStateDescriptions.BlendStateEnabled,
            };
        static RenderableComponent AsLineList(RasterizerStateDescription2 rast, RenderTechniques technique)
          => new RenderableComponent(CullMode.None, PrimitiveTopology.LineList, technique) {
              Tag = ElementTag.New(),
              IsValid = true,
              HasDepthStencil = true,
              HasBlendState = true,
              RasterizerStateDescription = rast,
              DepthStencilStateDescription = D3DDepthStencilStateDescriptions.DepthEnabled,
              BlendStateDescription = D3DBlendStateDescriptions.BlendStateDisabled,
          };

        #endregion


        internal RenderTechniques Technique { get; }
        public CullMode CullMode { get; }
        public PrimitiveTopology PrimitiveTopology { get; }


        //perhaps these blocks should be separate components as DepthStencilStateComponent and BlendStateComponent
        public bool HasDepthStencil { get; private set; }
        public DepthStencilStateDescription DepthStencilStateDescription { get; private set; }

        public bool HasBlendState { get; private set; }
        public BlendStateDescription BlendStateDescription { get; private set; }


        public RasterizerStateDescription2 RasterizerStateDescription { get; private set; }

        RenderableComponent(CullMode cullMode, PrimitiveTopology primitiveTopology, RenderTechniques technique) : this() {
            CullMode = cullMode;
            PrimitiveTopology = primitiveTopology;
            Technique = technique;
            IsRenderable = true;
        }

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsValid { get; private set; }
        public bool IsRenderable { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = false;
        }

        public RenderableComponent Disable() {
            Tag = ElementTag.New();
            IsRenderable = false;
            return this;
        }
        public RenderableComponent Enable() {
            Tag = ElementTag.New();
            IsRenderable = true;
            return this;
        }
    }
}
