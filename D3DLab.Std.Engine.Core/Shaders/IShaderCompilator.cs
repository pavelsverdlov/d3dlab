using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Shaders {
    public interface IShaderCompilator {
        IEnumerable<ShaderInfo> Infos { get; }
        void Compile(ShaderInfo info);
        void Compile(ShaderInfo info, string text);
    }
}
