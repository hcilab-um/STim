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

		private int visitCounter = 0;
		List<VisitStatus> lastVisits = null;
		List<VisitStatus> currentVisits = null;
		List<Skeleton> currentSkeletons = null;
		List<Skeleton> lastSkeletons = null;

		MemoryStream depthImageSourceMS;

		static readonly JointType	ShoulderRight = JointType.ShoulderRight;
		static readonly JointType ShoulderLeft = JointType.ShoulderLeft;
		static readonly JointType Head = JointType.Head;

		private Matrix3D transformMatrix;

		private Timer Trigger { get; set; }
		private VisitorController VisitorContr { get; set; }
		private InteractionController InteractionCtr { get; set; }
		private Controls.ContentControl ContentCtrl { get; set; }
		private int lastUserSkeletonId;
		private int currentUserSkeletonId;

		bool isControlling;
		bool wasControlling;
		string page = "";
		VisitStatus status;

		Vector3D viewDirection;
		Vector3D movementDirection;
		Vector3D location;

		double movementDistance;
		double adjustAngleInRadian;
		double viewAngle;
		DateTime currentDateTime;

		public StatusController(int period, double kinectAngleInRadian)
		{
			lastUserSkeletonId = -1;
			currentDateTime = DateTime.Now;
			Trigger = new Timer(new TimerCallback(TimerCallback), null, 0, period);
			VisitorContr = new VisitorController();
			adjustAngleInRadian = Math.Abs(kinectAngleInRadian);
			Vector3D vX = new Vector3D(1, 0, 0);
			Vector3D vY = new Vector3D(0, Math.Cos(adjustAngleInRadian), -Math.Sin(adjustAngleInRadian));
			Vector3D vZ = new Vector3D(0, Math.Sin(adjustAngleInRadian), Math.Cos(adjustAngleInRadian));
			transformMatrix = ToolBox.NewCoordinateMatrix(vX, vY, vZ);
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
				if (depthImageSourceMS != null)
					SaveDrawingImage(depthImageSourceMS);

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
							status.Location.X,
							status.Location.Y,
							status.Location.Z,
							status.MovementDirection.X,
							status.MovementDirection.Y,
							status.MovementDirection.Z,
							status.MovementDistance,
							status.ViewDirection.X,
							status.ViewDirection.Y,
							status.ViewDirection.Z,
							status.ViewAngle,
							status.Page
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
		public void LoadNewSkeletonData(Skeleton[] skeletons, Skeleton userSkeleton, DrawingImage dIS, Controls.ContentControl contentControl)
		{
			lock (monitor)
			{
				if (skeletons == null || skeletons.Length == 0 || userSkeleton == null)
				{
					currentSkeletons = null;
					currentUserSkeletonId = -1;
					return;
				}
				ContentCtrl = contentControl;
				this.depthImageSourceMS = SaveDrawingImage(dIS);
				currentSkeletons = skeletons.Where(temp => temp.TrackingState == SkeletonTrackingState.Tracked).ToList();
				currentUserSkeletonId = userSkeleton.TrackingId;
				currentDateTime = DateTime.Now;
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
				String qualifiedName = String.Format("{0}.png", DateTime.Now.ToString(Settings.Default.DateTimeFileNameFormat));

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

				Vector3D headV = RetrieveJointVector(skeleton, Head, transformMatrix);
				Vector3D shoulderRV = RetrieveJointVector(skeleton, ShoulderRight, transformMatrix);
				Vector3D shoulderLV = RetrieveJointVector(skeleton, ShoulderLeft, transformMatrix);

				VisitorContr.DetectZone(skeleton);

				if (currentUserSkeletonId == skeleton.TrackingId)
					isControlling = true;
				else
					isControlling = false;

				if (lastUserSkeletonId == skeleton.TrackingId)
					wasControlling = true;
				else
					wasControlling = false;

				location = new Vector3D(headV.X, headV.Y, headV.Z);

				//get movement direction
				Skeleton lastSkeleton = null;
				if (lastSkeletons != null)
					lastSkeleton = lastSkeletons.SingleOrDefault(tmp => tmp.TrackingId == skeleton.TrackingId);
				if (lastSkeleton != null)
				{
					Vector3D lastHeadV = RetrieveJointVector(lastSkeleton, Head, transformMatrix);
					movementDirection = ToolBox.GetDisplacementVector(lastHeadV, headV);
				}
				else
					movementDirection = new Vector3D();
				movementDistance = movementDirection.Length;

				viewDirection = GetViewDirection(shoulderRV, shoulderLV, headV);
				viewAngle = Vector.AngleBetween(new Vector(0, -1), new Vector(viewDirection.X, viewDirection.Z));
				page = ContentCtrl.CurrentPage();
				status = new VisitStatus()
				{
					SkeletonId = skeleton.TrackingId,
					VisitInit = currentDateTime,
					Zone = VisitorContr.Zone,
					Location = location,
					ViewDirection = viewDirection,
					ViewAngle = viewAngle,
					MovementDirection = movementDirection,
					MovementDistance = movementDistance,
					IsControlling = isControlling,
					WasControlling = wasControlling,
					Page = page
				};
				currentVisits.Add(status);
			}
		}

		/// <summary>
		/// Retrieve joint from skeleton, transform it into vector in correct coordinate
		/// </summary>
		/// <param name="skeleton"></param>
		/// <param name="type"></param>
		/// <param name="transformMatrix"></param>
		/// <returns></returns>
		private Vector3D RetrieveJointVector(Skeleton skeleton, JointType type, Matrix3D transformMatrix)
		{
			Joint joint = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == type);
			Vector3D vector = new Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
			vector *= transformMatrix;
			vector.Y += Settings.Default.KinectDeviceHeight;
			vector.Z -= Settings.Default.Kinect_DisplayDistance;
			return vector;
		}

		private Vector3D GetViewDirection(Vector3D shoulderRightVector, Vector3D shoulderLeftVector, Vector3D headVector)
		{
			Vector3D bodyCenterP = ToolBox.GetMiddleVector(shoulderLeftVector, shoulderRightVector);
			//get Relative X, Y, Z direction
			Vector3D bodyDirectionX = ToolBox.GetDisplacementVector(bodyCenterP, shoulderRightVector);
			Vector3D bodyDirectionY = ToolBox.GetDisplacementVector(bodyCenterP, headVector);
			return Vector3D.CrossProduct(bodyDirectionY, bodyDirectionX);
		}
	}

}