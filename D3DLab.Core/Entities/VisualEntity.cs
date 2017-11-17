using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using D3DLab.Core.Visual3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using SharpDX.Direct3D11;

namespace D3DLab.Core.Entities {
    public sealed class VisualData {
        public MeshGeometry3D Geometry { get; set; }

        public PhongMaterial Material { get; set; }
        public PhongMaterial BackMaterial { get; set; }
        public RenderTechnique RenderTechnique { get; set; }
        
        //structures
//        public BoundingBox Bounds { get; set; }
        public Matrix Transform { get; set; }
        public CullMode CullMaterial { get; set; }
        public int DepthBias { get; set; }
        public float Thickness { get; set; }
        public float Smoothness { get; set; }
        public Vector2 TextureCoordScale { get; set; }

        public VisualData() {
            RenderTechnique = Techniques.RenderPhongWithAmbient;
            Transform = Matrix.Identity;
            CullMaterial = CullMode.Back;
        }
    }
    public sealed class VisualEntity : SceneAttachedEntity<VisualData>{
        public VisualEntity(string tag) : base(tag) {

        }
    }
}
