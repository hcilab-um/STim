using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;

using System.Globalization;
using System.Text;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Input;
using System.Collections;

namespace PhotoAlbum
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        List<VideoPlayer> _videoPlayers;
        int MaximumVideosPlayingAtOnce = 4;
        VideoPlayer activeVideoPlayer = null;
        int tagValue;
        double last_tagOrientation = -1;
        double tagOrientation = -1;
        Dictionary<string, string> descriptions;
        IEnumerable<string> images;
        int num_img_per_load = 6;
        int round = 0;
        int total_rounds;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            _videoPlayers = new List<VideoPlayer>();
            descriptions = new Dictionary<string, string>();
            PreviewTouchDown += delegate { FireClickEvent(); };
            PreviewMouseDown += delegate { FireClickEvent(); };
            TouchMove += OnTouchMove;
            LoadDescriptions();
            LoadItems();
        }

        void FireClickEvent()
        {
            
        }

        void LoadItems()
        {
            //images = Directory.GetFiles("C:\\Users\\Public\\Pictures\\Sample Pictures", "*.*", SearchOption.AllDirectories)
            images = Directory.GetFiles("C:\\museumNum", "*.*", SearchOption.AllDirectories)
                         .Where(s => s.EndsWith(".jpg")
                             || s.EndsWith(".jpeg")
                             || s.EndsWith(".png")
                             || s.EndsWith(".gif")
                             || s.EndsWith(".tif"));
            if (images.Count() % num_img_per_load == 0)
                total_rounds = images.Count() / num_img_per_load;
            else
                total_rounds = images.Count() / num_img_per_load + 1;
            

            /*
            var videos = Directory.GetFiles("C:\\Users\\Public\\Videos\\Sample Videos", "*.*", SearchOption.AllDirectories)
                         .Where(s => s.EndsWith(".wmv")
                             || s.EndsWith(".mp4"));
            */
            /*
            foreach (var imagePath in images)
            {
                var imageControl = new ImageViewer();
                var myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(imagePath);
                myBitmapImage.EndInit();
                imageControl.Source = myBitmapImage;
                imageControl.file_name = imagePath.Substring(imagePath.LastIndexOf("\\") + 1, imagePath.IndexOf(".") - (imagePath.LastIndexOf("\\") + 1));
                //Console.WriteLine("file name: " + imageControl.file_name);
                if (descriptions.ContainsKey(imageControl.file_name))
                    imageControl.TextLabel.Content = descriptions[imageControl.file_name];
                else
                    imageControl.TextLabel.Content = "No Description";

                ScatterViewItem scatterViewItem = new ScatterViewItem();
                scatterViewItem.Content = imageControl;
                ScatterContainer.Items.Add(scatterViewItem);
            }*/
            for (int i = round; i < num_img_per_load; i++)
            {
                var imagePath = images.ElementAt(i);
                var imageControl = new ImageViewer();
                var myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(imagePath);
                myBitmapImage.EndInit();
                imageControl.Source = myBitmapImage;
                imageControl.file_name = imagePath.Substring(imagePath.LastIndexOf("\\") + 1, imagePath.IndexOf(".") - (imagePath.LastIndexOf("\\") + 1));
                //Console.WriteLine("file name: " + imageControl.file_name);
                if (descriptions.ContainsKey(imageControl.file_name))
                    imageControl.TextLabel.Content = descriptions[imageControl.file_name];
                else
                    imageControl.TextLabel.Content = "No Description";

                ScatterViewItem scatterViewItem = new ScatterViewItem();
                scatterViewItem.Content = imageControl;
                ScatterContainer.Items.Add(scatterViewItem);
            }
            round++;

            /*
            foreach (var videoPath in videos)
            {
                var videoControl = new VideoPlayer { Source = videoPath, parent = this};
                var scatterView = new ScatterViewItem();
                scatterView.Content = videoControl;
                videoControl.OnVideoPlayerPlayed += videoControl_OnVideoPlayerPlayed;
                videoControl.OnVideoPlayerStopped += videoControl_VideoStopped;

                ScatterContainer.Items.Add(scatterView);
                _videoPlayers.Add(videoControl);
            }*/
        }

        void LoadDescriptions()
        {
            //string fileName = "C:\\Users\\Public\\Pictures\\Sample Pictures\\descriptions.txt";
            string fileName = "C:\\museumNum\\descriptions.txt";
            if (File.Exists(fileName))
            {
                TextReader tr = new StreamReader(fileName);
                string line = tr.ReadLine();
                while (line != null)
                {
                    string[] words = line.Split(':');
                    if (!descriptions.ContainsKey(words[0]))
                    {
                        //Console.WriteLine("key: " +words[0]);
                        descriptions.Add(words[0], words[1]);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Duplicated File Names");
                    }

                    line = tr.ReadLine();
                }
                //Console.WriteLine(gestureDict.Count);
                tr.Close();

            }
            else
            {
                MessageBox.Show("ERROR: Can not find dictionary file.");
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            ScatterContainer.Items.Clear();
            int n;
            if (round * num_img_per_load + num_img_per_load < images.Count())
                n = round * num_img_per_load + num_img_per_load;
            else
                n = images.Count();
            for (int i = round * num_img_per_load; i < n; i++)
            {
                var imagePath = images.ElementAt(i);
                var imageControl = new ImageViewer();
                var myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(imagePath);
                myBitmapImage.EndInit();
                imageControl.Source = myBitmapImage;
                imageControl.file_name = imagePath.Substring(imagePath.LastIndexOf("\\") + 1, imagePath.IndexOf(".") - (imagePath.LastIndexOf("\\") + 1));
                //Console.WriteLine("file name: " + imageControl.file_name);
                if (descriptions.ContainsKey(imageControl.file_name))
                    imageControl.TextLabel.Content = descriptions[imageControl.file_name];
                   
                else
                    imageControl.TextLabel.Content = "No Description";

                ScatterViewItem scatterViewItem = new ScatterViewItem();
                scatterViewItem.Content = imageControl;
                ScatterContainer.Items.Add(scatterViewItem);
            }
            round++;
            if (round >= total_rounds)
                round = 0;
        }

        void videoControl_OnVideoPlayerPlayed(object sender, EventArgs e)
        {
            var videoPlayersPlaying = _videoPlayers.Count(x => x.VideoIsPlaying);

            if (videoPlayersPlaying > MaximumVideosPlayingAtOnce)
            {
                var singleOrDefault = _videoPlayers.Where(x => x.VideoIsPlaying).OrderBy(x => x.PlayStarted).Take(1).SingleOrDefault();
                if (singleOrDefault != null)
                    singleOrDefault.StopVideo();
                Debug.WriteLine("Too many videos playing, stopping one");
            }
        }

        void videoControl_VideoStopped(object sender, EventArgs e)
        {
            Debug.WriteLine("Video player stopped");
            Debug.WriteLine("{0} videos currently playing", _videoPlayers.Count(x => x.VideoIsPlaying));
        }

        void StopAllVideos()
        {
            for (int i = 0; i < _videoPlayers.Count; i++)
                _videoPlayers[i].StopVideo();
        }

        public void SetActiveVideoPlayer(VideoPlayer videoplayer)
        {
            activeVideoPlayer = videoplayer;
            Console.WriteLine(activeVideoPlayer.Source);
        }

        private void OnTouchMove(object sender, TouchEventArgs args)
        {
            if (activeVideoPlayer == null) return;

            TouchDevice touchDevice = args.TouchDevice;
            tagValue = (int)touchDevice.GetTagData().Value;

            last_tagOrientation = tagOrientation;
            tagOrientation = Math.Round(touchDevice.GetOrientation(activeVideoPlayer.VideoPlayerGrid));

            if (touchDevice.GetIsTagRecognized())
            {
                //Console.WriteLine("Value: " + tagValue + " " + last_tagOrientation + " " + tagOrientation + " " + GetTanglbleRotateDirection());
                if (GetTanglbleRotateDirection() != null)
                {
                    if (GetTanglbleRotateDirection().Equals("counter clockwise"))
                        activeVideoPlayer.VideoBackward();
                    else if (GetTanglbleRotateDirection().Equals("clockwise"))
                        activeVideoPlayer.VideoForward();
                }
            }
        }

        string GetTanglbleRotateDirection()
        {
            if (last_tagOrientation == -1) return null;

            if (Math.Abs(last_tagOrientation - tagOrientation) > 30)
            {
                if (last_tagOrientation < tagOrientation)
                    return "counter clockwise";
                else
                    return "clockwise";
            }
            else
            {
                if (last_tagOrientation < tagOrientation)
                    return "clockwise";
                else
                    return "counter clockwise";
            }
        }

        private void OnItemClicked(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            StopAllVideos();
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
    }
}