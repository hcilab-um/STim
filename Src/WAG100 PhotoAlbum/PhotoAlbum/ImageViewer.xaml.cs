using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;

namespace PhotoAlbum
{
    public partial class ImageViewer : UserControl
    {
        public ImageSource Source { get { return (ImageSource)GetValue(_source); } set { SetValue(_source, value); } }
        public static readonly DependencyProperty _source = DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageViewer), new FrameworkPropertyMetadata(null));
        public string file_name { get; set; }
        public bool show_text = false;

        public ImageViewer()
        {
            InitializeComponent();
        }

        private void ShowTextButton_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("show text: " + file_name);
            if (show_text)
            {
                show_text = false;
                TextLabel.Visibility = System.Windows.Visibility.Hidden;
                TextBorder.Visibility = System.Windows.Visibility.Hidden;
                
             }
            else
            {
                show_text = true;
                TextLabel.Visibility = System.Windows.Visibility.Visible;
                TextBorder.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
