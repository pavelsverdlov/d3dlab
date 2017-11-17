using System;
namespace HelixToolkit.Wpf.SharpDX
{
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Media.Media3D;

	using global::SharpDX;
	using global::SharpDX.Direct3D11;

	/// <summary>
	/// Provides a base class for a scene model
	/// </summary>
	public abstract class Model3D : Element3D, ITransformable
	{
		/// <summary>
		/// This is a hack model matrix. It is always pushed but
		/// never poped. It can be used to get the total model matrix
		/// in functions different than render or hittext, e.g., OnMouse3DMove.
		/// This is a temporar solution, until we have more time to think how to make it better.
		/// </summary>
		protected Matrix totalModelMatrix = Matrix.Identity;
		protected Matrix totalModelMatrixInv = Matrix.Identity;

		protected Matrix modelMatrix = Matrix.Identity;

		private Stack<Matrix> matrixStack = new Stack<Matrix>();

		public void PushMatrix(Matrix matrix)
		{
			lock (matrixStack)
			{
				matrixStack.Push(this.modelMatrix);
				this.modelMatrix = this.modelMatrix * matrix;
				this.totalModelMatrix = this.modelMatrix;
			}
		}

		public void PopMatrix()
		{
			lock (matrixStack)
			{
				this.modelMatrix = matrixStack.Pop();
			}
		}

		public virtual Matrix ModelMatrix
		{
			get { return this.modelMatrix; }
		}

		internal protected override Matrix CalcRenderMatrix(RenderContext renderContext, Matrix matrix)
		{
			return base.CalcRenderMatrix(renderContext, matrix) * modelMatrix;
		}

		public override void Update(RenderContext renderContext, System.TimeSpan timeSpan)
		{
			base.Update(renderContext, timeSpan);
		}

		public Matrix TotalModelMatrix
		{
			get { return this.totalModelMatrix; }
		}

		public Matrix LocalMatrix { get; private set; }

		public Transform3D Transform
		{
			get { return (Transform3D)this.GetValue(TransformProperty); }
			set
			{
				value.CheckIsNaN();
				this.SetValue(TransformProperty, value);
			}
		}

		public static readonly DependencyProperty TransformProperty =
			DependencyProperty.Register("Transform", typeof(Transform3D), typeof(Model3D), new FrameworkPropertyMetadata(Transform3D.Identity, TransformPropertyChanged));

		protected static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var model = (Model3D)d;
			if (model.isInTransformChanged)
				return;

			model.isInTransformChanged = true;
			try
			{
				model.OnTransformChanged(e);
			}
			finally
			{
				model.isInTransformChanged = false;
			}
		}

		bool isInTransformChanged;
		protected virtual void OnTransformChanged(DependencyPropertyChangedEventArgs e)
		{
			if (this.Transform != null)
			{
				var trafo = this.Transform.Value;
				this.modelMatrix = trafo.ToMatrix();
				this.LocalMatrix = this.modelMatrix;
				var matrix = this.modelMatrix;
				OnTransformChanged(matrix);
			}
		}

		protected virtual void OnTransformChanged(Matrix? matrix)
		{
			if (TransformChanged != null)
				TransformChanged(this, new TransformChangedEventArgs(matrix ?? this.modelMatrix));
		}
		public event EventHandler<TransformChangedEventArgs> TransformChanged;
	}

	public class TransformChangedEventArgs : EventArgs
	{
		public TransformChangedEventArgs(Matrix matrix)
		{
			Matrix = matrix;			
		}

		public Matrix Matrix { get; private set; }
	}
}
