using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Kinect;
using STim.Interaction;
using System.Windows;
using System.Net;
using System.IO;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using STim.Util;
using System.Diagnostics;

namespace STim.Status
{
	public class StatusController
	{
		private log4net.ILog visitLogger;
		private log4net.ILog statusLogger;

		private const float RenderWidth = 640.0f;
		private const float RenderHeight = 480.0f;

		private EventWaitHandle waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);
		private Timer Trigger { get; set; }

		private Dictionary<int, Point3D> lastPositions = null;
		private List<WagSkeleton> currentSkeletons = null;

		private byte[] imageSource;

		private int controllerId;
		private int lastControllerId;
		private int visitCounter = 0;
		private List<VisitStatus> lastVisits = null;
		public VisitorController VisitorContr { get; set; }

		private Dispatcher _dispatcher;

		public StatusController(Dispatcher uiDispatcher, int period, log4net.ILog visitLogger, log4net.ILog statusLogger)
		{
			this.visitLogger = visitLogger;
			this.statusLogger = statusLogger;
			_dispatcher = uiDispatcher;
			lastControllerId = -1;
			Trigger = new Timer(new TimerCallback(TimerCallback), null, 0, period);
			lastPositions = new Dictionary<int, Point3D>();
		}

		/// <summary>
		/// Every time this is called, the data is uploaded
		/// </summary>
		/// <param name="state"></param>
		public void TimerCallback(Object state)
		{
			waitHandle.WaitOne();
			DateTime currentTime = DateTime.Now;
			String qualifiedName = "No_Skeleton";

			String skeletonIdInfo = "";
			int totalVisits = 0;
			//"DDMMYY-HHmmss-milliseconds-SK1-SK2..."
			if (currentSkeletons != null)
			{
				foreach (WagSkeleton wagSkeleton in currentSkeletons)
				{
					skeletonIdInfo += String.Format("-{0}", wagSkeleton.TrackingId);
				}
				qualifiedName = String.Format("{0}ms{1}.jpg", currentTime.ToString(STimSettings.DateTimeFileNameFormat), skeletonIdInfo);
				totalVisits = currentSkeletons.Count;
				SaveDrawingImage(imageSource, qualifiedName);
			}

			Object[] logObjects = null;
			logObjects = new Object[]
			{
				currentTime.ToString(STimSettings.DateTimeFileNameFormat),
				totalVisits,
				qualifiedName
			};

			LogInformation(logObjects, statusLogger);

			waitHandle.Set();
		}

		private void SaveDrawingImage(byte[] image, String imageName)
		{
			if (image == null || image.Length == 0)
				return;
			try
			{

				using (var stream = new FileStream(STimSettings.ImageFolder + imageName, FileMode.Create))
				{
					stream.Write(image, 0, image.Length);
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		/// <summary>
		/// Every new frame this gets called
		/// </summary>
		/// <param name="skeleton"></param>
		/// <param name="deltaMilliseconds"></param>
		public void LoadNewSkeletonData(List<WagSkeleton> skeletons, WagSkeleton controllerSkeleton, byte[] drawingImage)
		{
			waitHandle.WaitOne();

			imageSource = drawingImage;

			if (skeletons == null || skeletons.Count == 0 || controllerSkeleton == null)
			{
				currentSkeletons = null;
				controllerId = -1;
			}
			else
			{
				currentSkeletons = skeletons;
				controllerId = controllerSkeleton.TrackingId;

				Object[] logObjects = null;
				List<VisitStatus> currentVisits = CreateVisitStatus();

				foreach (VisitStatus status in currentVisits)
				{
					//Visitors.Count
					//ImageFile
					logObjects = new Object[]
						{
							status.VisitInit.ToString(STimSettings.DateTimeLogFormat),
							currentVisits.Count,
							status.VisitId,
							status.SkeletonId,
							
							status.Zone,
							status.IsControlling,
							status.WasControlling,

							status.HeadLocation.X,							
							status.HeadLocation.Y,							
							status.HeadLocation.Z,

							status.HeadDirection.X,
							status.HeadDirection.Y,
							status.HeadDirection.Z,
							
							status.MovementDirection.X,
							status.MovementDirection.Y,					
							status.MovementDistance,
							
							status.BodyAngle,
							
							status.AttentionSimple.SimpleAttentionValue,
							status.AttentionSocial.SocialAttentionValue,
							
							status.TouchInteraction,
							status.GestureInteraction
						};

					LogInformation(logObjects, visitLogger);
				}

				lastControllerId = controllerId;
				lastVisits = currentVisits;

				lastPositions.Clear();
				if (currentSkeletons != null)
				{
					foreach (WagSkeleton wagSkel in currentSkeletons)
						lastPositions.Add(wagSkel.TrackingId, wagSkel.HeadLocation);
				}
			}
			waitHandle.Set();
		}

		private List<VisitStatus> CreateVisitStatus()
		{
			List<VisitStatus> currentVisits = new List<VisitStatus>();

			if (currentSkeletons == null || currentSkeletons.Count == 0)
				return currentVisits;

			DateTime currentDateTime = DateTime.Now;
			foreach (WagSkeleton skeleton in currentSkeletons)
			{
				Vector movementDirection = new Vector();
				if (lastPositions.ContainsKey(skeleton.TrackingId))
					movementDirection = ToolBox.GetMovementVector(lastPositions[skeleton.TrackingId], skeleton.HeadLocation);
				double movementDistance = movementDirection.Length;

				VisitStatus lastStatus = null;
				if (lastVisits != null)
					lastStatus = lastVisits.FirstOrDefault(visit => visit.SkeletonId == skeleton.TrackingId);

				VisitStatus status = new VisitStatus()
				{
					VisitId = lastStatus != null ? lastStatus.VisitId : ++visitCounter,
					SkeletonId = skeleton.TrackingId,
					
					VisitInit = lastStatus != null ? lastStatus.VisitInit : currentDateTime,
					Zone = VisitorContr.DetectZone(skeleton),

					IsControlling = controllerId == skeleton.TrackingId ? true : false,
					WasControlling = lastControllerId == skeleton.TrackingId ? true : false,

					HeadLocation = skeleton.HeadLocation,
					HeadDirection = skeleton.HeadOrientation,

					MovementDistance = movementDistance,
					MovementDirection = movementDirection,

					BodyAngle = skeleton.BodyOrientationAngle,

					AttentionSimple = skeleton.AttentionSimple,
					AttentionSocial = skeleton.AttentionSocial,
					
					TouchInteraction = false,
					GestureInteraction = false
				};
				currentVisits.Add(status);
			}

			return currentVisits;
		}

		private void LogInformation(Object[] logObjects, log4net.ILog logger)
		{
			int count = 0;
			StringBuilder formatSt = new StringBuilder();
			foreach (Object obj in logObjects)
				formatSt.Append("{" + (count++) + "};");

			String statusLog = String.Format(formatSt.ToString(), logObjects);
			logger.Info(statusLog);
		}


	}

}