using System;

namespace HelixToolkit.Wpf.SharpDX {
    public class MakePhotoParameters {
        public int Width;
        public int Height;
        public bool DrawBackground;
        public Camera Camera;
        public Action PrepareAction;
        public Action RestoreAction;
    }
}