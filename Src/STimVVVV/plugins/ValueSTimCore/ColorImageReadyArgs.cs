using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media;

namespace VVVV.STim.Nodes

{
	public class ColorImageReadyArgs : EventArgs
	{
		public ImageSource Frame { get; set; }
	}
}