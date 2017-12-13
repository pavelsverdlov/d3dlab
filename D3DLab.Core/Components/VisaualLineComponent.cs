using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace D3DLab.Core.Entities {
    public struct VisaualLineComponent {
        public Color Color { get; set; }
        public float Thickness { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }
    }    
}
