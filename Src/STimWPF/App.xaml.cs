﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using STimWPF.Properties;
using System.Xml;
using System.Reflection;

namespace STimWPF
{

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		public ControlWindow MainW { get; set; }
		public ContentWindow TextEntryW { get; set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			log4net.Config.XmlConfigurator.Configure();
			String destFolder = Settings.Default.DestFolder.Replace("{USERNAME}", Environment.UserName);
			Core.Instance.Initialize(Dispatcher, Settings.Default.SkeletonBufferSize, destFolder, Settings.Default.PlayerBufferSize);

			MainW = new ControlWindow(this);
			TextEntryW = new ContentWindow(this);

			MainW.Show();
			TextEntryW.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Core.Instance.Shutdown();
		}

		public void CloseApp(Window sender)
		{
			if (sender != MainW)
				MainW.Close();
			if (sender != TextEntryW)
				TextEntryW.Close();
		}

	}

}
