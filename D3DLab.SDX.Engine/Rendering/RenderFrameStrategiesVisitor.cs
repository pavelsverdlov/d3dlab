using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering.Strategies;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace D3DLab.SDX.Engine.Rendering {
    internal class RenderFrameStrategiesVisitor {
        IContextState contextState;
        readonly D3DShaderCompilator compilator;

        public IEnumerable<IRenderStrategy> Strategies { get { return dic.Values; } }
        readonly Dictionary<Type, IRenderStrategy> dic;

        Random ran;

        public RenderFrameStrategiesVisitor(D3DShaderCompilator compilator) {
            this.compilator = compilator;
            dic = new Dictionary<Type, IRenderStrategy>();
            ran = new Random(100);
        }

        public void SetContext(IContextState contextState) {
            this.contextState = contextState;
        }

        public void Visit(Components.D3DTriangleColoredVertexesRenderComponent component) {
            var type = typeof(ColoredVertexesRenderStrategy);
            var entityTag = component.EntityTag;

            var manager = contextState.GetComponentManager();

            var geometry = manager.GetComponent<IGeometryComponent>(entityTag);

            var tr = manager.GetComponent<D3DTransformComponent>(entityTag);

            //var v = ran.NextVector3(new Vector3(-100, -100, -100), new Vector3(100, 100, 100));
            tr.MatrixWorld = Matrix4x4.Identity;// Matrix4x4.CreateTranslation(v);

            GetOrCreate(() => new ColoredVertexesRenderStrategy(compilator))
                .RegisterEntity(component, geometry, tr);
        }

        public void Visit(Components.D3DLineVertexRenderComponent component) {
            var type = typeof(ColoredVertexesRenderStrategy);
            var entityTag = component.EntityTag;

            var geometry = contextState
                .GetComponentManager()
                .GetComponent<IGeometryComponent>(entityTag);

            GetOrCreate(() => new LineVertexRenderStrategy(compilator))
                .RegisterEntity(component, geometry);
        }

        T GetOrCreate<T>(Func<T> creator) where T : IRenderStrategy {
            var type = typeof(T);
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
