using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HelixToolkit.Wpf.SharpDX.Extensions {
    public static class PhongMaterialExtensions {

		public static Material CloneMaterial(this Material material)
		{
			if (material == null)
				return null;

			var pm = material as PhongMaterial;
			if (pm != null)
				return pm.Clone();

			throw new NotImplementedException("Not implement CloneMaterial for " + material);
		}
    }
}
