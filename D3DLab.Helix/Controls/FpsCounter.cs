using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;

namespace HelixToolkit.Wpf.SharpDX
{
	public class FpsCounter : DispatcherObject, INotifyPropertyChanged
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FpsCounter"/> class.
		/// </summary>
		public FpsCounter()
		{
			items = new Item[3]; // 0-BGRender, 1-Viewport3DX, 2-DPFCanvas
			for (int i = 0; i < items.Length; i++)
			{
				items[i] = new Item(i);
				items[i].PropertyChanged += FpsCounter_PropertyChanged;
			}
		}

		void FpsCounter_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var item = sender as Item;
			if (item == null)
				return;

			if (item.Idx == 0)
				OnPropertyChanged("Value");
			else if (item.Idx == 1)
				OnPropertyChanged("ValueVP");
			else if (item.Idx == 2)
				OnPropertyChanged("ValueDPF");
		}

		public void AddFrame(TimeSpan ts, int idx = 0)
		{
			items[idx].AddFrame(ts);
		}

		public void Clear()
		{
			foreach (Item item in items)
				item.Clear();
		}

		public double Value
		{
			get { return items[0].Value; }
		}

		public double ValueVP
		{
			get { return items[1].Value; }
		}

		public double ValueDPF
		{
			get { return items[2].Value; }
		}

		public double[] Values
		{
			get { return items.Select(i => i.Value).ToArray(); }
		}

		#region INotifyPropertyChanged Members

		void OnPropertyChanged(string name)
		{
			Dispatcher.BeginInvoke((Action<string>)(i =>
			{
				var e = PropertyChanged;
				if (e != null)
					e(this, new PropertyChangedEventArgs(i));
			}), name);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		Item[] items;

		private class Item : DispatcherObject, INotifyPropertyChanged
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="Item"/> class.
			/// </summary>
			public Item(int idx)
			{
				this.Idx = idx;
			}

			public int Idx { get; private set; }

			private TimeSpan m_averagingInterval = TimeSpan.FromSeconds(1);
			public TimeSpan AveragingInterval
			{
				get { return m_averagingInterval; }
				set
				{
					if (value == m_averagingInterval)
						return;
					if (value < TimeSpan.FromSeconds(0.1))
						throw new ArgumentOutOfRangeException();
					m_averagingInterval = value;
				}
			}

			private double m_value;
			public double Value
			{
				get { return m_value; }
				private set
				{
					if (value == m_value)
						return;
					m_value = value;
					OnPropertyChanged("Value");
				}
			}

			public void AddFrame(TimeSpan ts)
			{
				var sec = AveragingInterval;
				var index = m_frames.FindLastIndex(aTS => ts - aTS > sec);
				if (index > -1)
					m_frames.RemoveRange(0, index);
				m_frames.Add(ts);
				UpdateValue();
			}

			public void Clear()
			{
				m_frames.Clear();
				UpdateValue();
			}

			private List<TimeSpan> m_frames = new List<TimeSpan>();
			private void UpdateValue()
			{
				if (m_frames.Count < 2)
				{
					Value = -1;
				}
				else
				{
					var dt = m_frames[m_frames.Count - 1] - m_frames[0];
					Value = dt.Ticks > 100 ? m_frames.Count / dt.TotalSeconds : -1;
				}
			}

			#region INotifyPropertyChanged Members

			void OnPropertyChanged(string name)
			{
				var handler = PropertyChanged;
				if (handler != null)
					handler(this, new PropertyChangedEventArgs(name));
			}
			public event PropertyChangedEventHandler PropertyChanged;

			#endregion
		}
	}
}
