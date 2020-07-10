using D3DLab.ECS.Ext;

using Syncfusion.Windows.Tools;

using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Threading;

using WPFLab;
using WPFLab.MVVM;

namespace D3DLab.Viewer.Modules.Transform {
    class StepAxisTransform : BaseNotify {
        readonly Vector3 axis;
        readonly TransformTranslate facade;
        float moved;
        float step;

        public float Moved { 
            get => moved;
            set {
                facade.Move(value, axis);
                Update(ref moved, value, nameof(Moved));                
            }
        }
        public float Step { get => step; set => Update(ref step, value, nameof(Step)); }
        
        public ICommand ForwardCommand { get; }
        public ICommand BackwardCommand { get; }
        public ICommand MouseMoveCommand { get; }

        public StepAxisTransform(Vector3 axis, TransformTranslate facade) {
            Step = 0.1f;
            this.axis = axis;
            this.facade = facade;
            ForwardCommand = new WpfActionCommand(OnForward);
            BackwardCommand = new WpfActionCommand(OnBackward);
            MouseMoveCommand = new WpfActionCommand(OnMouseMove);
        }

        void OnMouseMove() {
            facade.ShowAxis(axis);
        }

        void OnForward() {
            facade.Move(Step, axis);
            moved += Step;
            SetPropertyChanged(nameof(Moved));
        }

        void OnBackward() {
            facade.Move(Step, -axis);
            moved -= Step;
            SetPropertyChanged(nameof(Moved));
        }

        public void Reset() {
            Moved = 0;
        }
    }
    class TransformTranslate {
        readonly TransformModuleViewModel facade;

        public StepAxisTransform XAxis { get; set; }
        public StepAxisTransform YAxis { get; set; }
        public StepAxisTransform ZAxis { get; set; }

        Vector3 showedAxis;
        readonly DispatcherTimer timer;
        readonly Stopwatch stopwatch;
        public TransformTranslate(TransformModuleViewModel facade) {
            XAxis = new StepAxisTransform(Vector3.UnitX, this);
            YAxis = new StepAxisTransform(Vector3.UnitY, this);
            ZAxis = new StepAxisTransform(Vector3.UnitZ, this);
            this.facade = facade;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.IsEnabled = true;
            timer.Tick += Timer_Tick;
            stopwatch = new Stopwatch();
        }

        void Timer_Tick(object sender, EventArgs e) {
            var time = stopwatch.Elapsed.TotalSeconds;
            if (time > 1) {
                facade.HideAxis(showedAxis);
                showedAxis = Vector3.Zero;
                stopwatch.Stop();
                stopwatch.Reset();
            }           
        }

        public void Reset() {
            XAxis.Reset();
        }
        public void Move(float step, Vector3 axis) {
            facade.Move(step, axis);
        }
        public void ShowAxis(Vector3 axis) {
            if (showedAxis != axis) {
                facade.HideAxis(showedAxis);
                facade.ShowAxis(axis);
                showedAxis = axis;
            }
            stopwatch.Restart();
        }
    }

    class TransformModuleViewModel : BaseNotify, IActionModule {
        readonly ISelectedObjectTransformation selectedObject;

        public TransformTranslate Translate { get; }

        public ICommand Reset { get; }

        Matrix4x4 history;
       

        public TransformModuleViewModel(ISelectedObjectTransformation loaded) {
            Translate = new TransformTranslate(this);
            this.selectedObject = loaded;
            history = Matrix4x4.Identity;

            Reset = new WpfActionCommand(OnReset);
        }

        void OnReset() {
            selectedObject.Transform(history.Inverted());
            history = Matrix4x4.Identity;
            Translate.Reset();
        }

        public void Move(float step, Vector3 axis) {
            var move = Matrix4x4.CreateTranslation(axis * step);
            history *= move;
            selectedObject.Transform(move);
        }
        public void Rotate(float step, Vector3 axis) {

        }

        public void ShowAxis(Vector3 axis) {
            selectedObject.ShowTransformationAxis(axis);
        }
        public void HideAxis(Vector3 axis) {
            selectedObject.HideTransformationAxis(axis);
        }
    }
}
