using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.Std.Engine.Core.Shaders {
    public interface IShaderInfo {
        string Stage { get; }
        string EntryPoint { get; }
        string CompiledPath { get; }

        byte[] ReadCompiledBytes();
        void WriteCompiledBytes(byte[] bytes);

        string ReadText();
        byte[] ReadBytes();
        
    }    
}
