using D3DLab.Std.Engine.Core;
using Veldrid;
using Veldrid.Utilities;
using D3DLab.Std.Engine.Core.Shaders;
using D3DLab.Std.Engine.Shaders;
using D3DLab.Std.Engine.Common;
using System.Linq;

namespace D3DLab.Std.Engine.Components {
    public abstract class ShaderComponent : GraphicComponent, IShaderEditingComponent {
        
        public readonly ShaderTechniquePass[] Passes;
        
        public ShaderComponent(ShaderTechniquePass[] passes) {
            Passes = passes;
        }

        public abstract VertexLayoutDescription[] GetLayoutDescription();

        #region IShaderEditingComponent

        public IShaderCompilator GetCompilator() {
            return new ShaderCompilator(Passes[0].ShaderInfos);
        }
        public void ReLoad() {
            Passes[0].ClearCache();
        }

        #endregion

        public void UpdateShaders(DisposeCollectorResourceFactory factory) {
            foreach (var pass in Passes) {
                pass.Update(factory, GetLayoutDescription());
            }
        }



        protected ushort[] ConvertToShaderIndices(Geometry3D geo) {
            return geo.Indices.Select(x => (ushort)x).ToArray();
        }
    }
}
