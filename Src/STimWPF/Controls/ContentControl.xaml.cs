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
using STimWPF.Interaction;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect;

namespace STimWPF.Controls
{
	/// <summary>
	/// Interaction logic for ContentControl.xaml
	/// </summary>
	public partial class ContentControl : UserControl, INotifyPropertyChanged, IUIInformer
	{

		private static readonly DependencyProperty KinectSensorProperty = DependencyProperty.Register("KinectSensor", typeof(KinectSensor), typeof(ContentControl));

		private bool isPlayingWF;
		private MainPage mainPageWF;
		private DetailWFPage detailWFPage;

		public KinectSensor KinectSensor
		{
			get { return (KinectSensor)GetValue(KinectSensorProperty); }
			set { SetValue(KinectSensorProperty, value); }
		}

		public bool IsPlayingWF
		{
			get { return isPlayingWF; }
			set
			{
				isPlayingWF = value;
				OnPropertyChanged("IsPlayingWF");
			}
		}

		public MainPage MainPageWF
		{
			get { return mainPageWF; }
			set
			{
				mainPageWF = value;
				OnPropertyChanged("MainPageWF");
			}
		}

		public DetailWFPage DetailWFPage
		{
			get { return detailWFPage; }
			set
			{
				detailWFPage = value;
				OnPropertyChanged("DetailWFPage");
			}
		}

		public ContentControl()
		{
			InitializeComponent();
		}

		void OnMainPageClick(object sender, RoutedEventArgs e)
		{
			KinectTileButton bt = (KinectTileButton)sender;
			string[] nameInfo = bt.Name.Split('_');
			foreach (MainPage m in Enum.GetValues(typeof(MainPage)))
			{
				if (m.ToString().Equals(nameInfo[0]))
				{
					if (nameInfo[1].Equals("WF"))
						MainPageWF = m;
					else
						throw new Exception("Invalid Name: " + nameInfo[1]);
				}
			}

			if (mainPageWF != MainPage.Overview)
			{
				IsPlayingWF = false;
				me_WF.Stop();
			}

			if (MainPageWF == MainPage.Detail)
			{
				if (nameInfo[1].Equals("WF"))
					DetailWFPage = DetailWFPage.DetailMenu_WF;
			}
		}

		void OnWFDetailClick(object sender, RoutedEventArgs e)
		{
			KinectTileButton bt = (KinectTileButton)sender;
			foreach (DetailWFPage dWFP in Enum.GetValues(typeof(DetailWFPage)))
			{
				if (dWFP.ToString().Equals(bt.Name))
					DetailWFPage = dWFP;
			}
		}

		void OnClickedPlay_WF(object sender, RoutedEventArgs e)
		{
			if (IsPlayingWF == false)
			{
				IsPlayingWF = true;
				me_WF.Play();
			}
			else
			{
				IsPlayingWF = false;
				me_WF.Pause();
			}
		}

		void onClickDetailMenu(object sender, EventArgs e)
		{
			KinectTileButton bt = (KinectTileButton)sender;
			string[] nameInfo = bt.Name.Split('_');
			if (nameInfo[1].Equals("WF"))
				DetailWFPage = DetailWFPage.DetailMenu_WF;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		public string CurrentPage()
		{
			return MainPageWF.ToString() + "-" + Detail_WF.ToString();
		}

		private void me_WF_MediaEnded(object sender, RoutedEventArgs e)
		{
			IsPlayingWF = false;
			me_WF.Stop();
		}

	}
}
