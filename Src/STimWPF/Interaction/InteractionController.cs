using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Collections;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Threading;
using STimWPF.Interaction;
using STimWPF.Util;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace STimWPF.Interaction
{

	public class InteractionController : INotifyPropertyChanged
	{
		//private int currentFrame = 1;// totalFrames = 0;
		private Object processingLock = new Object();
		private EventWaitHandle processingWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);

		private Point3D cursorLocation;
		private SelectionMethod selectionMethod;
		private InteractionZone interactionZone;

		private Vector3D planeOrigin;
		private double planeRadius = -1;
		private double planeWidth = -1;
		private double planeHeight = -1;
		private double boundaryCross = -1;
		//joints
		public JointType ShoulderRight { get; set; }
		public JointType ShoulderLeft { get; set; }
		public JointType Elbow { get; set; }
		public JointType Wrist { get; set; }
		public JointType Hand { get; set; }
		public JointType Hip { get; set; }
		public JointType Head { get; set; }
		
		public GestureRecognizer Recognizer { get; set; }

		public Point3D CursorLocation
		{
			get { return cursorLocation; }
			set
			{
				cursorLocation = value;
				OnPropertyChanged("CursorLocation");
			}
		}

		public Rect MouseBoundaries { get; set; }

		public SelectionMethod SelectionMethod
		{
			get { return selectionMethod; }
			set
			{
				selectionMethod = value;
				OnPropertyChanged("SelectionMethod");
			}
		}

		public InteractionZone InteractionZone
		{
			get { return interactionZone; }
			set
			{
				interactionZone = value;
				OnPropertyChanged("InteractionZone", interactionZone);
			}
		}

		private bool hasUserClicked = false;
		private Object clickLock = new Object();
		public bool HasUserClicked
		{
			get
			{
				lock (clickLock)
				{
					if (hasUserClicked)
					{
						hasUserClicked = false;
						return true;
					}
					return false;
				}
			}
			set
			{
				lock (clickLock)
					hasUserClicked = value;
				OnPropertyChanged("HasUserClicked");
			}
		}

		public InteractionController()
		{
			planeRadius = -1;
			Recognizer = new GestureRecognizer();
			PropertyChanged += new PropertyChangedEventHandler(Recognizer.InteractionCtrngine_PropertyChanged);

			CursorLocation = new Point3D(0, 0, 0);
			SelectionMethod = STimWPF.Interaction.SelectionMethod.Click;
		}

		public bool ProcessNewSkeletonData(Skeleton skeleton, double deltaMilliseconds, InteractionZone interactionZone)
		{
			if (skeleton == null)
				return false;
			//++totalFrames;
			//SkeletonCapture capture = new SkeletonCapture() { Delay = deltaMilliseconds, Skeleton = skeleton, FrameNro = totalFrames };
			//Thread backgroundThread = new Thread(ProcessInteractionData);
			//backgroundThread.Priority = ThreadPriority.AboveNormal;
			//backgroundThread.Start(capture);
			DoWork(skeleton, deltaMilliseconds, interactionZone);

			return true;
		}

		private void DoWork(Skeleton skeleton, double deltaMilliseconds, InteractionZone interactionZone)
		{
			Size layoutSize = new Size(35, 35);
			bool leftClick = false;
			if (interactionZone == Interaction.InteractionZone.Interaction)
			{
				//1- find the position of the cursor on the layout plane
				CursorLocation = FindCursorPosition(skeleton, MouseBoundaries, selectionMethod);
				//3- Looks for gestures [pressed, released]
				ICollection<InteractionGesture> gestures = Recognizer.ProcessGestures(skeleton, deltaMilliseconds, cursorLocation, selectionMethod, HasUserClicked);
				if (gestures != null && gestures.Count > 0 && gestures.ElementAt(0).Type == GestureType.Tap)
				{
					leftClick = true;
				}
				MouseController.SendMouseInput((int)CursorLocation.X, (int)CursorLocation.Y, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);
			}
		}

		private Point3D FindCursorPosition(Skeleton skeleton, Rect boundary, SelectionMethod selectionM)
		{
			ShoulderRight = JointType.ShoulderRight;
			ShoulderLeft = JointType.ShoulderLeft;
			Elbow = JointType.ElbowRight;
			Wrist = JointType.WristRight;
			Hand = JointType.HandRight;
			Hip = JointType.HipRight;
			Head = JointType.Head;
			Joint shoulderR = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == ShoulderRight);
			Joint shoulderL = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == ShoulderLeft);
			Joint elbow = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Elbow);
			Joint wrist = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Wrist);
			Joint hand = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Hand);
			Joint hipCenter = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Hip);

			Vector3D shoulderRightP = new Vector3D(shoulderR.Position.X, shoulderR.Position.Y, shoulderR.Position.Z);
			Vector3D shoulderLeftP = new Vector3D(shoulderL.Position.X, shoulderL.Position.Y, shoulderL.Position.Z);
			Vector3D wristRightP = new Vector3D(wrist.Position.X, wrist.Position.Y, wrist.Position.Z);
			Vector3D elbowRightP = new Vector3D(elbow.Position.X, elbow.Position.Y, elbow.Position.Z);
			Vector3D handRightP = new Vector3D(hand.Position.X, hand.Position.Y, hand.Position.Z);
			Vector3D hipP = new Vector3D(hipCenter.Position.X, hipCenter.Position.Y, hipCenter.Position.Z);
			
			if (planeRadius == -1)
			{
				planeRadius = ToolBox.GetDisplacementVector(shoulderRightP, elbowRightP).Length + ToolBox.GetDisplacementVector(elbowRightP, wristRightP).Length;
			}
			boundaryCross = Math.Sqrt(Math.Pow(boundary.Width, 2) + Math.Pow(boundary.Height, 2));
			planeWidth = boundary.Width / boundaryCross * planeRadius;
			planeHeight = boundary.Height / boundaryCross * planeRadius;
			//Build New coordinate system
			//Set Middle of Visitor shoulder as origin
			Vector3D coordinateOriginP = ToolBox.GetMiddleVector(shoulderRightP, shoulderLeftP);			
			//get Relative X, Y, Z direction
			Vector3D directionX = ToolBox.GetDisplacementVector(coordinateOriginP, shoulderRightP);
			Vector3D directionY = ToolBox.GetDisplacementVector(hipP, coordinateOriginP);
			Vector3D directionZ = Vector3D.CrossProduct(directionX, directionY);
			//get coordinate unit vector 
			directionX.Normalize();
			directionY.Normalize();
			directionZ.Normalize();

			Matrix3D transformMatrix = ToolBox.NewCoordinateMatrix(directionX, directionY, directionZ);
			planeOrigin = ToolBox.GetDisplacementVector(coordinateOriginP, shoulderRightP);
			planeOrigin *= transformMatrix;

			Vector3D cursorP = ToolBox.GetMiddleVector(handRightP, wristRightP);
			cursorP = ToolBox.GetDisplacementVector(coordinateOriginP, cursorP);
			cursorP *= transformMatrix;
			cursorP = ToolBox.GetDisplacementVector(planeOrigin, cursorP);
			cursorP.Y = -cursorP.Y;

			if (cursorP.X < 0 || cursorP.Y < 0)
				return new Point3D(-1,-1,-1);
			if (cursorP.X > planeWidth || cursorP.Y >planeHeight)
				return new Point3D(-1, -1, -1);
			cursorP.X = cursorP.X / (planeWidth) * boundary.Width + boundary.X;
			cursorP.Y = cursorP.Y / (planeHeight) * boundary.Height + boundary.Y;
			return (Point3D)cursorP;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name, object value = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new ExtendedPropertyChangedEventArgs(name, value));
		}
	}

}
