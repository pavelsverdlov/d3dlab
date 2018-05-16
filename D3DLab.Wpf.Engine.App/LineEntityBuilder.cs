using D3DLab.Std.Engine;
using D3DLab.Std.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Shaders;
using D3DLab.Std.Engine.Systems;
using System;
using System.IO;
using System.Numerics;
using Veldrid;

namespace D3DLab.Wpf.Engine.App {
    public static class LineEntityBuilder {
        public static void Build(IContextState context) {
            IShaderInfo[] lineShaders = {
                    new D3DShaderInfo(
                        Path.Combine(AppContext.BaseDirectory, "Shaders", "Line"),
                        $"line-{ShaderStages.Vertex}",
                        ShaderStages.Vertex.ToString(),
                        "VShaderLines"),
                        //new ShaderInfo{ Path= $"{Path.Combine(AppContext.BaseDirectory, "Shaders", "Line", "line")}-{ShaderStages.Geometry}.hlsl",
                    //    Stage = ShaderStages.Geometry.ToString(), EntryPoint = "GShaderLines"},
                     new D3DShaderInfo(
                        Path.Combine(AppContext.BaseDirectory, "Shaders", "Line"),
                        $"line-{ShaderStages.Fragment}",
                        ShaderStages.Fragment.ToString(),
                        "PShaderLinesFade")
                };

            var mb = new Std.Engine.Helpers.MeshBulder();
            var box = mb.BuildBox(Vector3.Zero, 1, 1, 1);


            var line = context.GetEntityManager()
                  .CreateEntity(new ElementTag(Guid.NewGuid().ToString()))
                  .AddComponent(new LineGeometryRenderComponent(new[] { new ShaderTechniquePass(lineShaders) }, box
                  // new Std.Engine.Helpers.LineBuilder().Build(box.Positions.GetRange(0,3))
                  ));

            context.EntityOrder.RegisterOrder<VeldridRenderSystem>(line.Tag);
        }
    }
}
