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
using System.ComponentModel;
using Microsoft.Kinect;

namespace STimWPF
{
	/// <summary>
	/// Interaction logic for TextEntryWindow.xaml
	/// </summary>
	public partial class ContentWindow : Window
	{

		public App AppInstance { get; set; }
		
		public Core CoreInstance
		{
			get { return Core.Instance; }
		}

		public ContentWindow(App appInst)
		{
			AppInstance = appInst;
			InitializeComponent();
			CoreInstance.InteractionCtr.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(InteractionCtr_PropertyChanged);
			LocationChanged += new EventHandler(ContentWindow_LocationChanged);
			SizeChanged += new SizeChangedEventHandler(ContentWindow_SizeChanged);
		}

		void ContentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				Left = 0;
				Top = 0;
			}
			CoreInstance.InteractionCtr.MouseBoundaries = new Rect(Left, Top, ActualWidth, ActualHeight);
		}

		void ContentWindow_LocationChanged(object sender, EventArgs e)
		{
			CoreInstance.InteractionCtr.MouseBoundaries = new Rect(Left, Top, ActualWidth, ActualHeight);
		}

		private void textEntryW_Closed(object sender, EventArgs e)
		{
			AppInstance.CloseApp(this);
		}

		public void Reset()
		{
		}

		void InteractionCtr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
		}

		private void bA_Click(object sender, RoutedEventArgs e)
		{
			tb_Result.Text = tb_Result.Text + "A";
		}

		private void bB_Click(object sender, RoutedEventArgs e)
		{
			tb_Result.Text = tb_Result.Text + "B";
		}

		private void bC_Click(object sender, RoutedEventArgs e)
		{
			tb_Result.Text = tb_Result.Text + "C";
		}

		private void bD_Click(object sender, RoutedEventArgs e)
		{
			tb_Result.Text = tb_Result.Text + "D";
		}

		private void ContentWindow_Loaded(object sender, RoutedEventArgs e)
		{
			CoreInstance.DepthImageReady += new EventHandler<DepthImageReadyArgs>(CoreInstance_DepthImageReady);
		}

		void CoreInstance_DepthImageReady(object sender, DepthImageReadyArgs e)
		{
			this.iKinectDepth.Source = e.Frame;
		}
	}
}
