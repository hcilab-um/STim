﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows;


namespace STimWPF
{

	class SkeletonDrawer
	{
		private const float RenderWidth = 640.0f;
		private const float RenderHeight = 480.0f;
		private KinectSensor kinectSensor;
		private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
		private readonly Brush inferredJointBrush = Brushes.Yellow;
		private const double JointThickness = 3;

		public SkeletonDrawer(KinectSensor kinectSensor)
		{
			this.kinectSensor = kinectSensor;
		}

		public void DrawRightArmSkeleton(Skeleton skeleton, DrawingContext drawingContext)
		{
			if (skeleton == null)
				return;
			// Right Arm
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
			this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
			this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);
			
			Joint shoulderR = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.ShoulderRight);
			Joint elbowR = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.ElbowRight);
			Joint wristR = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.WristRight);
			Joint handR = skeleton.Joints.SingleOrDefault(tmp => tmp.JointType == JointType.HandRight);
			
			RenderAJoint(drawingContext, shoulderR);
			RenderAJoint(drawingContext, elbowR);
			RenderAJoint(drawingContext, wristR);
			RenderAJoint(drawingContext, handR);
		}

		private void RenderAJoint(DrawingContext drawingContext, Joint joint)
		{
			Brush drawBrush = null;
			if (joint.TrackingState == JointTrackingState.Tracked)
			{
				drawBrush = this.trackedJointBrush;
			}
			else if (joint.TrackingState == JointTrackingState.Inferred)
			{
				drawBrush = this.inferredJointBrush;
			}

			if (drawBrush != null)
			{
				drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
			}
		}

		public void DrawUpperSkeleton(Skeleton skeleton, DrawingContext drawingContext)
		{
			if (skeleton == null)
				return;

			// Render Torso
			this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);

			// Left Arm
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
			this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
			this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

			// Right Arm
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
			this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
			this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

			// Render Joints
			foreach (Joint joint in skeleton.Joints)
			{
				RenderAJoint(drawingContext, joint);
			}
		}

		public void DrawFullSkeleton(Skeleton skeleton, DrawingContext drawingContext)
		{
			if (skeleton == null)
				return;

			// Render Torso
			this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
			this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
			this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
			this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

			// Left Arm
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
			this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
			this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

			// Right Arm
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
			this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
			this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

			// Left Leg
			this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
			this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
			this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

			// Right Leg
			this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
			this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
			this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

			// Render Joints
			foreach (Joint joint in skeleton.Joints)
			{
				RenderAJoint(drawingContext, joint);
			}
		}

		internal void DrawPoint(Skeleton skeleton, System.Windows.Media.Media3D.Vector3D? point, DrawingContext drawingContext)
		{
			if (!point.HasValue || skeleton == null)
				return;

			SkeletonPoint shoulder = skeleton.Joints[JointType.ShoulderRight].Position;
			SkeletonPoint sPoint = new SkeletonPoint() { X = (float)point.Value.X, Y = (float)point.Value.Y, Z = (float)point.Value.Z };
			SkeletonPoint result = new SkeletonPoint() { X = shoulder.X + sPoint.X, Y = shoulder.Y + sPoint.Y, Z = shoulder.Z + sPoint.Z };
			drawingContext.DrawEllipse(inferredJointBrush, null, SkeletonPointToScreen(result), JointThickness, JointThickness);
		}

		#region boneDrawing

		/// <summary>
		/// Pen used for drawing bones that are currently tracked
		/// </summary>
		private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

		/// <summary>
		/// Pen used for drawing bones that are currently inferred
		/// </summary>        
		private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

		/// <summary>
		/// Draws a bone line between two joints
		/// </summary>
		/// <param name="skeleton">skeleton to draw bones from</param>
		/// <param name="drawingContext">drawing context to draw to</param>
		/// <param name="jointType0">joint to start drawing from</param>
		/// <param name="jointType1">joint to end drawing at</param>
		private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
		{
			Joint joint0 = skeleton.Joints[jointType0];
			Joint joint1 = skeleton.Joints[jointType1];

			// If we can't find either of these joints, exit
			if (joint0.TrackingState == JointTrackingState.NotTracked ||
					joint1.TrackingState == JointTrackingState.NotTracked)
			{
				return;
			}

			// Don't draw if both points are inferred
			if (joint0.TrackingState == JointTrackingState.Inferred &&
					joint1.TrackingState == JointTrackingState.Inferred)
			{
				return;
			}

			// We assume all drawn bones are inferred unless BOTH joints are tracked
			Pen drawPen = this.inferredBonePen;
			if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
			{
				drawPen = this.trackedBonePen;
			}

			drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
		}
		#endregion

		#region skeletonPoint to 2D point translation
		/// <summary>
		/// Maps a SkeletonPoint to lie within our render space and converts to Point
		/// </summary>
		/// <param name="skelpoint">point to map</param>
		/// <returns>mapped point</returns>
		private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
		{
			// Convert point to depth space.  
			// We are not using depth directly, but we do want the points in our 640x480 output resolution.
			DepthImagePoint depthPoint;
			if (kinectSensor != null)

				depthPoint = kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
			else
				depthPoint = MapSkeletonPointToDepth(skelpoint, DepthImageFormat.Resolution640x480Fps30);
			return new Point(depthPoint.X, depthPoint.Y);
		}

		private DepthImagePoint MapSkeletonPointToDepth(SkeletonPoint skelpoint, DepthImageFormat depthImageFormat)
		{
			DepthImagePoint rPoint = new DepthImagePoint();
			rPoint.X = 0;
			rPoint.Y = 0;

			if (skelpoint.Z > _FLT_EPSILON)
			{
				rPoint.X = (int)(0.5 + skelpoint.X * (_NUI_CAMERA_COLOR_NOMINAL_FOCAL_LENGTH_IN_PIXELS / skelpoint.Z) / 640.0);
				rPoint.Y = (int)(0.5 - skelpoint.Y * (_NUI_CAMERA_COLOR_NOMINAL_FOCAL_LENGTH_IN_PIXELS / skelpoint.Z) / 480.0);
				return rPoint;
			}

			//Taken from: pytools.codeplex.com -> PyKinect-> nui -> __init__.py
			//def skeleton_to_depth_image(vPoint, scaleX = 1, scaleY = 1):
			//    """Given a Vector4 returns X and Y coordinates fo display on the screen.  Returns a tuple depthX, depthY"""
			//    if vPoint.z > _FLT_EPSILON: 
			//       ##
			//       ## Center of depth sensor is at (0,0,0) in skeleton space, and
			//       ## and (160,120) in depth image coordinates.  Note that positive Y
			//       ## is up in skeleton space and down in image coordinates.
			//       ##
			//       pfDepthX = 0.5 + vPoint.x * ( _NUI_CAMERA_SKELETON_TO_DEPTH_IMAGE_MULTIPLIER_320x240 / vPoint.z ) / 320.0
			//       pfDepthY = 0.5 - vPoint.y * ( _NUI_CAMERA_SKELETON_TO_DEPTH_IMAGE_MULTIPLIER_320x240 / vPoint.z ) / 240.0
			//       return pfDepthX * scaleX, pfDepthY * scaleY
			//    return 0.0, 0.0

			return rPoint;
		}

		private const double _NUI_CAMERA_COLOR_NOMINAL_FOCAL_LENGTH_IN_PIXELS = 531.15;   // Based on 640x480 pixel size.
		private const double _FLT_EPSILON = 1.192092896e-07;



		#endregion

	}
}
