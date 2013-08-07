#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "STimCamera", Category = "Transform", Help = "Basic template with one transform in/out", Tags = "matrix")]
	#endregion PluginInfo
	public class STimCameraNode : IPluginEvaluate
	{
		static readonly System.Windows.Media.Media3D.Vector3D UP_DIR = new System.Windows.Media.Media3D.Vector3D(0, 1, 0);
		static readonly System.Windows.Media.Media3D.Vector3D LOOK_DIR = new System.Windows.Media.Media3D.Vector3D(0, 0, -1);

		#region fields & pins
		[Input("CameraPosition")]
		public ISpread<Vector3D> FInCameraPosition;

		[Input("DisplayWidthInMeters", DefaultValue = 1.06)]
		private ISpread<double> FInDisplayWidthInMeters;

		[Input("DisplayHeightInMeters", DefaultValue = 0.605)]
		private ISpread<double> FInDisplayHeightInMeters;

		[Output("ViewMatrix")]
		public ISpread<Matrix4x4> FOutViewMatrix;

		[Output("ProjectionMatrix")]
		public ISpread<Matrix4x4> FOutProjectionMatrix;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutViewMatrix.SliceCount = SpreadMax;
			System.Windows.Media.Media3D.Vector3D cameraPosition = new System.Windows.Media.Media3D.Vector3D();
			cameraPosition.X = FInCameraPosition[0].x;
			cameraPosition.Y = FInCameraPosition[0].y;
			cameraPosition.Z = FInCameraPosition[0].z;

			FOutViewMatrix[0] = Math3D.SetViewMatrix(cameraPosition, LOOK_DIR, UP_DIR);

			double zn = 0.01;
			double zf = 100;
			double left = zn * (-FInDisplayWidthInMeters[0] / 2 - cameraPosition.X) / cameraPosition.Z;
			double right = zn * (FInDisplayWidthInMeters[0] / 2 - cameraPosition.X) / cameraPosition.Z;
			double bottom = zn * (-FInDisplayHeightInMeters[0] / 2 - cameraPosition.Y) / cameraPosition.Z;
			double top = zn * (FInDisplayHeightInMeters[0] / 2 - cameraPosition.Y) / cameraPosition.Z;

			FOutProjectionMatrix[0] = Math3D.SetPerspectiveOffCenter(left, right, bottom, top, zn, zf);
		}
	}
}
