using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.D2;
using D3DLab.SDX.Engine.Shader;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;

namespace D3DLab.SDX.Engine.Rendering {
    public interface IRenderProperties {
       CameraState CameraState { get; }
    }

    public interface IRenderTechnique<TProperties> where TProperties : IRenderProperties {
        IEnumerable<IRenderTechniquePass> GetPass();
        /// <summary>
        /// remove all entities from Technique
        /// </summary>
        void Cleanup();
        void Render(GraphicsDevice Graphics, TProperties game);
        void RegisterEntity(GraphicEntity entity);
        bool IsAplicable(GraphicEntity entity);
        /// <summary>
        /// Cleanup render cache, such as pass/shader/buffers/BlendState/render targets etc.
        /// should be invoked each time when render surface/device is recreared
        /// </summary>
        void CleanupRenderCache();
    }
    

    public abstract class D3DAbstractRenderTechnique<TProperties> where TProperties : IRenderProperties {
        protected readonly LinkedList<GraphicEntity> entities;
        //protected RasterizerStateDescription rasterizerStateDescription;
        [Obsolete("Move to specific technique/components")]
        protected BlendStateDescription blendStateDescription;
        [Obsolete("Move to specific technique/components")]
        protected DepthStencilStateDescription depthStencilStateDescription;

        readonly EntityHasSet entityHasSet;

        protected D3DAbstractRenderTechnique(EntityHasSet entityHasSet) {
           // pass = new HashSet<IRenderTechniquePass>();
            entities = new LinkedList<GraphicEntity>();
            this.entityHasSet = entityHasSet;
        }

        public void Render(GraphicsDevice graphics, TProperties game) {
            Rendering(graphics, game);
        }
        
        public bool IsAplicable(GraphicEntity entity) {
            return entityHasSet.HasComponents(entity);
        }


        protected abstract void Rendering(GraphicsDevice graphics, TProperties game);
        
        public void RegisterEntity(GraphicEntity entity) {
            entities.AddLast(entity);
        }

        public void Cleanup() {
            entities.Clear();
        }


        protected ShaderResourceView[] ConvertToResources(TexturedMaterialComponent material, TextureLoader loader) {
            var resources = new ShaderResourceView[material.Images.Length];
            for (var i = 0; i < material.Images.Length; i++) {
                var file = material.Images[i];
                resources[i] = loader.LoadShaderResource(file);
            }
            return resources;
        }

        public virtual void CleanupRenderCache() {

        }


    }
}
