using System.IO;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Components.Materials {
    public class MaterialComponent : GraphicComponent {

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
