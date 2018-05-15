using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Std.Engine.Core.Shaders {
    public interface IShaderCompilator {
        IEnumerable<IShaderInfo> Infos { get; }
        void Compile(IShaderInfo info);
        void Compile(IShaderInfo info, string text);
    }
}
