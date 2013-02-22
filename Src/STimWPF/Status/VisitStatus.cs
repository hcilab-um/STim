using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STimWPF.Interaction;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace STimWPF.Status
{
	public class VisitStatus
	{
		public int VisitId { get; set; }
		public int SkeletonId { get; set; }
		public DateTime VisitInit { get; set; }
		public Zone Zone { get; set; }
		public Vector3D Location { get; set; }
		public Vector3D MovementDirection { get; set; }
		public double MovementDistance { get; set; }
		public Vector3D ViewDirection { get; set; }
		public double ViewAngle { get; set; }
		public bool IsControlling { get; set; }
		public bool WasControlling { get; set; }
		public string Page { get; set; }

		public VisitStatus() { }

	}
}
