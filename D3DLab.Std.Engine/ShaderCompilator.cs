using System;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using D3DLab.Std.Engine.Core.Shaders;
using System.Text;

namespace D3DLab.Std.Engine {
    public class ShaderCompilator : IShaderCompilator {
        readonly Veldrid.D3D11.D3D11ShaderCompilator compilator;

        public IEnumerable<ShaderInfo> Infos { get;  }
        public ShaderCompilator(IEnumerable<ShaderInfo> infos) {
            Infos = infos;
            compilator = new Veldrid.D3D11.D3D11ShaderCompilator();
        }
        public void Compile(ShaderInfo info) {
            var fi = info.GetFileInfo();
            var bytes = File.ReadAllBytes(fi.FullName);

            bytes = compilator.Compile(bytes, info.EntryPoint, ConvertToShaderStage(info.Stage));

            File.WriteAllBytes(fi.FullName+".bytes", bytes);
        }

        public void Compile(ShaderInfo info, string text) {
            var fi = info.GetFileInfo();
            var bytes = Encoding.UTF8.GetBytes(text);

            bytes = compilator.Compile(bytes, info.EntryPoint, ConvertToShaderStage(info.Stage));

            File.WriteAllBytes(fi.FullName + ".bytes", bytes);
        }

        private static ShaderStages ConvertToShaderStage(string stage) {
            return (ShaderStages)Enum.Parse(typeof(ShaderStages), stage);
        }
    }
}
