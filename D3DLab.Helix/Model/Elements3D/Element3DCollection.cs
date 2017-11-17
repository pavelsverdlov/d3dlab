using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HelixToolkit.Wpf.SharpDX
{
	/// <summary>
	/// Provides a collection of Element3D.
	/// </summary>
	public class Element3DCollection : ObservableList<Element3D>
	{
	}

	/// <summary>
	/// Provides an observable collection of Element3D.
	/// </summary>
	public class ObservableElement3DCollection : ObservableCollection<Element3D>
	{
	}
}