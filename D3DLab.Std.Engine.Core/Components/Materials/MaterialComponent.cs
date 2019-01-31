using System.IO;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Components.Materials {
    public abstract class MaterialComponent : GraphicComponent {
        public abstract Vector4 GetVertexColor(int vertexInd);
    }
    public class ColorComponent : MaterialComponent {
        private Vector4 color;

        public Vector4 Color {
            get => color;
            set {
                color = value;
                IsModified = true;
            }
        }

        public override Vector4 GetVertexColor(int vertexInd) {
            return color;
        }
    }
    public class PositionColorsComponent : MaterialComponent {
        public Vector4[] Colors { get; set; }

        public override Vector4 GetVertexColor(int vertexInd) {
            return Colors[vertexInd];
        }

        public void UpdateColor(int index, Vector4 color) {
            Colors[index] = color;
            IsModified = true;
        }
    }

    public class MaterialComponent1 : GraphicComponent {
        public float Specular { get; set; } = -1; // -1 not specular

        public Vector4 AmbientColor { get; set; }
        public Vector4 DiffuseColor { get; set; }

    }

    public class TexturedMaterialComponent : GraphicComponent {
        public FileInfo Image { get; }

        public TexturedMaterialComponent(FileInfo image) {
            Image = image;
        }


    }
}
