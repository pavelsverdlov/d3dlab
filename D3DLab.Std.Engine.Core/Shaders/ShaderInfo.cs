using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.Std.Engine.Core.Shaders {
    public struct ShaderInfo {
        public string Path;
        /// <summary>
        /// Vertex/Fragment
        /// </summary>
        public string Stage;
        public string EntryPoint;

        public FileInfo GetFileInfo() {
            return new FileInfo(Path);
        }

    }
}
