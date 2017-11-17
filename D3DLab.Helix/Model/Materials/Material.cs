namespace HelixToolkit.Wpf.SharpDX
{
	using System;
	using System.Windows;

	[Serializable]
	public abstract class Material { 
		public string Name { get; set; }
		//{
		//    get { return (string)this.GetValue(NameProperty); }
		//    set { this.SetValue(NameProperty, value); }
		//}

		public virtual void Assign(Material other)
		{
			if (other == null)
				return;

			Name = other.Name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
