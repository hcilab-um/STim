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
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.ComponentModel;
using STimWPF.Properties;

namespace STimWPF.Controls
{
    /// <summary>
    /// Interaction logic for AnimatedRectControl.xaml
    /// </summary>
    public partial class AnimatedRectControl : UserControl, INotifyPropertyChanged
    {

        public event EventHandler AnimationFinished;

        private Interaction.Zone zone;
        private double userDisplayDistance;

        private double wPixelsPerMillisecond;
        private double hPixelsPerMillisecond;

        private AnimationStatus animationStatus;
        private DateTime lastFrameTime = DateTime.MaxValue;

        public int TimeSpan { get; set; }

        public AnimationStatus AnimationStatus
        {
            get { return animationStatus; }
            set
            {
                animationStatus = value;
                OnPropertyChanged("AnimationStatus");
            }
        }

        public Interaction.Zone Zone
        {
            get { return zone; }
            set
            {
                zone = value;
                OnPropertyChanged("Zone");
            }
        }

        public double UserDisplayDistance
        {
            get { return userDisplayDistance; }
            set
            {
                userDisplayDistance = value;
                OnPropertyChanged("UserDisplayDistance");
            }
        }

        public AnimatedRectControl()
        {
            InitializeComponent();
        }

        double calculatedWidth, calculatedHeight;
        double maxWidth, maxHeight;
        internal void CalculateNewSize()
        {
            DateTime currentFrameTime = DateTime.Now;
            TimeSpan delta = currentFrameTime - lastFrameTime;
            if (lastFrameTime == DateTime.MaxValue)
                delta = new System.TimeSpan(0);
            int deltaWPX = (int)(delta.TotalMilliseconds * wPixelsPerMillisecond);
            int deltaHPX = (int)(delta.TotalMilliseconds * hPixelsPerMillisecond);
            lastFrameTime = currentFrameTime;

            switch (AnimationStatus)
            {
                case AnimationStatus.Nothing:
                    var distance = UserDisplayDistance - Settings.Default.InteractionZoneConstrain;
                    if (distance < 0)
                        distance = 0;
                    var range = Settings.Default.NotificationZoneConstrain - Settings.Default.InteractionZoneConstrain;
                    var width = maxWidth * distance / range;
                    var height = maxHeight * distance / range;

                    calculatedWidth = width;
                    calculatedHeight = height;
                    break;
                case AnimationStatus.Decreasing:
                    if (calculatedWidth <= 0 || calculatedHeight <= 0)
                    {
                        AnimationStatus = AnimationStatus.Increasing;
                        return;
                    }

                    if (deltaWPX > calculatedWidth)
                        calculatedWidth = 0;
                    else
                        calculatedWidth -= deltaWPX;

                    if (deltaHPX > calculatedHeight)
                        calculatedHeight = 0;
                    else
                        calculatedHeight -= deltaHPX;
                    break;
                case AnimationStatus.Increasing:
                    if (calculatedWidth >= maxWidth || calculatedHeight >= maxHeight)
                    {
                        AnimationStatus = AnimationStatus.Nothing;
                        if (AnimationFinished != null)
                            AnimationFinished(this, null);
                        return;
                    }
                    calculatedWidth += deltaWPX;
                    calculatedHeight += deltaHPX;
                    break;
            }

            if (calculatedWidth == double.NaN)
            {
                Console.WriteLine(calculatedWidth);
            }

        }

        public void UpdateUI()
        {
            Canvas.SetLeft(animatedRect, (ActualWidth - calculatedWidth) / 2);
            Canvas.SetTop(animatedRect, (ActualHeight - calculatedHeight) / 2);
            animatedRect.Width = calculatedWidth;
            animatedRect.Height = calculatedHeight;
        }

        public void AnimationStart()
        {
            wPixelsPerMillisecond = (double)ActualWidth / (double)TimeSpan;
            hPixelsPerMillisecond = (double)ActualHeight / (double)TimeSpan;
            AnimationStatus = STimWPF.AnimationStatus.Decreasing;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void arControl_Loaded(object sender, RoutedEventArgs e)
        {
            maxWidth = ActualWidth;
            maxHeight = ActualHeight;
            calculatedWidth = maxWidth;
            calculatedHeight = maxHeight;
            animatedRect.Width = maxWidth;
            animatedRect.Height = maxHeight;
        }
    }
}