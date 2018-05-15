using D3DLab.Std.Engine.Core;
using Veldrid;
using Veldrid.Utilities;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Shaders;

namespace D3DLab.Std.Engine.Components {
    public abstract class ShaderComponent : GraphicComponent, IShaderEditingComponent {
        public readonly IShaderInfo[] ShaderInfos;
        public readonly ShaderTechniquePass TechniquePass;

        public ShaderSetDescription ShaderSetDesc { get; private set; }

        public ShaderComponent(IShaderInfo[] shaders) {
            this.ShaderInfos = shaders;
            TechniquePass = new ShaderTechniquePass();
        }

        public abstract VertexLayoutDescription[] GetLayoutDescription();

        #region IShaderEditingComponent

        public IShaderCompilator GetCompilator() {
            return new ShaderCompilator(ShaderInfos);
        }
        public void ReLoad() {
            TechniquePass.ClearCache();
        }

        #endregion

        public void UpdateShader(DisposeCollectorResourceFactory factory) {
            //to test
            var com = GetCompilator();
            foreach (var info in ShaderInfos) {
                com.Compile(info);
            }

            TechniquePass.Update(factory, ShaderInfos);
            ShaderSetDesc = new ShaderSetDescription(GetLayoutDescription(), TechniquePass.ToArray());
        }
    }
}
