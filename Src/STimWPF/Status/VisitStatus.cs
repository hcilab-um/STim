using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STimWPF.Interaction;
using System.Windows;

namespace STimWPF.Status
{
	public class VisitStatus
	{
		public int SkeletonId { get; set; }
		public DateTime VisitInit { get; set; }
		public Zone Zone { get; set; }
		public Point ViewDirection { get; set; }
		public Point MovementDirection { get; set; }
		public bool IsControlling { get; set; }
		public bool WasControlling { get; set; }
		public string Page { get; set; }

		public VisitStatus() { }

	}
}
