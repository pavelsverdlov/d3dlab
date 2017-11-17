using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Controllers;
using SharpDX;
using TDx.TDxInput;

namespace D3DLab.Core.Input {
    public class Mouse3DController : IMouse3DController {
        private Device device;
        private Sensor sensor;
        private Keyboard keyboard;
        private InputController inputController;

        public Mouse3DController() {
            
        }

        public void Initialize(InputController inputController) {
            this.inputController = inputController;

            try {
                device = new Device();
                sensor = device.Sensor;
                keyboard = device.Keyboard;
                //device.LoadPreferences("Zirkonzahn.Nesting.exe");
                //device.LoadPreferences("vshost.exe");
                sensor.SensorInput += DoSensorInput;
                keyboard.KeyDown += DoKeyDown;
                keyboard.KeyUp += DoKeyUp;
                device.Connect();
                Debug.Assert(device.IsConnected);
            } catch {
                device = null;
                sensor = null;
                keyboard = null;
            }
        }

        private void DoSensorInput() {
            if (device == null)
                return;
            //inputController.CameraViewController.RotateCenter;

            var axis = new Vector3((float)sensor.Rotation.X, (float)sensor.Rotation.Y, (float)sensor.Rotation.Z);
            var haveRotate = axis.Length() > 0.001f && sensor.Rotation.Angle > 0.001f;

            if (haveRotate && !inputController.OnAllowSwitchToState(null, InputControllerState.Rotate))
                haveRotate = false;

            var trans = new Vector3((float)sensor.Translation.X, (float)sensor.Translation.Y, (float)sensor.Translation.Z);
            var haveTranslate = trans.ToVector2IgnoreZ().Length() > 0.001f;
            var haveZoom = Math.Abs(trans.Z) > 0.001f;

            if (!haveTranslate && !haveRotate && !haveZoom)
                return;

            //inputController.CameraViewController.Rotate(axis, (float)sensor.Rotation.Angle);

            if (haveRotate) {
                axis.X = -axis.X;
                axis.Y = -axis.Y;
                inputController.CameraViewController.Rotate(axis, (float)sensor.Rotation.Angle * 0.01f);
            }

            if (haveTranslate) {
                var move = trans.ToVector2IgnoreZ() * 0.01f;
                move.Y = -move.Y;
                inputController.CameraViewController.Pan(move);
            }

            if (haveZoom) {
                inputController.CameraViewController.Zoom(trans.Z * 0.01f);
            }
        }

        private void DoKeyDown(int key) {
            if (device == null)
                return;
        }

        private void DoKeyUp(int key) {
            if (device == null)
                return;
        }

        
    }
}
