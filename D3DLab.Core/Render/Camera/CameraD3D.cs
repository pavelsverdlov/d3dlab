using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3DLab.Core.Components;
using SharpDX;

namespace D3DLab.Core.Render.Camera {
    public abstract class CameraD3D : ComponentContainer {
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public abstract Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the look direction.
        /// </summary>
        /// <value>
        /// The look direction.
        /// </value>
        public abstract Vector3 LookDirection { get; set; }

        /// <summary>
        /// Gets or sets up direction.
        /// </summary>
        /// <value>
        /// Up direction.
        /// </value>
        public abstract Vector3 UpDirection { get; set; }

        /// <summary>
        /// Creates the view matrix.
        /// </summary>
        /// <returns>A <see cref="Matrix" />.</returns>
        public abstract Matrix CreateViewMatrix();

        /// <summary>
        /// Creates the projection matrix.
        /// </summary>
        /// <param name="aspectRatio">The aspect ratio.</param>
        /// <returns>A <see cref="Matrix" />.</returns>
        public abstract Matrix CreateProjectionMatrix(double aspectRatio);
    }
}
