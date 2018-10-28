using D3DLab.Std.Engine.Core.Shaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.SDX.Engine.Shader {
    public struct D3DShaderInfo : IShaderInfo {
        const string extention = ".hlsl";
        const string binary_extention = ".hlsl.bytes";

        public string Name { get { return compiledPath; } }

        /// <summary>
        /// Vertex/Fragment
        /// </summary>
        public string Stage { get; }
        public string EntryPoint { get; }
        readonly string path;
        readonly string compiledPath;
        byte[] compiledBytes;

        public D3DShaderInfo(string directory, string filename, string stage, string entry) {
            path = System.IO.Path.Combine(directory, filename + extention);
            compiledPath = System.IO.Path.ChangeExtension(path, binary_extention);
            Stage = stage;
            EntryPoint = entry;
            compiledBytes = null;
        }

        public byte[] ReadCompiledBytes() {
            if (compiledBytes == null) {
                compiledBytes = File.ReadAllBytes(compiledPath);
            }
            return compiledBytes;
        }

        public FileInfo GetFileInfo() {
            return new FileInfo(path);
        }

        public string ReadText() {
            return File.ReadAllText(path);
        }

        public byte[] ReadBytes() {
            return File.ReadAllBytes(path);
        }

        public void WriteCompiledBytes(byte[] bytes) {
            compiledBytes = bytes;
            File.WriteAllBytes(compiledPath, bytes);
        }
    }

    public class D3DShaderCompilator : IShaderCompilator {
        readonly D3D11ShaderCompilator compilator;
        readonly Dictionary<string, string> resources;

        public D3DShaderCompilator() {
            compilator = new D3D11ShaderCompilator();
            resources = new Dictionary<string, string>();
        }

        public void CompileWithPreprocessing(IShaderInfo info) {
            var text  = info.ReadText();
            var preprocessed = compilator.Preprocess(text, new D3D11Include(resources));
            var bytes = Encoding.UTF8.GetBytes(preprocessed);
            bytes = compilator.Compile(bytes, info.EntryPoint, ConvertToShaderStage(info.Stage), info.Name);
            info.WriteCompiledBytes(bytes);
        }

        public void Compile(IShaderInfo info) {
            var bytes = info.ReadBytes();
            bytes = compilator.Compile(bytes, info.EntryPoint, ConvertToShaderStage(info.Stage), info.Name);
            info.WriteCompiledBytes(bytes);
        }

        public void Compile(IShaderInfo info, string text) {
            var bytes = Encoding.UTF8.GetBytes(text);
            bytes = compilator.Compile(bytes, info.EntryPoint, ConvertToShaderStage(info.Stage), info.Name);
            info.WriteCompiledBytes(bytes);
        }

        public byte[] Compile(string text, string entryPoint, string stage) {
            var bytes = Encoding.UTF8.GetBytes(text);
            return compilator.Compile(bytes, entryPoint, ConvertToShaderStage(stage), "undefined");
        }

        private static ShaderStages ConvertToShaderStage(string stage) {
            return (ShaderStages)Enum.Parse(typeof(ShaderStages), stage);
        }

        internal void AddResources(string include, string resource) {
            resources.Add(include, resource);
        }
    }
}
