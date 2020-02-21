using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D3DLab.ECS.Common {
    public class ManifestResourceLoader {
        readonly Type type;
        public ManifestResourceLoader(Type assembly) {
            type = assembly;
        }

        string GetManifestResourceNameBy(string resource) {
            foreach (var name in type.Assembly.GetManifestResourceNames()) {
                if (name.EndsWith(resource)) {
                    return name;
                }
            }
            return null;
        }

        public string GetResourceTextByName(string resource) {
            var text = string.Empty;
            resource = GetManifestResourceNameBy(resource);
            using (var srt = type.Assembly.GetManifestResourceStream(resource)) {
                var reader = new StreamReader(srt);
                text = reader.ReadToEnd();
            }
            return text;
        }
        public Stream GetResourceStreamByName(string resource) {
            resource = GetManifestResourceNameBy(resource);
            return type.Assembly.GetManifestResourceStream(resource);
        }

    }
}
