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
		
		public bool IsControlling { get; set; }
		public bool WasControlling { get; set; }

		public Point3D HeadLocation { get; set; }
		public Vector3D HeadDirection { get; set; }

		public double MovementDistance { get; set; }
		public Vector MovementDirection { get; set; }
		public double BodyAngle { get; set; }
		
		public Attention.AttentionSimple AttentionSimple { get; set; }
		public Attention.AttentionSocial AttentionSocial { get; set; }

		public bool TouchInteraction { get; set; }
		public bool GestureInteraction { get; set; }

		public VisitStatus() { }

	}
}
