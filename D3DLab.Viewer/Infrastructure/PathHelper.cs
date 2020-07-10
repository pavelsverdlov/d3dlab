using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.Viewer.Infrastructure {
    static class PathHelper {
        public static string GetPathWithMiddleSkipping(string fullPath) {
            var root = Path.GetPathRoot(fullPath);
            var name = Path.GetFileName(fullPath);

            return $"{root}...\\{name}";
        }
    }
}
