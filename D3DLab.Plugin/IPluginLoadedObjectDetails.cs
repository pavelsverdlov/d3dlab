using System.Collections.Generic;
using System.IO;
using D3DLab.ECS;

namespace D3DLab.Plugin
{
    public interface IPluginLoadedObjectDetails {
        public FileInfo FilePath { get; }
        public IEnumerable<ElementTag> VisualObjectTags { get; }
    }
}