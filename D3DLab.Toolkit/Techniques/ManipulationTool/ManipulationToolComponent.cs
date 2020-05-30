using D3DLab.ECS;
using System.Numerics;

namespace D3DLab.Toolkit.Techniques.ManipulationTool {
    public struct ManipulationToolComponent : IGraphicComponent {
        public ElementTag Tag { get; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        public void Dispose() {            
            
        }


        public Vector3 AttachPoint;
        public float Size; 
    }

    public class ManipulationToolObject {
        public static ManipulationToolObject Create(ManipulationToolComponent com) {
            var obj = new ManipulationToolObject();
            
            var halfSize = com.Size / 2f;
           // var boxgeo = GeometryBuilder.BuildBox(new BoundingBox(new Vector3(-halfSize, -halfSize, -halfSize), new Vector3(halfSize, halfSize, halfSize)));

           // var farCorner = new Vector3(,);

            return obj;
        }
    }



}
