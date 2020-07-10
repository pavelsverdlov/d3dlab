using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

using WPFLab.MVVM;

namespace D3DLab.Viewer.Presentation.Componets {
    class BoundingBoxDetailsViewModel : BaseNotify, IViewComponent {
        public string MinX => box.Minimum.X.ToString("F5",CultureInfo.InvariantCulture);
        public string MinY => box.Minimum.Y.ToString("F5",CultureInfo.InvariantCulture);
        public string MinZ => box.Minimum.Z.ToString("F5",CultureInfo.InvariantCulture);
        public string MaxX => box.Maximum.X.ToString("F5",CultureInfo.InvariantCulture);
        public string MaxY => box.Maximum.Y.ToString("F5",CultureInfo.InvariantCulture);
        public string MaxZ => box.Maximum.Z.ToString("F5", CultureInfo.InvariantCulture);
        public string CenterX => box.Center.X.ToString("F5",CultureInfo.InvariantCulture);
        public string CenterY => box.Center.Y.ToString("F5",CultureInfo.InvariantCulture);
        public string CenterZ => box.Center.Z.ToString("F5", CultureInfo.InvariantCulture);
        public string SizeX => box.Size().X.ToString("F5",CultureInfo.InvariantCulture);
        public string SizeY => box.Size().Y.ToString("F5", CultureInfo.InvariantCulture);
        public string SizeZ => box.Size().Z.ToString("F5", CultureInfo.InvariantCulture);


        AxisAlignedBox box;

        public BoundingBoxDetailsViewModel(in AxisAlignedBox box) {
            this.box = box; 
        }
        public void ApplyMatrix(in Matrix4x4 matrix) {
            box = box.Transform(matrix);

            SetPropertyChanged(nameof(CenterX));
            SetPropertyChanged(nameof(CenterY));
            SetPropertyChanged(nameof(CenterZ));

            SetPropertyChanged(nameof(SizeX));
            SetPropertyChanged(nameof(SizeY));
            SetPropertyChanged(nameof(SizeZ));

            SetPropertyChanged(nameof(MaxX));
            SetPropertyChanged(nameof(MaxY));
            SetPropertyChanged(nameof(MaxZ));

            SetPropertyChanged(nameof(MinX));
            SetPropertyChanged(nameof(MinY));
            SetPropertyChanged(nameof(MinZ));
        }
    }
}