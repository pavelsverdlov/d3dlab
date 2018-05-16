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
        public bool IsCached { get; private set; }

        public Shader VertexShader { get => Get(Veldrid.ShaderStages.Vertex); }
        public Shader GeometryShader { get => Get(Veldrid.ShaderStages.Geometry); }
        public Shader PixelShader { get => Get(Veldrid.ShaderStages.Fragment); }

        public ShaderSetDescription Description { get; private set; }

        public readonly IShaderInfo[] ShaderInfos;

        readonly Dictionary<Veldrid.ShaderStages, Shader> pass;

        public ShaderTechniquePass(IShaderInfo[] shaderInfos) {
            this.ShaderInfos = shaderInfos;
            pass = new Dictionary<Veldrid.ShaderStages, Shader>();
            foreach (var info in shaderInfos) {
                var stage = (Veldrid.ShaderStages)Enum.Parse(typeof(Veldrid.ShaderStages), info.Stage);
                if (pass.ContainsKey(stage)) {
                    throw new Exception($"One pass can contain only one {stage} shader");
                }
                pass.Add(stage, null);
            }
        }

        Shader[] ToArray() {
            return pass.Values.ToArray();
        }
        public void ClearCache() {
            pass.Clear();
            IsCached = false;
        }

        public void Update(ResourceFactory factory, VertexLayoutDescription[] layoutDescriptions) {
            pass.Clear();
            foreach (var info in ShaderInfos) {
                var stage = (Veldrid.ShaderStages)Enum.Parse(typeof(Veldrid.ShaderStages), info.Stage);
                Shader shader = factory.CreateShader(new ShaderDescription(stage, info.ReadCompiledBytes(), info.EntryPoint));
                shader.Name = Path.GetFileName(info.CompiledPath);
                pass[stage] = shader;
            }
            Description = new ShaderSetDescription(layoutDescriptions, ToArray());
            IsCached = true;
        }

        Shader Get(Veldrid.ShaderStages stage) {
            return pass[stage];
        }
    }
}
