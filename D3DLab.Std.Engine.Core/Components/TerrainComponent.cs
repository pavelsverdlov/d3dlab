using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Std.Engine.Core.Components {
    public interface ITerrainComponent : IGraphicComponent{
        int Width { get; set; }
        int Heigth { get; set; }
        //Vector3[] HeightMap { get; set; }
    }
}
