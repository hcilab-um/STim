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

namespace STimWPF.Interaction
{

	public class InteractionController : INotifyPropertyChanged
	{
		private int currentFrame = 1;// totalFrames = 0;
		private Object processingLock = new Object();
		private EventWaitHandle processingWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);

		private Point3D cursorLocation;
		private Point3D secCursorLocation;
		private SelectionMethod selectionMethod;
		private InteractionZone interactionZone;

		private Point3D planeOrigin;
		private double planeRadius;

		//joints
		public JointType Shoulder { get; set; }
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
			secCursorLocation = new Point3D(0, 0, 0);
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
				ICollection<InteractionGesture> gestures = Recognizer.ProcessGestures(skeleton, deltaMilliseconds, cursorLocation, secCursorLocation, selectionMethod, HasUserClicked);
				if (gestures != null && gestures.Count > 0 && gestures.ElementAt(0).Type == GestureType.Tap)
				{
					leftClick = true;
				}
				MouseController.SendMouseInput((int)CursorLocation.X, (int)CursorLocation.Y, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);
			}
		}

		private Point3D FindCursorPosition(Skeleton skeleton, Rect boundary, SelectionMethod selectionM)
		{
			Shoulder = JointType.ShoulderRight;
			Elbow = JointType.ElbowRight;
			Wrist = JointType.WristRight;
			Hand = JointType.HandRight;
			Hip = JointType.HipRight;
			Head = JointType.Head;

			Joint shoulder = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Shoulder);
			Joint elbow = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Elbow);
			Joint wrist = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Wrist);
			Joint hand = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Hand);
			Joint hip = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Hip);
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Head);
			
			if (planeRadius == -1)
			{
				System.Windows.Media.Media3D.Vector3D elbowP = new System.Windows.Media.Media3D.Vector3D(elbow.Position.X, elbow.Position.Y, elbow.Position.Z);
				System.Windows.Media.Media3D.Vector3D shoulderP = new System.Windows.Media.Media3D.Vector3D(shoulder.Position.X, shoulder.Position.Y, shoulder.Position.Z);
				System.Windows.Media.Media3D.Vector3D wristP = new System.Windows.Media.Media3D.Vector3D(wrist.Position.X, wrist.Position.Y, wrist.Position.Z);
				planeRadius = ToolBox.CalculateDisplacement(elbowP, shoulderP).Length + ToolBox.CalculateDisplacement(elbowP, wristP).Length/2;
			}
			planeOrigin.X = shoulder.Position.X - planeRadius;
			planeOrigin.Y = shoulder.Position.Y + planeRadius;
			Point3D cursorP = new Point3D(-1, -1, -1);

			double pointerPosX = (hand.Position.X + wrist.Position.X) / 2;
			double distX = pointerPosX - planeOrigin.X;
			double pointerPosY = (hand.Position.Y + wrist.Position.Y) / 2;
			double distY = planeOrigin.Y - pointerPosY;
			if (distX < 0 || distY < 0)
				return cursorP;
			if (distX > 2 * planeRadius || distY > 2 * planeRadius)
				return cursorP;
			cursorP.X = distX / (2 * planeRadius) * boundary.Width + boundary.X;
			cursorP.Y = distY / (2 * planeRadius) * boundary.Height + boundary.Y;
			return cursorP;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name, object value = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new ExtendedPropertyChangedEventArgs(name, value));
		}
	}

}
