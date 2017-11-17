using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Visual3D;
using SharpDX;

namespace D3DLab.Core.Entities {
    public struct DirectionalLightData {
//        public Vector3 Direction { get; set; }
        public Color4 Color { get; set; }
    }
    public sealed class DirectionalLightEntity : Entity<DirectionalLightData> {
        public DirectionalLightEntity(string tag) : base(tag) {
        }
    }

   
}
