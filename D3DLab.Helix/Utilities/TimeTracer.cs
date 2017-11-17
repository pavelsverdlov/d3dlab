using System;

namespace HelixToolkit.Wpf.SharpDX.Utilities {
	public class TimeTracer {
		private readonly DateTime start;
		private string text;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="title"></param>
		public TimeTracer(string title) {
			start = DateTime.Now;
			text = title + " : " + start.ToString("HH:mm:ss.fff");
		}

		/// <summary>
		/// Add a text value
		/// </summary>
		/// <param name="value"></param>
		public void Type(string value) {
			text += " - " + value;
		}

		/// <summary>
		/// Add a time mark
		/// </summary>
		public void Mark() {
			text += " - " + DateTime.Now.ToString("HH:mm:ss.fff");
		}

		/// <summary>
		/// Make a time-tracking text
		/// </summary>
		/// <returns></returns>
		public string Text() {
			var stop = DateTime.Now;
			return text + " - " + stop.ToString("HH:mm:ss.fff") + " - " + (stop - start).TotalMilliseconds;
		}

		/// <summary>
		/// Print time track
		/// </summary>
		public void Trace() {
			System.Diagnostics.Trace.WriteLine(Text());
		}
	}
}
