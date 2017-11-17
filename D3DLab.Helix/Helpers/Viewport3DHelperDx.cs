// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Viewport3DHelper.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace HelixToolkit.Wpf.SharpDX.Helpers
{
    /// <summary>
    /// Provides extension methods for <see cref="Viewport3D"/>.
    /// </summary>
    /// <remarks>
    /// See "3D programming for Windows" (Charles Petzold book) and <a hef="http://www.ericsink.com/wpf3d/index.html">Twelve Days of WPF 3D</a>.
    /// </remarks>
    public static class Viewport3DHelperDx
    {
        /// <summary>
        /// Gets the camera transform.
        /// </summary>
        /// <param name="viewport3DVisual">The viewport visual.</param>
        /// <returns>The camera transform.</returns>
        public static Matrix3D GetCameraTransform(this Viewport3DVisual viewport3DVisual)
        {
            return viewport3DVisual.Camera.GetTotalTransform(viewport3DVisual.Viewport.Size.Width / viewport3DVisual.Viewport.Size.Height);
        }

        /// <summary>
        /// Gets the camera transform (viewport and projection).
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <returns>
        /// A <see cref="Matrix3D"/>.
        /// </returns>
        public static Matrix3D GetCameraTransform(this Viewport3DX viewport)
        {
            var m = Matrix3D.Identity;
            m.Append(viewport.Camera.CreateViewMatrix().ToMatrix3D());
            m.Append(viewport.Camera.CreateProjectionMatrix(viewport.ActualWidth / viewport.ActualHeight).ToMatrix3D());
            return m;
            //     return viewport.Camera.GetTotalTransform(viewport.ActualWidth / viewport.ActualHeight);
        }

        /// <summary>
        /// Gets the ray at the specified position.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="position">
        /// A 2D point.
        /// </param>
        /// <returns>
        /// A <see cref="Wpf.Ray3D"/>.
        /// </returns>
        public static Ray3D GetRay(this Viewport3DX viewport, Point position)
        {
            Point3D point1, point2;
            bool ok = Point2DtoPoint3D(viewport, position, out point1, out point2);
            if (!ok)
            {
                return null;
            }

            return new Ray3D { Origin = point1, Direction = point2 - point1 };
        }

        /// <summary>
        /// Gets the total transform (camera and viewport).
        /// </summary>
        /// <param name="viewport3DVisual">The viewport visual.</param>
        /// <returns>The total transform.</returns>
        public static Matrix3D GetTotalTransform(this Viewport3DVisual viewport3DVisual)
        {
            var m = GetCameraTransform(viewport3DVisual);
            m.Append(GetViewportTransform(viewport3DVisual));
            return m;
        }

        /// <summary>
        /// Gets the total transform (camera and viewport).
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>The total transform.</returns>
        public static Matrix3D GetTotalTransform(this Viewport3DX viewport)
        {
            var transform = GetCameraTransform(viewport);
            transform.Append(GetViewportTransform(viewport));
            return transform;
        }

        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>A <see cref="Matrix3D"/>.</returns>
        public static Matrix3D GetViewMatrix(this Viewport3DX viewport)
        {
            return viewport.Camera.GetViewMatrix3D();
        }

        /// <summary>
        /// Gets the viewport transform.
        /// </summary>
        /// <param name="viewport3DVisual">The viewport3DVisual.</param>
        /// <returns>The transform.</returns>
        public static Matrix3D GetViewportTransform(this Viewport3DVisual viewport3DVisual)
        {
            return new Matrix3D(
                viewport3DVisual.Viewport.Width / 2,
                0,
                0,
                0,
                0,
                -viewport3DVisual.Viewport.Height / 2,
                0,
                0,
                0,
                0,
                1,
                0,
                viewport3DVisual.Viewport.X + (viewport3DVisual.Viewport.Width / 2),
                viewport3DVisual.Viewport.Y + (viewport3DVisual.Viewport.Height / 2),
                0,
                1);
        }

        /// <summary>
        /// Gets the viewport transform.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <returns>The transform.</returns>
        public static Matrix3D GetViewportTransform(this Viewport3DX viewport)
        {
            return new Matrix3D(
                viewport.ActualWidth / 2,
                0,
                0,
                0,
                0,
                -viewport.ActualHeight / 2,
                0,
                0,
                0,
                0,
                1,
                0,
                viewport.ActualWidth / 2,
                viewport.ActualHeight / 2,
                0,
                1);
        }

        /// <summary>
        /// Transforms a position to Point3D at the near and far clipping planes.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="pointIn">The point to transform.</param>
        /// <param name="pointNear">The point at the near clipping plane.</param>
        /// <param name="pointFar">The point at the far clipping plane.</param>
        /// <returns>True if points were found.</returns>
        public static bool Point2DtoPoint3D(this Viewport3DX viewport, Point pointIn, out Point3D pointNear, out Point3D pointFar)
        {
            pointNear = new Point3D();
            pointFar = new Point3D();

            var pointIn3D = new Point3D(pointIn.X, pointIn.Y, 0);
            var matrixViewport = GetViewportTransform(viewport);
            var matrixCamera = GetCameraTransform(viewport);

            if (!matrixViewport.HasInverse)
            {
                return false;
            }

            if (!matrixCamera.HasInverse)
            {
                return false;
            }

            matrixViewport.Invert();
            matrixCamera.Invert();

            var pointNormalized = matrixViewport.Transform(pointIn3D);
            pointNormalized.Z = 0.01;
            pointNear = matrixCamera.Transform(pointNormalized);
            pointNormalized.Z = 0.99;
            pointFar = matrixCamera.Transform(pointNormalized);

            return true;
        }

        /// <summary>
        /// Transforms a 2D point to a ray.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="pointIn">The point.</param>
        /// <returns>The ray.</returns>
        public static Ray3D Point2DtoRay3D(this Viewport3DX viewport, Point pointIn)
        {
            Point3D pointNear, pointFar;
            if (!Point2DtoPoint3D(viewport, pointIn, out pointNear, out pointFar))
            {
                return null;
            }

            return new Ray3D(pointNear, pointFar);
        }

        /// <summary>
        /// Transforms the Point3D to a Point2D.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="point">The 3D point.</param>
        /// <returns>The point.</returns>
        public static Point Point3DtoPoint2D(this Viewport3DX viewport, Point3D point)
        {
            var matrix = GetTotalTransform(viewport);
            var pointTransformed = matrix.Transform(point);
            var pt = new Point(pointTransformed.X, pointTransformed.Y);
            return pt;
        }

        /// <summary>
        /// Recursive search in a Visual3D collection for objects of given type T
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>A list of models.</returns>
        public static IList<System.Windows.Media.Media3D.Model3D> SearchFor<T>(this IEnumerable<Visual3D> collection)
        {
            var output = new List<System.Windows.Media.Media3D.Model3D>();
            SearchFor(collection, typeof(T), output);
            return output;
        }

        /// <summary>
        /// Transforms a point from the screen (2D) to a point on plane (3D)
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="p">
        /// The 2D point.
        /// </param>
        /// <param name="position">
        /// A point in the plane.
        /// </param>
        /// <param name="normal">
        /// The plane normal.
        /// </param>
        /// <returns>
        /// A 3D point.
        /// </returns>
        /// <remarks>
        /// Maps window coordinates to object coordinates like <code>gluUnProject</code>.
        /// </remarks>
   
        public static Point3D? UnProject(this Viewport3DX viewport, Point p, Point3D position, Vector3D normal)
        {
            var rayTmp = viewport.UnProjectToRay(p);
            var ray = new Ray3D(rayTmp.Origin,rayTmp.Direction);
            if (ray == null) {
                return null;
            }

            return ray.PlaneIntersection(position, normal);
        }

        /// <summary>
        /// Transforms a point from the screen (2D) to a point on the plane trough the camera target point.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="p">
        /// The 2D point.
        /// </param>
        /// <returns>
        /// A 3D point.
        /// </returns>
        public static Point3D? UnProjectPoint(this Viewport3DX viewport, Point p)
        {
            var pc = viewport.Camera;
            if (pc == null)
            {
                return null;
            }

            return UnProject(viewport, p, pc.Position + pc.LookDirection, pc.LookDirection);
        }

        /// <summary>
        /// Copies the bitmap.
        /// </summary>
        /// <param name="source">The source bitmap.</param>
        /// <param name="target">The target bitmap.</param>
        /// <param name="x">The x offset.</param>
        /// <param name="y">The y offset.</param>
        private static void CopyBitmap(BitmapSource source, WriteableBitmap target, int x, int y)
        {
            // Calculate stride of source
            int stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);

            // Create data array to hold source pixel data
            var data = new byte[stride * source.PixelHeight];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            // Write the pixel data to the bitmap.
            target.WritePixels(new Int32Rect(x, y, source.PixelWidth, source.PixelHeight), data, stride, 0);
        }

        /// <summary>
        /// Gets the normal for a hit test result.
        /// </summary>
        /// <param name="rayHit">
        /// The ray hit.
        /// </param>
        /// <returns>
        /// The normal.
        /// </returns>
        private static Vector3D? GetNormalHit(RayMeshGeometry3DHitTestResult rayHit)
        {
            if ((rayHit.MeshHit.Normals == null) || (rayHit.MeshHit.Normals.Count < 1))
            {
                return null;
            }

            return (rayHit.MeshHit.Normals[rayHit.VertexIndex1] * rayHit.VertexWeight1)
                   + (rayHit.MeshHit.Normals[rayHit.VertexIndex2] * rayHit.VertexWeight2)
                   + (rayHit.MeshHit.Normals[rayHit.VertexIndex3] * rayHit.VertexWeight3);
        }

        /// <summary>
        /// Recursive search for an object of a given type
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="type">The type.</param>
        /// <param name="output">The output.</param>
        private static void SearchFor(IEnumerable<Visual3D> collection, Type type, IList<System.Windows.Media.Media3D.Model3D> output)
        {
            // TODO: change to use Stack/Queue
            foreach (var visual in collection)
            {
                var modelVisual = visual as ModelVisual3D;
                if (modelVisual != null)
                {
                    var model = modelVisual.Content;
                    if (model != null)
                    {
                        if (type.IsInstanceOfType(model))
                        {
                            output.Add(model);
                        }

                        // recursive
                        SearchFor(modelVisual.Children, type, output);
                    }

                    var modelGroup = model as Model3DGroup;
                    if (modelGroup != null)
                    {
                        SearchFor(modelGroup.Children, type, output);
                    }
                }
            }
        }

        /// <summary>
        /// Searches for models of the specified type.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        private static void SearchFor(IEnumerable<System.Windows.Media.Media3D.Model3D> collection, Type type, IList<System.Windows.Media.Media3D.Model3D> output)
        {
            foreach (var model in collection)
            {
                if (type.IsInstanceOfType(model))
                {
                    output.Add(model);
                }

                var group = model as Model3DGroup;
                if (group != null)
                {
                    SearchFor(group.Children, type, output);
                }
            }
        }

        /// <summary>
        /// A hit result.
        /// </summary>
        public class HitResult
        {
            /// <summary>
            /// Gets or sets the distance.
            /// </summary>
            /// <value>The distance.</value>
            public double Distance { get; set; }

            /// <summary>
            /// Gets the mesh.
            /// </summary>
            /// <value>The mesh.</value>
            public System.Windows.Media.Media3D.MeshGeometry3D Mesh
            {
                get
                {
                    return this.RayHit.MeshHit;
                }
            }

            /// <summary>
            /// Gets the model.
            /// </summary>
            /// <value>The model.</value>
            public System.Windows.Media.Media3D.Model3D Model
            {
                get
                {
                    return this.RayHit.ModelHit;
                }
            }

            /// <summary>
            /// Gets or sets the normal.
            /// </summary>
            /// <value>The normal.</value>
            public Vector3D Normal { get; set; }

            /// <summary>
            /// Gets or sets the position.
            /// </summary>
            /// <value>The position.</value>
            public Point3D Position { get; set; }

            /// <summary>
            /// Gets or sets the ray hit.
            /// </summary>
            /// <value>The ray hit.</value>
            public RayMeshGeometry3DHitTestResult RayHit { get; set; }

            /// <summary>
            /// Gets the visual.
            /// </summary>
            /// <value>The visual.</value>
            public Visual3D Visual
            {
                get
                {
                    return this.RayHit.VisualHit;
                }
            }
        }
    }
}