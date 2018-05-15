using D3DLab.Std.Engine.Core.Shaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Veldrid;

namespace D3DLab.Std.Engine.Shaders {
    /// <summary>
    /// Class just describe shader technique structure, no spetific actions or behaviours just for readability
    /// </summary>
    public class ShaderTechniquePass {
        public bool IsCached { get => pass.Any(); }

        public Shader VertexShader { get => Get(Veldrid.ShaderStages.Vertex); }
        public Shader GeometryShader { get => Get(Veldrid.ShaderStages.Geometry); }
        public Shader PixelShader { get => Get(Veldrid.ShaderStages.Fragment); }

        readonly Dictionary<Veldrid.ShaderStages, Shader> pass;
        public ShaderTechniquePass() {
            pass = new Dictionary<Veldrid.ShaderStages, Shader>();
        }

        public Shader[] ToArray() {
            return pass.Values.ToArray();
        }
        public void ClearCache() {
            pass.Clear();
        }

        public void Update(ResourceFactory factory, IShaderInfo[] shaderInfos) {
            pass.Clear();
            foreach (var info in shaderInfos) {
                var stage = (Veldrid.ShaderStages)Enum.Parse(typeof(Veldrid.ShaderStages), info.Stage);
                Shader shader = factory.CreateShader(new ShaderDescription(stage, info.ReadCompiledBytes(), info.EntryPoint));
                shader.Name = Path.GetFileName(info.CompiledPath);
                pass[stage] = shader;
            }
        }

        Shader Get(Veldrid.ShaderStages stage) {
            return pass[stage];
        }
    }
}
