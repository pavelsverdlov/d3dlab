using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Veldrid.D3D11 {
    public class D3D11ShaderCompilator {
        ShaderFlags sFlags = ShaderFlags.None;
        EffectFlags eFlags = EffectFlags.None;

        public string Preprocess(string shadertext) {
           return  ShaderBytecode.Preprocess(shadertext, new ShaderMacro[0]);
        }
        public byte[] Compile(byte[] shader, string entrypoint, ShaderStages stage) { // vs_5_0  ps_5_0
            //fxc /E VS /T vs_5_0 Vertex.hlsl /Fo Vertex.hlsl.bytes
            string profile = null;
            switch (stage) {
                case ShaderStages.Vertex:
                    profile = "vs_5_0";
                    break;
                case ShaderStages.Fragment:
                    profile = "ps_5_0";
                    break;
                case ShaderStages.Geometry:
                    profile = "gs_5_0";
                    break;
            }
            var res = ShaderBytecode.Compile(shader,entrypoint, profile, sFlags,eFlags);
            if(res.Bytecode == null) {
                throw new Exception(res.Message);
            }
            return res.Bytecode.Data;
        }
        public void CompileToFile(FileInfo file,string shadertext) {
            var shaderBytes = ShaderBytecode.Compile(shadertext, "vs_5_0", sFlags, eFlags);
            File.WriteAllBytes(file.FullName, shaderBytes.Bytecode.Data);
        }
    }
}
