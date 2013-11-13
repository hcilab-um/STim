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

namespace SpikeWPF.Controls
{
	/// <summary>
	/// Interaction logic for AttentionTrackingControl.xaml
	/// </summary>
	public partial class AttentionTrackingControl : UserControl
	{
		public Core CoreInstance 
		{
			get { return Core.Instance; }
		}
		public AttentionTrackingControl()
		{
			InitializeComponent();
			CoreInstance.ColorImageReady += new EventHandler<ColorImageReadyArgs>(CoreInstance_ColorImageReady);
		}

		void CoreInstance_ColorImageReady(object sender, ColorImageReadyArgs e)
		{
			ImageSource colorFrame = e.Frame;
			if (colorFrame == null)
				return;
			iAttentionInfo.Source = colorFrame;
		}
	}
}
