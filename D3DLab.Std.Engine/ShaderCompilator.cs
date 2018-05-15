using System;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using D3DLab.Std.Engine.Core.Shaders;
using System.Text;

namespace D3DLab.Std.Engine {
    public struct D3DShaderInfo : IShaderInfo {
        const string extention = ".hlsl";
        const string binary_extention = ".hlsl.bytes";

        public string Path { get; }
        public string CompiledPath { get { return System.IO.Path.ChangeExtension(Path, binary_extention); } }
        /// <summary>
        /// Vertex/Fragment
        /// </summary>
        public string Stage { get; }
        public string EntryPoint { get; }

        public D3DShaderInfo(string directory, string filename, string stage, string entry) {
            Path = System.IO.Path.Combine(directory, filename + extention);
            Stage = stage;
            EntryPoint = entry;
        }

        public byte[] ReadCompiledBytes() {
            return File.ReadAllBytes(CompiledPath);
        }

        public FileInfo GetFileInfo() {
            return new FileInfo(Path);
        }

        public string ReadText() {
            return File.ReadAllText(Path);
        }

        public byte[] ReadBytes() {
            return File.ReadAllBytes(Path);
        }

        public void WriteCompiledBytes(byte[] bytes) {
            File.WriteAllBytes(CompiledPath, bytes);
        }
    }
    public class ShaderCompilator : IShaderCompilator {
        readonly Veldrid.D3D11.D3D11ShaderCompilator compilator;

        public IEnumerable<IShaderInfo> Infos { get;  }
        public ShaderCompilator(IEnumerable<IShaderInfo> infos) {
            Infos = infos;
            compilator = new Veldrid.D3D11.D3D11ShaderCompilator();
        }
        public void Compile(IShaderInfo info) {
            var bytes = info.ReadBytes();
            bytes = compilator.Compile(bytes, info.EntryPoint, ConvertToShaderStage(info.Stage));
            info.WriteCompiledBytes(bytes);
        }

        public void Compile(IShaderInfo info, string text) {
            var bytes = Encoding.UTF8.GetBytes(text);
            bytes = compilator.Compile(bytes, info.EntryPoint, ConvertToShaderStage(info.Stage));
            info.WriteCompiledBytes(bytes);
        }

        private static ShaderStages ConvertToShaderStage(string stage) {
            return (ShaderStages)Enum.Parse(typeof(ShaderStages), stage);
        }
    }
}
