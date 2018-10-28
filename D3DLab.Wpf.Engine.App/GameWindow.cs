using D3DLab.Std.Engine;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Input;
using D3DLab.Wpf.Engine.App.Host;
using D3DLab.Wpf.Engine.App.Input;
using System;
using System.Windows;

namespace D3DLab.Wpf.Engine.App {
    public sealed class GameWindow : IAppWindow {
        private WinFormsD3DControl win;

        

        public GameWindow(WinFormsD3DControl win, CurrentInputObserver input) {
            this.win = win;
            Width = (float)Application.Current.MainWindow.ActualWidth;
            Height = (float)Application.Current.MainWindow.ActualHeight;
            InputManager = new InputManager(input);
        }

        public float Width { get; }

        public float Height { get; }

        public bool IsActive => true;

        public IntPtr Handle => win.Handle;

        public IInputManager InputManager { get; }
    }
}
