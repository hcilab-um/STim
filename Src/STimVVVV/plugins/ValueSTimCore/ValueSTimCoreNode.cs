#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using System.IO;
using STim;
using System.Windows.Threading;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "STimCore", Category = "Value", Author = "xg", Tags = "")]
	#endregion PluginInfo
	public class ValueSTimCoreNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("CloseZoneConstrain", DefaultValue = 0.5)]
		private ISpread<double> FInCloseZoneConstrain;

		[Input("InteractionZoneConstrain", DefaultValue = 1)]
		private ISpread<double> FInInteractionZoneConstrain;

		[Input("NotificationZoneConstrain", DefaultValue = 2)]
		private ISpread<double> FInNotificationZoneConstrain;

		[Input("BlockPercentBufferSize", DefaultValue = 20)]
		private ISpread<int> FInBlockPercentBufferSize;

		[Input("UploadPeriod (Millisecond)", DefaultValue = 1000)]
		private ISpread<int> FInUploadPeriod;

		[Input("ImageFolder", DefaultString = @"C:\Users\wag\Dropbox\STimStatus\Image\")]
		private ISpread<string> FInImageFolder;

		[Input("DateTimeFileNameFormat", DefaultString = "yyMMdd-HHmmss-fff")]
		private ISpread<string> FInDateTimeFileNameFormat;

		[Input("DateTimeLogFormat", DefaultString = "yyMMdd-HHmmss")]
		private ISpread<string> FInDateTimeLogFormat;

		[Input("DisplayWidthInMeters", DefaultValue = 1.06)]
		private ISpread<double> FInDisplayWidthInMeters;

		[Input("DisplayHeightInMeters", DefaultValue = 0.605)]
		private ISpread<double> FInDisplayHeightInMeters;

		[Input("KinectDisplayDistanceZ", DefaultValue = 0.167)]
		private ISpread<double> FInKinectDisplayDistanceZ;

		[Input("KinectDisplayDistanceY", DefaultValue = 0.225)]
		private ISpread<double> FInKinectDisplayDistanceY;

		[Input("BlockDepthPercent", DefaultValue = 28)]
		private ISpread<int> FInBlockDepthPercent;

		[Input("ScreenGridRows", DefaultValue = 4)]
		private ISpread<int> FInScreenGridRows;

		[Input("ScreenGridColumns", DefaultValue = 4)]
		private ISpread<int> FInScreenGridColumns;

		[Input("IncludeStatusRender", DefaultBoolean = true)]
		private ISpread<bool> FInIncludeStatusRender;

		[Input("EnableCore", DefaultBoolean = false)]
		private ISpread<bool> FInEnableCore;

		[Output("HeadLocation")]
		private ISpread<VVVV.Utils.VMath.Vector3D> FOutHeadLocation;

		[Output("StatusInit", DefaultString = "Running OK")]
		private ISpread<string> FOutExcept1;

		[Output("StatusGet", DefaultString = "Running OK")]
		private ISpread<string> FOutExcept2;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//1- Copy all the parameters to the settings object
			STimSettings.CloseZoneConstrain = FInCloseZoneConstrain[0];
			STimSettings.InteractionZoneConstrain = FInInteractionZoneConstrain[0];
			STimSettings.NotificationZoneConstrain = FInNotificationZoneConstrain[0];

			STimSettings.BlockPercentBufferSize = FInBlockPercentBufferSize[0];
			STimSettings.BlockDepthPercent = FInBlockDepthPercent[0];

			STimSettings.UploadPeriod = FInUploadPeriod[0];

			STimSettings.ImageFolder = FInImageFolder[0];
			STimSettings.DateTimeFileNameFormat = FInDateTimeFileNameFormat[0];
			STimSettings.DateTimeLogFormat = FInDateTimeLogFormat[0];

			STimSettings.DisplayWidthInMeters = FInDisplayWidthInMeters[0];
			STimSettings.DisplayHeightInMeters = FInDisplayHeightInMeters[0];

			STimSettings.KinectDistanceZ = FInKinectDisplayDistanceZ[0];
			STimSettings.KinectDistanceY = FInKinectDisplayDistanceY[0];

			STimSettings.ScreenGridRows = FInScreenGridRows[0];
			STimSettings.ScreenGridColumns = FInScreenGridColumns[0];

			STimSettings.IncludeStatusRender = FInIncludeStatusRender[0];
			//2- Initialize if it's the first time
			if (!STim.Core.Instance.IsInitialized && FInEnableCore[0])
			{
				log4net.Config.XmlConfigurator.Configure(new FileInfo("logger.xml"));
				log4net.ILog visitLogger = log4net.LogManager.GetLogger("VisitLogger");
				log4net.ILog statusLogger = log4net.LogManager.GetLogger("StatusLogger");
				try
				{
					STim.Core.Instance.Initialize(Dispatcher.CurrentDispatcher, visitLogger, statusLogger);
					FOutExcept1[0] = "OK";
				}
				catch (Exception ex)
				{
					FOutExcept1[0] = ex.Message;
				}
			}

			if (STim.Core.Instance.IsInitialized && !FInEnableCore[0])
			{
				STim.Core.Instance.Shutdown();
			}

			try
			{
				if (STim.Core.Instance.ClosestVisitor != null)
				{
					FOutHeadLocation[0] = new VVVV.Utils.VMath.Vector3D(STim.Core.Instance.ClosestVisitor.HeadLocation.X, STim.Core.Instance.ClosestVisitor.HeadLocation.Y, STim.Core.Instance.ClosestVisitor.HeadLocation.Z);
				}
				else
				{
					FOutHeadLocation[0] = new VVVV.Utils.VMath.Vector3D(0, 0, STimSettings.NotificationZoneConstrain);
				}
				FOutExcept2[0] = "OK";
			}
			catch (Exception ex)
			{
				FOutExcept2[0] = ex.Message;
			}
		}
	}
}
