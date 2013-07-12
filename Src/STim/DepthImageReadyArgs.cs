using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace STim
{
	public class DepthImageReadyArgs : EventArgs
	{
		public ImageSource Frame { get; set; }
	}
}
