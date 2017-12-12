using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using D3DLab.Core.Entities;
using SharpDX;
using HelixToolkit.Wpf.SharpDX;
using Cursor = System.Windows.Forms.Cursor;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace D3DLab.Core.Render {

    public sealed class World {
        public Matrix WorldMatrix { get; set; }
        public int LightCount { get; set; }

        public Matrix ViewMatrix { get; set; }
        public Matrix ProjectionMatrix { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 LookDirection { get; set; }

        public MouseButtons MouseButtons { get; private set; }

        public Vector2 MousePoint { get; private set; }


        public void UpdateInputState() {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {



                MousePoint = Mouse.GetPosition(System.Windows.Application.Current.MainWindow).ToVector2();
                MouseButtons = Control.MouseButtons;
                Debug.WriteLine(MousePoint);



            });
        }


        //private MouseEventArgs FixMouse()
        //{
        //    //var position = Extensions.MouseEx.GetPosition();
        //  //  return new MouseEventArgs(ea.Button, ea.Clicks, (int)position.X, (int)position.Y, ea.Delta);
        //}

        public Matrix GetViewportMatrix() {
            return new Matrix(
                (float)(actualWidth / 2),
                0,
                0,
                0,
                0,
                (float)(-actualHeight / 2),
                0,
                0,
                0,
                0,
                1,
                0,
                (float)((actualWidth - 1) / 2),
                (float)((actualHeight - 1) / 2),
                0,
                1);
        }

        readonly double actualWidth;
        readonly double actualHeight;
        private readonly Control control;
        // public OrthographicCamera Camera { get; set; }

        public World(Control control, double actualWidth, double actualHeight) {
            this.control = control;
            this.actualHeight = actualHeight;
            this.actualWidth = actualWidth;
            WorldMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
            ViewMatrix = Matrix.Identity;
        }
    }
}