using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Components {
    public interface IColoringMaterialComponent : IGraphicComponent {
        Vector4 GetVertexColor(int vertexInd);
    }

    public struct ColorComponent : IColoringMaterialComponent {

        public static ColorComponent Create(Vector4 color) {
            return new ColorComponent {
                IsValid = true,
                IsModified = true,
                Tag = new ElementTag(Guid.NewGuid().ToString()),
                Color = color,
            };
        }

        public Vector4 Color { get; private set; }

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = true;
        }

        public Vector4 GetVertexColor(int vertexInd) {
            throw new System.NotImplementedException();
        }
    }
}
