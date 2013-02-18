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
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace STimWPF.Status
{
	public class StatusController
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("statusLogger");
		
		private const float RenderWidth = 640.0f;
		private const float RenderHeight = 480.0f;

		Object monitor = new Object();

		List<VisitStatus> lastVisits = null;
		List<VisitStatus> currentVisits = null;
		List<Skeleton> currentSkeletons = null;
		List<Skeleton> previousSkeletons = null;
		
		MemoryStream depthImageSourceMS;
		
		//joints
		public JointType ShoulderRight { get; set; }
		public JointType ShoulderLeft { get; set; }
		public JointType Head { get; set; }

		private Timer Trigger { get; set; }
		private VisitorController VisitorContr { get; set; }
		private Core Core { get; set; }
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
				Object[] logObjects = null;

				currentVisits = new List<VisitStatus>();
				GenerateVisitStatus();
				if(depthImageSourceMS != null)
					SaveDrawingImage(depthImageSourceMS);
				
				if (currentVisits.Count == 0)
				{
					logObjects = new Object[]
					{
						DateTime.Now,
						currentVisits.Count,
						"-",
						"-",
						"-",
						"-",
						"-",
						"-",
						"-"
					};
				}
				else
				{
					foreach (VisitStatus status in currentVisits)
					{
						logObjects = new Object[]
						{
							status.VisitInit,
							currentVisits.Count,
							status.SkeletonId,
							status.Zone,
							status.IsControlling,
							status.WasControlling,
							status.MovementDirection,
							status.ViewDirection,
							status.Page
						};

					}
				}
				
				LogVisitStatus(logObjects);

				lastUserId = currentUserId;
				lastVisits = currentVisits;
				previousSkeletons = currentSkeletons;
			}
		}

		private void LogVisitStatus(Object[] logObjects)
		{
			int count = 0;
			StringBuilder formatSt = new StringBuilder();
			foreach (Object obj in logObjects)
				formatSt.Append("{" + (count++) + "};");

			String statusLog = String.Format(formatSt.ToString(), logObjects);
			logger.Info(statusLog);
		}

		/// <summary>
		/// Every new frame this gets called
		/// </summary>
		/// <param name="skeleton"></param>
		/// <param name="deltaMilliseconds"></param>
		public void LoadNewSkeletonData(Skeleton[] skeletons, Skeleton userSkeleton, DrawingImage dIS)
		{
			lock (monitor)
			{
				if (skeletons == null || skeletons.Length == 0 || userSkeleton == null)
				{
					currentSkeletons = null;
					currentUserId = -1;
					return;
				}

				this.depthImageSourceMS = SaveDrawingImage(dIS);
				currentSkeletons = new List<Skeleton>(skeletons);
				currentUserId = userSkeleton.TrackingId;
			}
		}

		private MemoryStream SaveDrawingImage(DrawingImage drawingImage)
		{
			if (drawingImage == null)
				return null;

			try
			{
				DrawingVisual drawingVisual = new DrawingVisual();
				DrawingContext drawingContext = drawingVisual.RenderOpen();
				drawingContext.DrawImage(drawingImage, new Rect(0, 0, RenderWidth, RenderHeight));
				drawingContext.Close();

				RenderTargetBitmap bmp = new RenderTargetBitmap((int)RenderWidth, (int)RenderHeight, 96, 96, PixelFormats.Pbgra32);
				bmp.Render(drawingVisual);

				BmpBitmapEncoder encoder = new BmpBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(bmp));

				MemoryStream diMS = new MemoryStream();
				encoder.Save(diMS);
				return diMS;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			return null;
		}

		private void SaveDrawingImage(MemoryStream imageMS)
		{
			try
			{
				imageMS.Seek(0, SeekOrigin.Begin);
				BitmapFrame bitmap = BitmapFrame.Create(imageMS);
				String qualifiedName = String.Format("{0}.png", DateTime.Now.ToString("MMddyy-HHmmss"));

				PngBitmapEncoder encoder = new PngBitmapEncoder();
				encoder.Frames.Add(bitmap);
				using (var stream = new FileStream(Settings.Default.ImageFolder + qualifiedName, FileMode.Create))
				{
					encoder.Save(stream);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private void GenerateVisitStatus()
		{

			if (currentSkeletons == null || currentSkeletons.Count == 0)
				return;
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
					if (Core.Instance.ContentState != ContentState.Detail)
						return Core.Instance.ContentState.ToString();
					return Core.Instance.ContentState+"-"+Core.Instance.DetailContentState;
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
