using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using STimWPF.Util;
using STimWPF.Interaction;
using System.Windows.Media.Media3D;

namespace STimWPF.Pointing
{
	public class BoxInteractionMethod : InteractionMethod
	{

		private int layoutRows = 6;
		private int layoutCols = 6;
		public static readonly Key[] layout = { 
                              Key.A, Key.B, Key.C, Key.D, Key.E, Key.F,
                              Key.G, Key.H, Key.I, Key.J, Key.K, Key.L,
                              Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R,
                              Key.S, Key.T, Key.U, Key.V, Key.W, Key.X,
                              Key.Y, Key.Z, Key.D0, Key.D1, Key.D2, Key.D3,
                              Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9
                            };

		private Array allKeys = null;

		public BoxInteractionMethod()
		{
			allKeys = Enum.GetValues(typeof(Key));
		}

		public override InteractionStatus ProcessNewFrame(Point3D cursor, ICollection<InteractionGesture> gestures, Microsoft.Kinect.Skeleton stableSkeleton, double deltaTimeMilliseconds)
		{
			double columnData = cursor.X;
			double rowData = cursor.Y;
			double constraintData = cursor.Z;

			if (columnData < 0 || rowData < 0)
				return new InteractionStatus();
			double stepX = 1.0 / layoutCols;
			double stepY = 1.0 / layoutRows;

			int col = (int)(columnData / stepX);
			int row = (int)(rowData / stepY);

			InteractionStatus status = new InteractionStatus();
			status.HighlightedKey = layout[row * layoutCols + col];
			if (gestures != null && gestures.Count > 0 && gestures.ElementAt(0).Type == GestureType.Tap)
			{
				InteractionGesture gesture = gestures.ElementAt(0);
				col = (int)(gesture.Position.X / stepX);
				row = (int)(gesture.Position.Y / stepY);
				status.SelectedKey = layout[row * layoutCols + col];
			}

			return status;
		}

		public override string ToString()
		{
			return "Box";
		}

	}
}
