using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using System.Globalization;
using System.Text;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;

namespace PhotoAlbum
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    public partial class VideoPlayer : UserControl
    {
        public string Source { get { return (string)GetValue(_sourceProperty); } set { SetValue(_sourceProperty, value); } }
        public static readonly DependencyProperty _sourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(VideoPlayer), new FrameworkPropertyMetadata(String.Empty));

        public double CurrentVideoProgress { get { return (double)GetValue(_currentVideoProgress); } set { SetValue(_currentVideoProgress, value); } }
        public static readonly DependencyProperty _currentVideoProgress = DependencyProperty.Register("CurrentVideoProgress", typeof(double), typeof(VideoPlayer), new FrameworkPropertyMetadata((double)0));

		public bool VideoIsPlaying { get { return (bool)GetValue(_videoIsPlaying); } set { SetValue(_videoIsPlaying, value); } }
		public static readonly DependencyProperty _videoIsPlaying = DependencyProperty.Register("VideoIsPlaying", typeof(bool), typeof(VideoPlayer), new FrameworkPropertyMetadata(false));

        private Timer _playTimer;

        public DateTime? PlayStarted { get; set; }

        public SurfaceWindow1 parent { get; set; }

        public event ChangedEventHandler OnVideoPlayerPlayed;
        protected virtual void OnPlayed(EventArgs e)
        {
            PlayStarted = DateTime.Now;
            if (OnVideoPlayerPlayed != null)
                OnVideoPlayerPlayed(this, e);
        }
        public event ChangedEventHandler OnVideoPlayerStopped;
        protected virtual void OnStopped(EventArgs e)
        {
            PlayStarted = null;
            if (OnVideoPlayerStopped != null)
                OnVideoPlayerStopped(this, e);
        }

        public VideoPlayer()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(VideoPlayerLoaded);
            TouchDown += OnTouchDown;
        }

        private void OnTouchDown(object sender, TouchEventArgs args)
        {
            Console.WriteLine("touch down");
            parent.SetActiveVideoPlayer(this);    
        }

        void VideoPlayerLoaded(object sender, RoutedEventArgs e)
        {
            videoPlayer.MediaEnded += delegate(object o, RoutedEventArgs args)
            {
                videoPlayer.Position = new TimeSpan(0, 0, 0, 0);
                videoPlayer.Play();
            };

            _playTimer = new Timer {Interval = 300};
            _playTimer.Elapsed += delegate(object o, ElapsedEventArgs args)
                                      {
                                          Application.Current.Dispatcher.BeginInvoke(
                                              DispatcherPriority.Background,
                                              new Action(() => CurrentVideoProgress =
                                                               videoPlayer.Position.TotalMilliseconds/
                                                               videoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds));
                                      };
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayVideo();
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.Position = new TimeSpan(0, 0, 0, 0);
        }

        public void VideoForward()
        {
            videoPlayer.Position += TimeSpan.FromSeconds(10);
            if (videoPlayer.Position >= videoPlayer.NaturalDuration.TimeSpan)
                videoPlayer.Position = videoPlayer.NaturalDuration.TimeSpan;
        }

        public void VideoBackward()
        {
            videoPlayer.Position -= TimeSpan.FromSeconds(10);
            if (videoPlayer.Position <= new TimeSpan(0, 0, 0, 0))
                videoPlayer.Position = new TimeSpan(0, 0, 0, 0);
        }

        public void StopVideo()
        {
            videoPlayer.Pause();
            _playTimer.Stop();
            VideoIsPlaying = false;
            OnStopped(EventArgs.Empty);
        }

        public void PlayVideo()
        {
            videoPlayer.Play();
            _playTimer.Start();
            VideoIsPlaying = true;
            OnPlayed(EventArgs.Empty);
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            StopVideo();
        }

        private void videoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            ((MediaElement)sender).Play();
            ((MediaElement)sender).Position = new TimeSpan(0, 0, 0, 1);
            ((MediaElement)sender).Pause();
        }

        private void PlayButtonSmall_Click(object sender, RoutedEventArgs e)
        {
            PlayVideo();
        }
    }
}
