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
    abstract class TransformationByAxis : BaseNotify {
        readonly Vector3 axis;
        readonly CommonTransformation facade;
        float moved;
        float step;

        public float Moved {
            get => moved;
            set {
                facade.ResetTransform();
                facade.Transform(value, axis);
                Update(ref moved, value, nameof(Moved));
            }
        }
        public float Step { get => step; set => Update(ref step, value, nameof(Step)); }

        public ICommand ForwardCommand { get; }
        public ICommand BackwardCommand { get; }
        public ICommand MouseMoveCommand { get; }

        public TransformationByAxis(Vector3 axis, CommonTransformation facade) {
            Step = 0.1f;
            this.axis = axis;
            this.facade = facade;
            ForwardCommand = new WpfActionCommand(OnForward);
            BackwardCommand = new WpfActionCommand(OnBackward);
            MouseMoveCommand = new WpfActionCommand(OnMouseMove);
        }

        //abstract protected void OnMoved(float value, Vector3 axis);

        void OnMouseMove() {
            facade.ShowAxis(axis);
        }

        void OnForward() {
            facade.Transform(Step, axis);
            moved += Step;
            SetPropertyChanged(nameof(Moved));
        }

        void OnBackward() {
            facade.Transform(Step, -axis);
            moved -= Step;
            SetPropertyChanged(nameof(Moved));
        }

        public void Reset() {
            Moved = 0;
        }

    }
    abstract class CommonTransformation {
        public AxisTranslateTransform XAxis { get; set; }
        public AxisTranslateTransform YAxis { get; set; }
        public AxisTranslateTransform ZAxis { get; set; }

        public float OriginX {
            get => originX;
            set {
                originX = value;
            }
        }
        public float OriginY {
            get => originY;
            set {
                originY = value;
            }
        }
        public float OriginZ {
            get => originZ;
            set {
                originZ = value;
            }
        }

        Vector3 showedAxis;

        float originX;
        float originY;
        float originZ;

        readonly DispatcherTimer timer;
        readonly Stopwatch stopwatch;
        public CommonTransformation() {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.IsEnabled = true;
            timer.Tick += OnTimer_Tick;
            stopwatch = new Stopwatch();
        }

        void OnTimer_Tick(object? sender, EventArgs e) {
            var time = stopwatch.Elapsed.TotalSeconds;
            if (time > 1) {
                OnHideAxis(showedAxis);
                showedAxis = Vector3.Zero;
                stopwatch.Stop();
                stopwatch.Reset();
            }
        }

        public abstract void ResetTransform();
        public abstract void Transform(float value, Vector3 axis);
        public void ShowAxis(Vector3 axis) {
            if (showedAxis != axis) {
                OnHideAxis(showedAxis);
                OnShowAxis(axis);
                showedAxis = axis;
            }
            stopwatch.Restart();
        }
        protected abstract void OnShowAxis(Vector3 show);
        protected abstract void OnHideAxis(Vector3 hide);
    }

    class AxisTranslateTransform : TransformationByAxis {
        public AxisTranslateTransform(Vector3 axis, TransformTranslate facade) : base(axis,facade){
            Step = 0.1f;
        }
    }
    
    class TransformTranslate : CommonTransformation {
        readonly TransformModuleViewModel facade;
        public TransformTranslate(TransformModuleViewModel facade) {
            XAxis = new AxisTranslateTransform(Vector3.UnitX, this);
            YAxis = new AxisTranslateTransform(Vector3.UnitY, this);
            ZAxis = new AxisTranslateTransform(Vector3.UnitZ, this);
            this.facade = facade;
        }

        public override void ResetTransform() {
            facade.ResetTransform();
        }
        public void Reset() {
            XAxis.Reset();
        }
        public override void Transform(float step, Vector3 axis) {
            facade.Move(step, axis);
        }
        protected override void OnShowAxis(Vector3 show) {
            facade.ShowAxis(show);
        }
        protected override void OnHideAxis( Vector3 hide) {
            facade.HideAxis(hide);
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
            ResetTransform();
            Translate.Reset();
        }
        public void ResetTransform() {
            selectedObject.Transform(history.Inverted());
            history = Matrix4x4.Identity;
        }
        public void Move(float step, Vector3 axis) {
            var move = Matrix4x4.CreateTranslation(axis * step);
            history *= move;
            selectedObject.Transform(move);
        }
        public void Rotate(float value, Vector3 axis) {

        }

        public void ShowAxis(Vector3 axis) {
            selectedObject.ShowTransformationAxis(axis);
        }
        public void HideAxis(Vector3 axis) {
            selectedObject.HideTransformationAxis(axis);
        }
    }
}
