using D3DLab.Core.Render;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D3DLab.Core.Test {
    public interface IComponent : IDisposable { }
    
    public interface IHittableComponent : IComponent { }

    public interface ITransformableComponent : IComponent {
        void AddMatrix(Matrix matrix);
        Matrix GetMatrix();
    }

    public interface ISubscriberComponent<TMessage> : IComponent where TMessage : IMessageComponent {
        void Push(TMessage message);
    }

    public interface IMessageComponent : IComponent {

    }
    
    public abstract class Component : IComponent {
        public void Dispose() {

        }
    }

   
        
    public struct InputStates {
        public MouseButtons Buttons { get; set; }
    }


    /*
     *
     *
     */

    public sealed class GeometryComponent : Component {
        public HelixToolkit.Wpf.SharpDX.MeshGeometry3D Geometry { get; set; }

       
    }

    public sealed class MaterialComponent : Component {
        public HelixToolkit.Wpf.SharpDX.PhongMaterial Material { get; set; }
        public HelixToolkit.Wpf.SharpDX.PhongMaterial BackMaterial { get; set; }
        public CullMode CullMaterial { get; set; }

    }

    public sealed class RenderComponent : Component {
        public HelixToolkit.Wpf.SharpDX.RenderTechnique RenderTechnique { get; set; }

       
    }

    public sealed class TransformComponent : Component {
        public Matrix Matrix { get; set; }

       
    }


}
