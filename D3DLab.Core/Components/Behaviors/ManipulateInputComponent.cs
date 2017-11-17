using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using D3DLab.Core.Entities;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;

namespace D3DLab.Core.Components.Behaviors {
    public sealed class ManipulateInputComponent : InputComponent, IAttachTo<VisualEntity>, ManipulateInputComponent.IInputHandler {
        public enum InputStates {
            Idle,
            Translate
        }
        public interface IInputHandler : IHandler {
            void Translate(InputStateDate state);
        }
        private VisualEntity parent;
        public ManipulateInputComponent(Control control) : base(control) {}
        public void OnAttach(VisualEntity parent) { this.parent = parent; }
        protected override InputState GetIdleState() {
            var states = new StateDictionary();
            states.Add((int)InputStates.Idle, s => new InputIdleState(s));
            states.Add((int)InputStates.Translate, s => new InputTranslateState(s));

            var router = new StateHandleProcessor<IInputHandler>(states, this);
            router.SwitchTo((int)InputStates.Idle, new InputStateDate(control));
            return router;
        }
        private sealed class InputIdleState : InputStateMachine {
            public InputIdleState(StateProcessor processor) : base(processor) { }
            public override bool OnMouseDown(InputStateDate state) {
                switch (state.Buttons) {
                    case MouseButtons.Left:
                        SwitchTo((int)InputStates.Translate, state);
                        break;
                }
                return base.OnMouseDown(state);
            }
            
        }

        private sealed class InputTranslateState : InputStateMachine {
            public InputTranslateState(StateProcessor processor) : base(processor) {

            }

            public override bool OnMouseDown(InputStateDate state) {
                return base.OnMouseDown(state);
            }

            public override bool OnMouseUp(InputStateDate state) {
                if (!state.IsPressed(MouseButtons.Left)) {
                    SwitchTo((int)InputStates.Idle, state);
                }
                return base.OnMouseUp(state);
            }

            public override bool OnMouseMove(InputStateDate state) {
                Processor.InvokeHandler<IInputHandler>(x=>x.Translate(state));
                return true;
            }
        }
        private Matrix matrix;
        public void TranslateFinish() {
            matrix = Matrix.Identity;
        }

        public void Translate(InputStateDate state) {
            var visual = parent;
            var scene = visual.Parent;
            var camera = scene.GetComponents<ICameraComponent>().Single().Data;
            var visualCenter = Vector3.Zero;// Vector3.TransformCoordinate(Vector3.Zero, visual.Data.Transform);
            /*
            var ray = scene.ConvertToRay(state.ButtonsStates[MouseButtons.Left].PointDown);
            Vector3 startPoint;
            var plane = new Plane(visualCenter, Vector3.UnitY);
            ray.Intersects(ref plane, out startPoint);

            
            Vector3 currentPoint;
            plane = new Plane(visualCenter, Vector3.UnitZ);
            scene.ConvertToRay(state.CurrentPosition)
                .Intersects(ref plane, out currentPoint);
            
            var move = currentPoint - startPoint;*/
//            var startRay = scene.UnProject(state.ButtonsStates[MouseButtons.Left].PointDown);
//            startRay.Direction *= -1;
//            var endRay = scene.UnProject(state.CurrentPosition);
//            endRay.Direction *= -1;

            var startRay = scene.Point2DtoRay3D(state.ButtonsStates[MouseButtons.Left].PointDown);
            var endRay = scene.Point2DtoRay3D(state.CurrentPosition);
            startRay.Direction *= -1;
            endRay.Direction *= -1;

            //            Debug.Wri  startRay.Direction *= -1;teLine(startRay + " " + startRay1);
            //            Debug.WriteLine(endRay + " " + endRay1);

            //            Debug.WriteLine(state.ButtonsStates[MouseButtons.Left].PointDown + " | " + state.CurrentPosition);

            var matrix = Matrix.Identity;
            foreach (var dir in new [] { Vector3.UnitY, Vector3.UnitX }) {// Vector3.UnitZ, Vector3.UnitY
                var axis = dir;
                var planeForDetectingNormal = Vector3.Cross(camera.LookDirection.Normalized(), axis);
                planeForDetectingNormal.Normalize();
                var planeNormalForAxis = Vector3.Cross(planeForDetectingNormal, axis);
                planeNormalForAxis.Normalize();
                var plane = new Plane(visualCenter, dir);
                
                Vector3 startPointWorld;
                Vector3 endPointWorld;


                startPointWorld = PlaneIntersection(visualCenter, dir, startRay.Position, startRay.Direction);
                endPointWorld = PlaneIntersection(visualCenter, dir, endRay.Position, endRay.Direction);
//
                if (startRay.Intersects(ref plane, out startPointWorld)) {
                    
                }
                if (endRay.Intersects(ref plane, out endPointWorld)) {
                    
                }

                /*
                var direction = Vector3.TransformNormal(dir, visual.Data.Transform);

                var angleInDegrees = camera.LookDirection.AngleBetween(direction);
                var angleInRadians = MathUtil.DegreesToRadians(angleInDegrees);// angleInDegrees / 180 * Math.PI;
                var normalizationCoeficient = Math.Sin(angleInRadians);
                var up = Vector3.Cross(camera.LookDirection, direction);
                var hitPlaneNormal = Vector3.Cross(up, direction);
                
                var ray = scene.UnProject(state.CurrentPosition);
                var plane = new Plane(visualCenter, hitPlaneNormal);
                Vector3 startPointWorld;
                ray.Intersects(ref plane, out startPointWorld);

                ray = scene.UnProject(state.ButtonsStates[MouseButtons.Left].PointDown);
                plane = new Plane(visualCenter, hitPlaneNormal);
                Vector3 endPointWorld;
                ray.Intersects(ref plane, out endPointWorld);

                */
//                Debug.WriteLine("St " + startPointWorld);
//                Debug.WriteLine("En " + endPointWorld);
                var move = endPointWorld - startPointWorld;
                if (move == Vector3.Zero) {
                    continue;
                }
             
                matrix = matrix * Matrix.Translation(move); 
            }
            Debug.WriteLine("Move " + matrix.TranslationVector);
            visual.Data.Transform = matrix;
        }
        public Vector3 PlaneIntersection(Vector3 position, Vector3 normal, Vector3 rayP, Vector3 rayN) {
            // http://paulbourke.net/geometry/planeline/
            float dn = Vector3.Dot(normal, rayN);
            if (dn == 0) {
                return Vector3.Zero;
            }

            float u = Vector3.Dot(normal, position - rayP) / dn;
            return rayP + u * rayN;
        }

    }
}
