using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using D3DLab.Std.Engine.Core.Components.Materials;
using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace D3DLab.SDX.Engine.Rendering {
    class RenderStrategyRegistrator {
        public IEnumerable<IRenderStrategy> Strategies { get { return dic.Values; } }
        readonly Dictionary<Type, IRenderStrategy> dic;

        public RenderStrategyRegistrator(D3DShaderCompilator compilator) {
            dic = new Dictionary<Type, IRenderStrategy>();
        }
      
        public void Register(GraphicEntity entity) {
            var renders = entity.GetComponents<D3DRenderComponent>();
            if (renders.Any() && renders.All(x=>x.CanRender)) {
                if(!entity.Has<IGeometryComponent>() || !entity.Has<D3DTransformComponent>()) {
                    throw new Exception("There are not all necessary components in entity to render."); 
                }

                GetOrCreate(() => new DefaultRenderStrategy())
                    .RegisterEntity(entity);
            }
        }
        T GetOrCreate<T>(Func<T> creator) where T : IRenderStrategy {
            var type = typeof(T);
            ///TODO:
            if (!dic.TryGetValue(type, out var str)) {
                dic[type] = creator();
                str = dic[type];
            }
            return (T)str;
        }
        public void Cleanup() {
            foreach (var s in Strategies) {
                s.Cleanup();
            }
        }
    }
}
