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

}
