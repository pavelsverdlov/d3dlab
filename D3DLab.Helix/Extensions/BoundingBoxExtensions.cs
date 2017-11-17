using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace HelixToolkit.Wpf.SharpDX.Extensions {
    public static class BoundingBoxExtensions {
        public static double SizeX(this BoundingBox bounds) {
            return Math.Abs(bounds.Minimum.X - bounds.Maximum.X);
        }
        public static double SizeY(this BoundingBox bounds) {
            return Math.Abs(bounds.Minimum.Y - bounds.Maximum.Y);
        }
        public static double SizeZ(this BoundingBox bounds) {
            return Math.Abs(bounds.Minimum.Z - bounds.Maximum.Z);
        }
        public static double X(this BoundingBox bounds) {
            return bounds.Minimum.X;
        }
        public static double Y(this BoundingBox bounds) {
            return bounds.Minimum.Y;
        }
        public static double Z(this BoundingBox bounds) {
            return bounds.Minimum.Z;
        }
        public static double GetDiagonal(this BoundingBox bounds) {
            return Math.Sqrt((bounds.Minimum.X - bounds.Maximum.X) * (bounds.Minimum.X - bounds.Maximum.X) + 
                (bounds.Minimum.Y - bounds.Maximum.Y) * (bounds.Minimum.Y - bounds.Maximum.Y) + 
                (bounds.Minimum.Z - bounds.Maximum.Z) * (bounds.Minimum.Z - bounds.Maximum.Z));
        }
    }
}
