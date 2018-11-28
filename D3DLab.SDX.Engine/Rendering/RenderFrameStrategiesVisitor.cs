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

        public void Visit(D3DTriangleColoredVertexesRenderComponent component) {
            var type = typeof(ColoredVertexesRenderStrategy);
            var entityTag = component.EntityTag;
            try {
                var manager = contextState.GetComponentManager();

                var geometry = manager.GetComponent<IGeometryComponent>(entityTag);

                var tr = manager.GetComponent<D3DTransformComponent>(entityTag);

                //var v = ran.NextVector3(new Vector3(-100, -100, -100), new Vector3(100, 100, 100));
                tr.MatrixWorld = Matrix4x4.Identity;// Matrix4x4.CreateTranslation(v);

                GetOrCreate(() => new ColoredVertexesRenderStrategy(compilator, 
                    StategyStaticShaders.ColoredVertexes.GetPasses(),
                    StategyStaticShaders.ColoredVertexes.GetLayoutConstructor()))
                    .RegisterEntity(component, geometry, tr);
            }catch(Exception ex) {
                ex.ToString();
                throw ex;
            }
        }

        public void Visit(D3DLineVertexRenderComponent component) {
            var type = typeof(ColoredVertexesRenderStrategy);
            var entityTag = component.EntityTag;

            var geometry = contextState
                .GetComponentManager()
                .GetComponent<IGeometryComponent>(entityTag);

            GetOrCreate(() => new LineVertexRenderStrategy(compilator,
                    StategyStaticShaders.LineVertex.GetPasses(),
                    StategyStaticShaders.LineVertex.GetLayoutConstructor()))
                .RegisterEntity(component, geometry);
        }

        public void Visit(D3DSphereRenderComponent component) {
            var entityTag = component.EntityTag;

            var geometry = contextState
                .GetComponentManager()
                .GetComponent<IGeometryComponent>(entityTag);

            GetOrCreate(() => new SphereRenderStrategy(compilator,
                    StategyStaticShaders.SphereByPoint.GetPasses(),
                    StategyStaticShaders.SphereByPoint.GetLayoutConstructor()))
                .RegisterEntity(component, geometry);
        }

        public void Visit(D3DTerrainRenderComponent com) {
            var entityTag = com.EntityTag;
            var manager = contextState.GetComponentManager();

            var geometry = manager.GetComponent<IGeometryComponent>(entityTag);

            var material = manager.GetComponent<D3DTexturedMaterialComponent>(entityTag); 

            GetOrCreate(() => new TerrainRenderStrategy(compilator,
                    StategyStaticShaders.Terrain.GetPasses(),
                    StategyStaticShaders.Terrain.GetLayoutConstructor()))
                .RegisterEntity(com, geometry, material);
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
