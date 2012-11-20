using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Win32;
using STimWPF.Properties;
using System.IO;
using Microsoft.Kinect;
using System.Windows.Threading;
using STimWPF.Pointing;
using STimWPF.Interaction;

namespace STimWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{

		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MainWindow));

		public event PropertyChangedEventHandler PropertyChanged;

		private App appInstance = null;
		private Dispatcher uiDispatcher = null;
		private String filePath = String.Empty;
		private String initialPlaybackFolder = Settings.Default.DestFolder;

		public Core CoreInstance
		{
			get { return Core.Instance; }
		}

		public String FilePath
		{
			get { return filePath; }
			set
			{
				filePath = value;
				OnPropertyChanged("FilePath");

			}
		}

		public MainWindow(App appInst)
		{
			InitializeComponent();
			appInstance = appInst;
			uiDispatcher = Dispatcher;
		}


		private void bBrowse_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofdForm = new OpenFileDialog();
			ofdForm.Filter = "Skeleton Record (*.kr)|*.kr|All Files|*.*";
			ofdForm.FilterIndex = 1;
			ofdForm.InitialDirectory = initialPlaybackFolder;
			if (ofdForm.ShowDialog().Value)
			{
				FilePath = ofdForm.FileName;
				initialPlaybackFolder = new FileInfo(FilePath).DirectoryName;
			}
		}

		private void cbUseFile_Checked(object sender, RoutedEventArgs e)
		{
			if (cbUseFile.IsChecked == true)
			{
				if (!File.Exists(FilePath))
				{
					cbUseFile.IsChecked = false;
					return;
				}
			}
		}

		private void bStartPlay_click(object sender, RoutedEventArgs e)
		{
			if (cbUseFile.IsChecked == true)
				CoreInstance.PlayBack(tbFilePath.Text, Player_PlaybackFinished, true);
		}

		//start recording skeleton frame
		private void bRecordStart_Click(object sender, RoutedEventArgs e)
		{
			CoreInstance.Recorder.Start();
		}

		//stop recording skeleton frame
		private void bRecordPlayStop_Click(object sender, RoutedEventArgs e)
		{
			FilePath = CoreInstance.Recorder.Stop(true, false);
			CoreInstance.PlayBackFromFile = false;
		}

		public void Player_PlaybackFinished(object sender, EventArgs e)
		{
			CoreInstance.PlayBackFromFile = false;
		}

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private void mwObject_Loaded(object sender, RoutedEventArgs e)
		{
			CoreInstance.ColorImageReady += new EventHandler<ColorImageReadyArgs>(CoreInstance_ColorImageReady);
		}

		private void mwObject_Closed(object sender, EventArgs e)
		{
			appInstance.CloseApp(this);
		}

		void CoreInstance_ColorImageReady(object sender, ColorImageReadyArgs e)
		{
			ImageSource colorFrame = e.Frame;
			if (colorFrame == null)
				return;

			iSource.Source = colorFrame;
			//if (CoreInstance.Recorder.IsRecording && CoreInstance.Recorder.TotalTime >= 10)
			//  FilePath = CoreInstance.Recorder.Stop(true, false);
		}

		private void bReset_Click(object sender, RoutedEventArgs e)
		{
			CoreInstance.SkeletonF.Reset();
			CoreInstance.InteractionCtr.InteractionMethod.Reset();
			appInstance.TextEntryW.Reset();
			
		}

		private void btSelectionClick_Click(object sender, RoutedEventArgs e)
		{
			Core.Instance.InteractionCtr.HasUserClicked = true;
		}
	}
}
