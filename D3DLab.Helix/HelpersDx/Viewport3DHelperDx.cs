using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixToolkit.Wpf.SharpDX.HelpersDx {
    public static class Viewport3DHelperDx {
    //    /// <summary>
    //    /// Finds the hits for the specified position.
    //    /// </summary>
    //    /// <param name="viewport">
    //    /// The viewport.
    //    /// </param>
    //    /// <param name="position">
    //    /// The position.
    //    /// </param>
    //    /// <returns>
    //    /// List of hits, sorted with the nearest hit first.
    //    /// </returns>
    //    public static IList<Viewport3DHelper.HitResult> FindHits(this Viewport3DX viewport, Point position) {
    //        var camera = viewport.Camera as ProjectionCamera;
    //        if (camera == null) {
    //            return null;
    //        }

    //        var result = new List<Viewport3DHelper.HitResult>();
    //        HitTestResultCallback callback = hit => {
    //                var rayHit = hit as RayMeshGeometry3DHitTestResult;
    //                if (rayHit != null) {
    //                    if (rayHit.MeshHit != null) {
    //                        var p = GetGlobalHitPosition(rayHit, viewport);
    //                        var nn = GetNormalHit(rayHit);
    //                        var n = nn.HasValue ? nn.Value : new Vector3D(0, 0, 1);

    //                        result.Add(
    //                            new Viewport3DHelper.HitResult {
    //                                Distance = (camera.Position - p).Length,
    //                                RayHit = rayHit,
    //                                Normal = n,
    //                                Position = p
    //                            });
    //                    }
    //                }

    //                return HitTestResultBehavior.Continue;
    //            };

    //        var htp = new PointHitTestParameters(position);
    //        VisualTreeHelper.HitTest(viewport, null, callback, htp);

    //        return result.OrderBy(k => k.Distance).ToList();
    //    }

    //    /// <summary>
    //    /// Gets the hit position transformed to global (viewport) coordinates.
    //    /// </summary>
    //    /// <param name="rayHit">
    //    /// The hit structure.
    //    /// </param>
    //    /// <param name="viewport">
    //    /// The viewport.
    //    /// </param>
    //    /// <returns>
    //    /// The 3D position of the hit.
    //    /// </returns>
    //    private static Point3D GetGlobalHitPosition(RayHitTestResult rayHit, Viewport3DX viewport) {
    //        // PointHit is in Visual3D space
    //        var p = rayHit.PointHit;

    //        // transform the Visual3D hierarchy up to the Viewport3D ancestor
    //        var t = GetTransform(viewport, rayHit.VisualHit);
    //        if (t != null) {
    //            p = t.Transform(p);
    //        }

    //        return p;
    //    }
    //    /// <summary>
    //    /// Gets the normal for a hit test result.
    //    /// </summary>
    //    /// <param name="rayHit">
    //    /// The ray hit.
    //    /// </param>
    //    /// <returns>
    //    /// The normal.
    //    /// </returns>
    //    private static Vector3D? GetNormalHit(RayMeshGeometry3DHitTestResult rayHit) {
    //        if ((rayHit.MeshHit.Normals == null) || (rayHit.MeshHit.Normals.Count < 1)) {
    //            return null;
    //        }

    //        return (rayHit.MeshHit.Normals[rayHit.VertexIndex1] * rayHit.VertexWeight1)
    //               + (rayHit.MeshHit.Normals[rayHit.VertexIndex2] * rayHit.VertexWeight2)
    //               + (rayHit.MeshHit.Normals[rayHit.VertexIndex3] * rayHit.VertexWeight3);
    //    }

    //    /// <summary>
    //    /// Gets the total transform of the specified visual.
    //    /// </summary>
    //    /// <param name="viewport">The viewport.</param>
    //    /// <param name="visual">The visual.</param>
    //    /// <returns>The transform.</returns>
    //    public static GeneralTransform3D GetTransform(this Viewport3DX viewport, Visual3D visual) {
    //        if (visual == null) {
    //            return null;
    //        }

    //        foreach (Visual3D ancestor in viewport.Items) {
    //            if (visual.IsDescendantOf(ancestor)) {
    //                var g = new GeneralTransform3DGroup();

    //                // this includes the visual.Transform
    //                var ta = visual.TransformToAncestor(ancestor);
    //                if (ta != null) {
    //                    g.Children.Add(ta);
    //                }

    //                // add the transform of the top-level ancestor
    //                g.Children.Add(ancestor.Transform);

    //                return g;
    //            }
    //        }

    //        return visual.Transform;
    //    }
    }
}
