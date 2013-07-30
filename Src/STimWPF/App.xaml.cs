using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using STimWPF.Properties;
using System.Xml;
using System.Reflection;
using STim;

namespace STimWPF
{

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		public ControlWindow ControlW { get; set; }
		public ContentWindow ContentW { get; set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			log4net.Config.XmlConfigurator.Configure();

			STimSettings.CloseZoneConstrain = Settings.Default.CloseZoneConstrain;
			STimSettings.NotificationZoneConstrain = Settings.Default.NotificationZoneConstrain;

			STimSettings.BlockPercentBufferSize = Settings.Default.BlockPercentBufferSize;
			STimSettings.BlockDepthPercent = Settings.Default.BlockDepthPercent;

			STimSettings.UploadPeriod = Settings.Default.UploadPeriod;

			STimSettings.ImageFolder = Settings.Default.ImageFolder;
			STimSettings.DateTimeFileNameFormat = Settings.Default.DateTimeFileNameFormat;
			STimSettings.DateTimeLogFormat = Settings.Default.DateTimeLogFormat;

			STimSettings.DisplayWidthInMeters = Settings.Default.DisplayWidthInMeters;
			STimSettings.DisplayHeightInMeters = Settings.Default.DisplayHeightInMeters;

			STimSettings.KinectDistanceZ = Settings.Default.KinectDisplayDistanceZ;
			STimSettings.KinectDistanceY = Settings.Default.KinectDisplayDistanceY;

			STimSettings.ScreenGridRows = Settings.Default.ScreenGridRows;
			STimSettings.ScreenGridColumns = Settings.Default.ScreenGridColumns;

			STimSettings.IncludeStatusRender = Settings.Default.IncludeStatusRender;

			Core.Instance.Initialize
			(
				Dispatcher,
				log4net.LogManager.GetLogger("VisitLogger"),
				log4net.LogManager.GetLogger("StatusLogger")
			);
			
			if (Settings.Default.Testing)
			{
				ControlW = new ControlWindow(this);
				ControlW.Show();
			}

			ContentW = new ContentWindow(this);
			ContentW.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Core.Instance.Shutdown();
		}

		public void CloseApp(Window sender)
		{
			if (sender != ControlW)
				ControlW.Close();
			if (sender != ContentW)
				ContentW.Close();
		}

	}

}
