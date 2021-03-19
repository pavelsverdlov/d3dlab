using D3DLab.ECS.Ext;

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
                facade.Transform(value - moved, axis);
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
            Update(ref moved, 0, nameof(Moved));
        }

    }
    abstract class CommonTransformation : BaseNotify {
        public TransformationByAxis XAxis { get; set; }
        public TransformationByAxis YAxis { get; set; }
        public TransformationByAxis ZAxis { get; set; }

        Vector3 showedAxis;
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
        public AxisTranslateTransform(Vector3 axis, TransformTranslate facade) : base(axis, facade) {
            Step = 0.1f;
        }
    }

    class TransformTranslate : CommonTransformation {
        public ICommand ResetToWorldCommand { get; }
        public ICommand SetObjectCenterCommand { get; }

        public float OriginX {
            get => originX;
            set {
                originX = value;
                XAxis.Moved = value;
            }
        }
        public float OriginY {
            get => originY;
            set {
                originY = value;
                YAxis.Moved = value;
            }
        }
        public float OriginZ {
            get => originZ;
            set {
                originZ = value;
                ZAxis.Moved = value;
            }
        }

        float originX;
        float originY;
        float originZ;
        Vector3? objectCenter;
        readonly TransformModuleViewModel facade;

        public TransformTranslate(TransformModuleViewModel facade) {
            XAxis = new AxisTranslateTransform(Vector3.UnitX, this);
            YAxis = new AxisTranslateTransform(Vector3.UnitY, this);
            ZAxis = new AxisTranslateTransform(Vector3.UnitZ, this);
            this.facade = facade;

            ResetToWorldCommand = new WpfActionCommand(OnResetToWorldCenter);
            SetObjectCenterCommand = new WpfActionCommand(OnSetObjectCenter);

            OnSetObjectCenter();
        }
        void SetOrigin(Vector3 v) {
            Update(ref originX, (float)Math.Round(v.X, 5), nameof(OriginX));
            Update(ref originY, (float)Math.Round(v.Y, 5), nameof(OriginY));
            Update(ref originZ, (float)Math.Round(v.Z, 5), nameof(OriginZ));
        }
        void OnSetObjectCenter() {
            var v = facade.GetObjectCenter();
            SetOrigin(v);
            objectCenter = v;
        }
        void OnResetToWorldCenter() {
            facade.Transfrom(
                  Matrix4x4.CreateTranslation(Vector3.UnitX * -originX)
                * Matrix4x4.CreateTranslation(Vector3.UnitY * -originY)
                * Matrix4x4.CreateTranslation(Vector3.UnitZ * -originZ)
                );
            SetOrigin(Vector3.Zero);
            objectCenter = Vector3.Zero;
            Reset();
        }
        public void UpdateOrigin(Matrix4x4 matrix) {
            if (objectCenter.HasValue) {
                objectCenter = Vector3.Transform(objectCenter.Value, matrix);
                SetOrigin(objectCenter.Value);
            }
        }

        public void Reset() {
            XAxis.Reset();
            YAxis.Reset();
            ZAxis.Reset();
        }
        public override void Transform(float step, Vector3 axis) {
            facade.Move(step, axis);
        }
        protected override void OnShowAxis(Vector3 show) {
            facade.ShowAxis(show);
        }
        protected override void OnHideAxis(Vector3 hide) {
            facade.HideAxis(hide);
        }
    }

    class AxisRotateTransform : TransformationByAxis {
        public AxisRotateTransform(Vector3 axis, TransformRotate facade) : base(axis, facade) {
            Step = 10;
        }
    }
    class TransformRotate : CommonTransformation {
        readonly TransformModuleViewModel facade;

        public TransformRotate(TransformModuleViewModel facade) {
            XAxis = new AxisRotateTransform(Vector3.UnitX, this);
            YAxis = new AxisRotateTransform(Vector3.UnitY, this);
            ZAxis = new AxisRotateTransform(Vector3.UnitZ, this);
            this.facade = facade;
        }

        public override void Transform(float value, Vector3 axis) {
            throw new NotImplementedException();
        }

        protected override void OnHideAxis(Vector3 hide) {
            throw new NotImplementedException();
        }

        protected override void OnShowAxis(Vector3 show) {
            throw new NotImplementedException();
        }
    }

    class TransformModuleViewModel : BaseNotify, IActionModule {
        readonly ISelectedObjectTransformation selectedObject;

        public TransformTranslate Translate { get; }

        public ICommand Reset { get; }
        public ICommand PreviewCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ApplyCommand { get; }


        Matrix4x4 history;

        bool isPreviewing;
        bool isApplied;
        public TransformModuleViewModel(ISelectedObjectTransformation loaded) {
            isPreviewing = true;
            this.selectedObject = loaded;
            history = Matrix4x4.Identity;

            Translate = new TransformTranslate(this);

            Reset = new WpfActionCommand(OnReset);
            ApplyCommand = new WpfActionCommand(OnApply);
            CloseCommand = new WpfActionCommand(OnClose);
            PreviewCommand = new WpfActionCommand<bool>(OnPreview);          
        }

        void OnApply() {
            selectedObject.Transform(history);
            isApplied = true;
            history = Matrix4x4.Identity;
        }

        void OnClose() {
            if (!isApplied && isPreviewing) {
                selectedObject.Transform(history.Inverted());
            }

            selectedObject.Finish();
        }

        void OnPreview(bool ischeked) {
            isPreviewing = ischeked;
            if (isPreviewing) {
                selectedObject.Transform(history);
            } else {
                selectedObject.Transform(history.Inverted());
            }
        }

        void OnReset() {
            ResetTransform();
            Translate.Reset();
        }
        public void ResetTransform() {
            var matrix = history.Inverted();
            if (isPreviewing) {
                selectedObject.Transform(matrix);
            }
            history = Matrix4x4.Identity;
            Translate.UpdateOrigin(matrix);
        }
        public void Move(float step, Vector3 axis) {
            var move = Matrix4x4.CreateTranslation(axis * step);
            history *= move;
            if (isPreviewing) {
                selectedObject.Transform(move);
            }
            Translate.UpdateOrigin(move);
        }
        public void Transfrom(Matrix4x4 matrix) {
            history *= matrix;
            if (isPreviewing) {
                selectedObject.Transform(matrix);
            }
            Translate.UpdateOrigin(matrix);
        }

        public void Rotate(float value, Vector3 axis) {

        }

        public void ShowAxis(Vector3 axis) {
            selectedObject.ShowTransformationAxis(axis);
        }
        public void HideAxis(Vector3 axis) {
            selectedObject.HideTransformationAxis(axis);
        }

        public Vector3 GetObjectCenter() {
            return selectedObject.GetCenter();
        }
    }
}
