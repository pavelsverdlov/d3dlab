using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace D3DLab.Viewer.Infrastructure.Settings {
    [Serializable]
    public class ObjGroupFilter {
        public string Name { get; set; }
        public string Color { get; set; }
        public string Filter { get; set; }
    }
}
