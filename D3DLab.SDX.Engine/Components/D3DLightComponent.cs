using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Components;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace D3DLab.SDX.Engine.Components {



    
   
    public class D3DLightComponent : GraphicComponent, ILightComponent {
        /// <summary>
        /// 0 - 1 range
        /// </summary>
        public float Intensity { get; set; }
        public int Index { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public LightTypes Type { get; set; }
    }

    
}
