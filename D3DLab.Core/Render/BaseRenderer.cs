using System;
using System.Windows.Forms;

namespace D3DLab.Core.Render {
    public abstract class BaseRenderer : IDisposable {
        protected int Width = 1280;
        protected int Height = 720;
        protected readonly IntPtr handleForm;

        ~BaseRenderer() { Dispose(false); }

        protected BaseRenderer(IntPtr handleForm) {
            this.handleForm = handleForm;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
        }
    }
}