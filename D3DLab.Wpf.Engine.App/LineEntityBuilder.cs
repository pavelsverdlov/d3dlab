using D3DLab.Std.Engine;
using D3DLab.Std.Engine.Common;
using D3DLab.Std.Engine.Components;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Shaders;
using D3DLab.Std.Engine.Systems;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace D3DLab.Wpf.Engine.App {
    public static class SolidGeometryEntityBuilder {
        public static void Build(IContextState context, Geometry3D geo) {
            IShaderInfo[] shaders = {
                    new D3DShaderInfo(
                        Path.Combine(AppContext.BaseDirectory, "Shaders", "SolidMesh"),
                        $"solid-{ShaderStages.Vertex}",
                        ShaderStages.Vertex.ToString(),
                        "VShaderDefault"),
                        //new ShaderInfo{ Path= $"{Path.Combine(AppContext.BaseDirectory, "Shaders", "Line", "line")}-{ShaderStages.Geometry}.hlsl",
                    //    Stage = ShaderStages.Geometry.ToString(), EntryPoint = "GShaderLines"},
                     new D3DShaderInfo(
                        Path.Combine(AppContext.BaseDirectory, "Shaders", "SolidMesh"),
                        $"solid-{ShaderStages.Fragment}",
                        ShaderStages.Fragment.ToString(),
                        "FS")
                };
            var line = context.GetEntityManager()
                  .CreateEntity(new ElementTag(Guid.NewGuid().ToString()))
                  .AddComponent(new SolidGeometryRenderComponent(new[] { new ShaderTechniquePass(shaders) }, geo
                  ));

            context.EntityOrder.RegisterOrder<VeldridRenderSystem>(line.Tag);
        }
    }

    public static class LineEntityBuilder {
        public static void Build(IContextState context, Vector3[] points) {
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
            //var box = mb.BuildBox(Vector3.Zero, 1, 1, 1);

            var lb = new Std.Engine.Helpers.LineBuilder();

            //var box = lb.Build(mb.BuildBox(Vector3.Zero, -20, 20, 20).Positions);// points.ToList().GetRange(0,20));
            var dd = points.ToList().GetRange(0, 5).ToList();
            var box = lb.Build(points);


            var line = context.GetEntityManager()
                  .CreateEntity(new ElementTag("LineEntity"))
                  .AddComponent(new LineGeometryRenderComponent(new[] { new ShaderTechniquePass(lineShaders) }, box
                  // new Std.Engine.Helpers.LineBuilder().Build(box.Positions.GetRange(0,3))
                  ));

            context.EntityOrder.RegisterOrder<VeldridRenderSystem>(line.Tag);
        }
    }
}
