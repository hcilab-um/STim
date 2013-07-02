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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using STimWPF.Util;

namespace STimWPF.Status
{
	public class StatusController
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger("statusLogger");

		private const float RenderWidth = 640.0f;
		private const float RenderHeight = 480.0f;

		Object monitor = new Object();

		private int visitCounter = 0;

		DrawingImage imageSource;

		private Timer Trigger { get; set; }
		private VisitorController VisitorContr { get; set; }

		List<VisitStatus> lastVisits = null;
		List<VisitStatus> currentVisits = null;

		List<WagSkeleton> lastSkeletons = null;
		List<WagSkeleton> currentSkeletons = null;
		VisitStatus status;

		bool wasControlling;
		bool isControlling;
		int lastUserSkeletonId;
		int currentUserSkeletonId;

		Vector movementDirection;
		double movementDistance;

		DateTime currentDateTime;

		public StatusController(int period)
		{
			lastUserSkeletonId = -1;
			currentDateTime = DateTime.Now;
			Trigger = new Timer(new TimerCallback(TimerCallback), null, 0, period);
			VisitorContr = new VisitorController();
		}

		/// <summary>
		/// Every time this is called, the data is uploaded
		/// </summary>
		/// <param name="state"></param>
		public void TimerCallback(Object state)
		{
			if (imageSource != null)
				SaveDrawingImage(imageSource);

			lock (monitor)
			{
				Object[] logObjects = null;
				currentVisits = new List<VisitStatus>();

				GenerateVisitStatus();

				if (currentVisits.Count == 0)
				{
					logObjects = new Object[]
					{
						DateTime.Now.ToString(Settings.Default.DateTimeLogFormat),
						currentVisits.Count,
						"-",				
						"-",				
						"-",				
						"-",				
						"-",				
						"-",				
						"-",				
						"-",					
						"-",					
						"-",				
						"-"
					};

					LogVisitStatus(logObjects);
				}
				else
				{

					foreach (VisitStatus status in currentVisits)
					{
						bool existedBefore = false;

						if (lastVisits != null)
							existedBefore = lastVisits.Exists(tmp => tmp.SkeletonId == status.SkeletonId);

						if (!existedBefore)
							status.VisitId = ++visitCounter;
						else
							status.VisitId = lastVisits.Single(tmp => tmp.SkeletonId == status.SkeletonId).VisitId;

						logObjects = new Object[]
						{
							status.VisitInit.ToString(Settings.Default.DateTimeLogFormat),
							currentVisits.Count,								
							status.VisitId,											
							status.Zone,												
							status.IsControlling,								
							status.WasControlling,							
							status.HeadLocation.X,							
							status.HeadLocation.Y,							
							status.HeadLocation.Z,							
							status.MovementDirection.X,					
							status.MovementDirection.Y,					
							status.MovementDistance,						
							status.BodyAngle								
						};

						LogVisitStatus(logObjects);
					}
				}																					

				lastUserSkeletonId = currentUserSkeletonId;
				lastVisits = currentVisits;
				lastSkeletons = currentSkeletons;
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
		public void LoadNewSkeletonData(WagSkeleton[] skeletons, WagSkeleton userSkeleton, DrawingImage drawingImage)
		{
			lock (monitor)
			{
				if (skeletons == null || skeletons.Length == 0 || userSkeleton == null)
				{
					currentSkeletons = null;
					currentUserSkeletonId = -1;
					return;
				}

				imageSource = drawingImage;
				currentSkeletons = skeletons.ToList();
				currentUserSkeletonId = userSkeleton.TrackingId;
				currentDateTime = DateTime.Now;
			}
		}

		private void SaveDrawingImage(DrawingImage image)
		{
			image.Dispatcher.Invoke(DispatcherPriority.Render, (Action)delegate()
			{
				try
				{
					DrawingVisual drawingVisual = new DrawingVisual();
					DrawingContext drawingContext = drawingVisual.RenderOpen();
					drawingContext.DrawImage(image, new Rect(0, 0, RenderWidth, RenderHeight));
					drawingContext.Close();

					RenderTargetBitmap bmp = new RenderTargetBitmap((int)RenderWidth, (int)RenderHeight, 96, 96, PixelFormats.Pbgra32);
					bmp.Render(drawingVisual);
					String qualifiedName = String.Format("{0}.png", DateTime.Now.ToString(Settings.Default.DateTimeFileNameFormat));
					PngBitmapEncoder encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(bmp));
					using (var stream = new FileStream(Settings.Default.ImageFolder + qualifiedName, FileMode.Create))
					{
						encoder.Save(stream);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			});
		}

		private void GenerateVisitStatus()
		{
			if (currentSkeletons == null || currentSkeletons.Count == 0)
				return;

			foreach (WagSkeleton skeleton in currentSkeletons)
			{

				Point3D headP = skeleton.HeadLocation;

				VisitorContr.DetectZone(skeleton);

				if (currentUserSkeletonId == skeleton.TrackingId)
					isControlling = true;
				else
					isControlling = false;

				if (lastUserSkeletonId == skeleton.TrackingId)
					wasControlling = true;
				else
					wasControlling = false;

				//get movement direction
				WagSkeleton lastSkeleton = null;
				if (lastSkeletons != null)
					lastSkeleton = lastSkeletons.SingleOrDefault(tmp => tmp.TrackingId == skeleton.TrackingId);
				if (lastSkeleton != null)
				{
					Point3D lastHeadP = lastSkeleton.HeadLocation;
					movementDirection = ToolBox.GetMovementVector(lastHeadP, headP);
				}
				else
					movementDirection = new Vector();
				
				movementDistance = movementDirection.Length;
				
				status = new VisitStatus()
				{
					SkeletonId = skeleton.TrackingId,
					VisitInit = currentDateTime,
					Zone = VisitorContr.Zone,
					IsControlling = isControlling,
					WasControlling = wasControlling,

					HeadLocation = headP,
					MovementDistance = movementDistance,
					MovementDirection = movementDirection,
					BodyAngle = skeleton.BodyOrientationAngle,
				};

				currentVisits.Add(status);
			}
		}

	}

}