using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Toolkit.Components {
    public struct MaterialColorComponent : IGraphicComponent {
        public static MaterialColorComponent Create() {
            return new MaterialColorComponent {
                IsValid = true,
                IsModified = true,
                Tag = new ElementTag(Guid.NewGuid().ToString()),
            };
        }
        public static MaterialColorComponent Create(Vector4 color) {
            return new MaterialColorComponent {
                IsValid = true,
                IsModified = true,
                Tag = new ElementTag(Guid.NewGuid().ToString()),

                Ambient = color,
                Diffuse = color,
                Specular = color,
                Reflection = color,
            };
        }

        public MaterialColorComponent Clone() {
            return new MaterialColorComponent() {
                IsValid = true,
                IsModified = true,
                Tag = new ElementTag(Guid.NewGuid().ToString()),
                Ambient = Ambient,
                Diffuse = Diffuse,
                Specular = Specular,
                SpecularFactor = 400f,
                HasAlpha = HasAlpha
            };
        }

        public Vector4 Ambient;
        public Vector4 Diffuse;
        public Vector4 Specular;
        public Vector4 Reflection;
        public float SpecularFactor;

        public bool HasAlpha { get; private set; }

        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            IsDisposed = true;
        }
        public void SetAlpha(float alfa) {
            HasAlpha = alfa < 1;
            Ambient.W = alfa;
            Diffuse.W = alfa;
            Specular.W = alfa;
            IsModified = true;
        }
    }
}
