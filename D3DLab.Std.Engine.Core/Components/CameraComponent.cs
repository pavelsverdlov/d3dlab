using D3DLab.Std.Engine.Core.Ext;
using System;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Components {


    public class CameraComponent1 : GraphicComponent {
        private Vector3 _position;
        private readonly float _moveSpeed = 10.0f;

        private float _yaw;
        private float _pitch;

        private Vector2 _previousMousePos;
        public float Width { get; set; }

        public float VWidth { get; set; }
        public float VHeight { get; set; }
        public float Scale { get; set; }

        public Vector3 Position { get => _position; set { _position = value; } }
        public Vector3 LookDirection { get; set; }
        public Vector3 UpDirection { get; set; }
        public Vector3 RotatePoint { get; set; }
        public Matrix4x4 ViewMatrix { get; private set; }
        public Matrix4x4 ProjectionMatrix { get; private set; }
        public float FarDistance { get; }
        public float FieldOfView { get; }
        public float NearDistance { get; }
        public float AspectRatio => VWidth / VHeight;
        public float Yaw { get => _yaw; set { _yaw = value; } }
        public float Pitch { get => _pitch; set { _pitch = value; } }

        public CameraComponent1(float width, float height) {
            Width = 70;

            _position = Vector3.UnitZ * Width * 2f;
            FarDistance = 1000f;
            FieldOfView = 1f;
            NearDistance = 1f;



            RotatePoint = Vector3.Zero;
            UpDirection = Vector3.UnitY;
            LookDirection = new Vector3(0, 0, -1);
            VWidth = width;

            VHeight = height;
            Scale = 1;

            //Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            //Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            //LookDirection = lookDir;

            UpdatePerspectiveMatrix(width, height);
            UpdateViewMatrix();
        }

        public void WindowResized(float width, float height) {
            VWidth = width;
            VHeight = height;
            UpdatePerspectiveMatrix(width, height);
        }

        public void UpdatePerspectiveMatrix(float width, float height) {
            var aspectRatio = width / height;
            //  ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, width / height, NearDistance, FarDistance);

            float halfWidth = (float)(Width * 0.5f);
            float halfHeight = (float)((this.Width / aspectRatio) * 0.5f);

            ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(-halfWidth * Scale, halfWidth * Scale, -halfHeight * Scale, halfHeight * Scale,
                NearDistance, FarDistance);
        }

        protected void UpdatePerspectiveMatrix() {
            UpdatePerspectiveMatrix(VWidth, VHeight);
        }

        public void UpdateViewMatrix() {
            ViewMatrix = Matrix4x4.CreateLookAt(_position, _position + LookDirection, UpDirection);
        }

        /// <summary>
        /// Converts a screen space point into a corresponding point in world space.
        /// </summary>
        /// <param name="source">The vector to project.</param>
        /// <param name="matrix">An inverted combined WorldViewProjection matrix.</param>
        /// <param name="vector">The unprojected vector.</param>
        public void Unproject(ref Vector3 source, out Vector3 vector) {
            float X = 0;
            float Y = 0;
            var MinDepth = NearDistance;
            var MaxDepth = FarDistance;
            var matrix = ViewMatrix * ProjectionMatrix;
            vector.X = (((source.X - X) / (VWidth)) * 2f) - 1f;
            vector.Y = -((((source.Y - Y) / (VWidth)) * 2f) - 1f);
            vector.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);

            float a = (((vector.X * matrix.M14) + (vector.Y * matrix.M24)) + (vector.Z * matrix.M34)) + matrix.M44;
            vector = Vector3.Transform(vector, matrix);

            if (!MathUtil.IsOne(a)) {
                vector = (vector / a);
            }
        }

        public override string ToString() {
            return $"Pos:{_position}; LoockDirection:{LookDirection}; Scale:{Scale}";
        }

    }


    public class CameraComponent : OrthographicCameraComponent {
        public Vector3 RotatePoint { get; set; }

        public CameraComponent() {
            ResetToDefault();
        }

        public void ResetToDefault() {
            UpDirection = Vector3.UnitY;
            Width = 35f;
            FieldOfViewRadians = 1.05f;
            NearPlaneDistance = 0.01f;
            LookDirection = ForwardRH;
            Position = Vector3.UnitZ * Width * 10f;

            FarPlaneDistance = Position.Length() * 5;

            scale = 1;
        }
    }

    public class PerspectiveCameraComponent : GeneralCameraComponent {
       

        public PerspectiveCameraComponent() {
            FieldOfViewRadians = 1.05f;
        }

        public override Matrix4x4 UpdateProjectionMatrix(float width, float height) {
            float aspectRatio = width / height;

            var projection = Matrix4x4.CreatePerspectiveFieldOfView(
                        FieldOfViewRadians,
                        aspectRatio,
                        NearPlaneDistance,
                        FarPlaneDistance);



            return projection;
        }

        public override void Zoom(float delta) {
            
        }
    }

    public class OrthographicCameraComponent : GeneralCameraComponent {
        public float Width { get; set; }

        protected float scale;
        float prevScreenWidth;
        float prevScreenHeight;

        public OrthographicCameraComponent() {
            scale = 1;
        }

        public override Matrix4x4 UpdateProjectionMatrix(float width, float height) {
            this.prevScreenWidth = width;
            this.prevScreenHeight = height;
            float aspectRatio = width / height;

            var frameWidth = Width * scale;
            ProjectionMatrix = Matrix4x4.CreateOrthographic(
                        frameWidth,
                        frameWidth / aspectRatio,
                        NearPlaneDistance,
                        FarPlaneDistance);
            return ProjectionMatrix;
        }

        public void ZoomTo(float delta, Vector2 screen) {
            var ray = GetRayFromScreenPoint(screen);
            var prevWidth = Width * scale;

            Zoom(delta);

            var ortoWidth = Width * scale;
            var d = ortoWidth / prevWidth;

            var screenX = screen.X;
            var screenY = screen.Y;

            var PanK = prevWidth / prevScreenWidth;
            var p1 = new Vector2(screenX * PanK, screenY * PanK);
            var p0 = new Vector2(prevScreenWidth * 0.5f * PanK, prevScreenHeight * 0.5f * PanK);

            var pan = (p1 - p0) * (d - 1);

            Pan1(pan);
        }
        public override void Zoom(float delta) {
            var newscale = scale + (delta * 0.01f);
            if (newscale > 0) {
                scale = newscale;
            }
        }
        public void Pan(Vector2 move) {
            var PanK = (Width * scale) / prevScreenWidth;
            var p1 = new Vector2(move.X * PanK, move.Y * PanK);
            
            var left = Vector3.Cross(UpDirection, LookDirection);
            left.Normalized();

            var panVector = left * p1.X + UpDirection * p1.Y;
            Position += panVector;
        }
        public void Pan1(Vector2 screen) {
            var kx = screen.X;
            var ky =  screen.Y;

            var left = Vector3.Cross(UpDirection, LookDirection);
            left.Normalized();

            var panVector = left * kx + UpDirection * ky;

            Position += panVector;
        }

        static Vector3 Project(Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix4x4 worldViewProjection) {
            var v = Vector3.Transform( vector, worldViewProjection);
            return new Vector3(((1.0f + v.X) * 0.5f * width) + x, ((1.0f - v.Y) * 0.5f * height) + y, (v.Z * (maxZ - minZ)) + minZ);
        }
        
        static Vector3 Unproject(Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix4x4 worldViewProjection) {
            Vector3 v = new Vector3();
            Matrix4x4 matrix = new Matrix4x4();
            Matrix4x4.Invert(worldViewProjection, out matrix);

            v.X = (((vector.X - x) / width) * 2.0f) - 1.0f;
            v.Y = -((((vector.Y - y) / height) * 2.0f) - 1.0f);
            v.Z = (vector.Z - minZ) / (maxZ - minZ);

            return Vector3.Transform(v, matrix);
        }

        public Veldrid.Utilities.Ray GetRayFromScreenPoint(Vector2 screen) {
            var screenX = screen.X;
            var screenY = screen.Y;
            var scaleFactor = new Vector2(1, 1);
            // Normalized Device Coordinates Top-Left (-1, 1) to Bottom-Right (1, -1)
            float x = (2.0f * screenX) / (prevScreenWidth / scaleFactor.X) - 1.0f;
            float y = 1.0f - (2.0f * screenY) / (prevScreenHeight / scaleFactor.Y);
            float z = 1.0f;
            Vector3 deviceCoords = new Vector3(x, y, z);

            // Clip Coordinates
            Vector4 clipCoords = new Vector4(deviceCoords.X, deviceCoords.Y, -1.0f, 1.0f);

            // View Coordinates
            Matrix4x4 invProj;
            Matrix4x4.Invert(ProjectionMatrix, out invProj);
            Vector4 viewCoords = Vector4.Transform(clipCoords, invProj);
            viewCoords.Z = -1.0f;
            viewCoords.W = 0.0f;

            Matrix4x4 invView;
            Matrix4x4.Invert(ViewMatrix, out invView);
            Vector3 worldCoords = Vector4.Transform(viewCoords, invView).XYZ();
            worldCoords = Vector3.Normalize(worldCoords);

            return new Veldrid.Utilities.Ray(Position, worldCoords);
        }
    }

    public abstract class GeneralCameraComponent : GraphicComponent {
        protected bool COORDINATE_SYSTEM_LH = false;

        // A unit Vector3 designating forward in a left-handed coordinate system
        public static readonly Vector3 ForwardLH = new Vector3(0, 0, 1);
        //A unit Vector3 designating forward in a right-handed coordinate system
        public static readonly Vector3 ForwardRH = new Vector3(0, 0, -1);

        public float FieldOfViewRadians { get; set; }
        public Vector3 Position { get; set; }
        public float NearPlaneDistance { get; set; }
        public Vector3 LookDirection { get; set; }
        public Vector3 UpDirection { get; set; }
        public float FarPlaneDistance { get; set; }

        public Matrix4x4 ViewMatrix { get; protected set; }
        public Matrix4x4 ProjectionMatrix { get; protected set; }

        protected GeneralCameraComponent() {

        }

        public Matrix4x4 UpdateViewMatrix() {
            ViewMatrix =  Matrix4x4.CreateLookAt(Position, Position + LookDirection, UpDirection);
            return ViewMatrix;
        }
        public abstract Matrix4x4 UpdateProjectionMatrix(float width, float height);

        public abstract void Zoom(float delta);

      
    }
}
