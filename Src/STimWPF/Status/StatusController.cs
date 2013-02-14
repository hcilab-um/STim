using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Kinect;
using STimWPF.Interaction;
using System.Windows;
using STimWPF.Properties;
using System.Net;
using System.IO;
using System.Windows.Media.Media3D;
using STimWPF.Util;

namespace STimWPF.Status
{
	public class StatusController
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("statusLogger");

		Object monitor = new Object();

		List<VisitStatus> lastVisits = null;
		List<VisitStatus> currentVisits = null;
		List<Skeleton> currentSkeletons = null;
		List<Skeleton> previousSkeletons = null;

		//joints
		public JointType ShoulderRight { get; set; }
		public JointType ShoulderLeft { get; set; }
		public JointType Head { get; set; }

		private Timer Trigger { get; set; }
		private VisitorController VisitorContr { get; set; }
		private int lastUserId;
		private int currentUserId;

		bool isControlling;
		bool wasControlling;
		string page = "";
		VisitStatus status;
		Vector3D viewDirection;
		Vector3D movementDirection;

		public StatusController(int period)
		{
			lastUserId = -1;
			Trigger = new Timer(new TimerCallback(TimerCallback), null, 0, period);
			VisitorContr = new VisitorController();
		}

		/// <summary>
		/// Every time this is called, the data is uploaded
		/// </summary>
		/// <param name="state"></param>
		public void TimerCallback(Object state)
		{
			lock (monitor)
			{
				//compare lastVisit with currentVisit
				currentVisits = new List<VisitStatus>();
				GenerateVisitStatus();

				foreach (VisitStatus status in currentVisits)
				{
					Object[] logObjects = new Object[]
					{
						status.VisitInit,
						status.SkeletonId,
						status.Zone,
						status.IsControlling,
						status.WasControlling,
						status.MovementDirection,
						status.ViewDirection,
						status.Page
					};
					int count = 0;
					StringBuilder formatSt = new StringBuilder();
					foreach (Object obj in logObjects)
						formatSt.Append("{" + (count++) + "};");
					String statusLog = String.Format(formatSt.ToString(), logObjects);
					logger.Info(statusLog);
				}

				lastUserId = currentUserId;
				lastVisits = currentVisits;
				previousSkeletons = currentSkeletons;
			}
		}

		/// <summary>
		/// Every new frame this gets called
		/// </summary>
		/// <param name="skeleton"></param>
		/// <param name="deltaMilliseconds"></param>
		public void LoadNewSkeletonData(Skeleton[] skeletons, double deltaMilliseconds, int userId)
		{
			lock (monitor)
			{
				if (skeletons != null && skeletons.Length != 0)
				{
					currentSkeletons = new List<Skeleton>(skeletons);
					currentUserId = userId;
				}
			}
		}

		private void GenerateVisitStatus()
		{

			if (currentSkeletons != null && currentSkeletons.Count != 0)
			{
				foreach (Skeleton skeleton in currentSkeletons)
				{
					if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
						continue;
					VisitorContr.DetectZone(skeleton);

					if (currentUserId == skeleton.TrackingId)
						isControlling = true;
					else
						isControlling = false;

					if (lastUserId == skeleton.TrackingId)
						wasControlling = true;
					else
						wasControlling = false;

					page = GetPage(VisitorContr.Zone);

					movementDirection = GetMovementDirection(skeleton);
					viewDirection = GetViewDirection(skeleton);

					status = new VisitStatus()
					{
						SkeletonId = skeleton.TrackingId,
						VisitInit = DateTime.Now,
						Zone = VisitorContr.Zone,
						ViewDirection = viewDirection,
						MovementDirection = movementDirection,
						IsControlling = isControlling,
						WasControlling = wasControlling,
						Page = page
					};

					currentVisits.Add(status);

				}
			}
		}

		private Vector3D GetViewDirection(Skeleton skeleton)
		{
			ShoulderRight = JointType.ShoulderRight;
			ShoulderLeft = JointType.ShoulderLeft;
			Head = JointType.Head;
			Joint shoulderR = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == ShoulderRight);
			Joint shoulderL = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == ShoulderLeft);
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Head);

			Vector3D shoulderRightP = new Vector3D(shoulderR.Position.X, shoulderR.Position.Y, shoulderR.Position.Z);
			Vector3D shoulderLeftP = new Vector3D(shoulderL.Position.X, shoulderL.Position.Y, shoulderL.Position.Z);
			Vector3D headP = new Vector3D(head.Position.X, head.Position.Y, head.Position.Z);

			Vector3D coordinateOriginP = ToolBox.GetMiddleVector(shoulderLeftP, shoulderRightP);
			//get Relative X, Y, Z direction
			Vector3D directionX = ToolBox.GetDisplacementVector(coordinateOriginP, shoulderRightP);
			Vector3D directionY = ToolBox.GetDisplacementVector(headP, coordinateOriginP);
			return Vector3D.CrossProduct(directionX, directionY);
		}

		private string GetPage(Zone zone)
		{
			switch (zone)
			{
				case Zone.Interaction:
					return "Content";
				default:
					return "None";
			}
		}

		private Vector3D GetMovementDirection(Skeleton currentSkeleton)
		{
			Skeleton lastSkeleton = null;
			if (previousSkeletons != null)
				lastSkeleton = previousSkeletons.SingleOrDefault(tmp => tmp.TrackingId == currentSkeleton.TrackingId);
			if (lastSkeleton != null)
			{
				return new Vector3D(currentSkeleton.Position.X - lastSkeleton.Position.X, currentSkeleton.Position.Y - lastSkeleton.Position.Y, currentSkeleton.Position.Z - lastSkeleton.Position.Z);
			}
			return new Vector3D(0, 0, 0);
		}
	}

}
