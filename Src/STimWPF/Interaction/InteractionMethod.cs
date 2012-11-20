using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Kinect;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using STimWPF.Util;

namespace STimWPF.Interaction
{

	public abstract class InteractionMethod : INotifyPropertyChanged
	{
		//If there is a displacement of at least the value below of in the dimension of the push then the values
		// on the two other dimensions are blocked.
		private Point3D planeCenter;
		private Point3D lastPos;

		//joints
		public JointType Shoulder { get; set; }
		public JointType Elbow { get; set; }
		public JointType Wrist { get; set; }
		public JointType Hand { get; set; }
		public JointType Hip { get; set; }
		public JointType Head { get; set; }
		//this method is to be overridden by each particular typing method
		public abstract InteractionStatus ProcessNewFrame(Point3D cursor, ICollection<InteractionGesture> gestures, Skeleton stableSkeleton, double deltaTimeMilliseconds);

		public InteractionMethod()
		{
			Shoulder = JointType.Head;
			Elbow = JointType.Head;
			Wrist = JointType.Head;
			Hand = JointType.Head;
			Hip = JointType.Head;
			Head = JointType.Head;
		}

		internal virtual Point3D FindCursorPosition(Skeleton skeleton, Size layoutSize, SelectionMethod selectionM, InteractionZone interactZone)
		{
			Shoulder = JointType.ShoulderRight;
			Elbow = JointType.ElbowRight;
			Wrist = JointType.WristRight;
			Hand = JointType.HandRight;
			Hip = JointType.HipRight;
			Head = JointType.Head;
			Joint shoulder = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Shoulder);
			Joint hand = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Hand);
			Joint wrist = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Wrist);
			Joint hip = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Hip);
			Joint head = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == Head);
			double width = layoutSize.Width / 100;
			double height = layoutSize.Height / 100;

			planeCenter.X = shoulder.Position.X + width / 2;
			planeCenter.Y = shoulder.Position.Y;


			double pointerPosX = (hand.Position.X + wrist.Position.X) / 2;
			double distX = pointerPosX - planeCenter.X;
			double pointerPosY = (hand.Position.Y + wrist.Position.Y) / 2;
			double distY = pointerPosY - planeCenter.Y;
			double pointerPosZ = (hand.Position.Z + wrist.Position.Z) / 2;
			double distZ = pointerPosZ - planeCenter.Z;

			Point3D pointer = new Point3D(pointerPosX, pointerPosY, pointerPosZ);
			Point3D origin = new Point3D(shoulder.Position.X, shoulder.Position.Y, shoulder.Position.Z);
			System.Windows.Media.Media3D.Vector3D distanceFromOrigin = ToolBox.CalculateDisplacement(
				(System.Windows.Media.Media3D.Vector3D)pointer,
				(System.Windows.Media.Media3D.Vector3D)origin);

			Point3D cursorP = new Point3D(-1, -1, -1);
			if (interactZone != InteractionZone.Interaction)
				return cursorP;
			//out of width boundary
			if (Math.Abs(distX) > width / 2)
				return lastPos = cursorP;
			//out of height boundary
			if (Math.Abs(distY) > height / 2)
				return lastPos = cursorP;

			double absX = distX + width / 2;
			double absY = height - ((height / 2) + distY);

			cursorP.X = absX / width;
			cursorP.Y = absY / height;
			cursorP.Z = distZ;
			lastPos = cursorP;

			//if not close to plane, return an invalid cursorP
			return lastPos = cursorP;
		}

		public virtual void Reset()
		{
		}


		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}

}
