using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace STimWPF.Interaction
{
	public enum GestureType { Pressing, Releasing, Tap };

	public struct InteractionGesture
	{
		public Point3D Position;
		public GestureType Type;
		public DateTime Time;
	}
}
