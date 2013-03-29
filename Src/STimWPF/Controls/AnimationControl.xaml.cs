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
using System.Timers;
using System.ComponentModel;

namespace STimWPF.Controls
{
    /// <summary>
    /// Interaction logic for AnimationControl.xaml
    /// </summary>
    public partial class AnimationControl : UserControl
    {
        private const int MAX_TIMER_WAIT = 5000;

        private DateTime[] triggers;
        private Timer triggerTimer = new Timer(100);
        private Random randomGenerator = new Random((int)DateTime.Now.Ticks % 10000);

        public static readonly DependencyProperty ZoneProperty = DependencyProperty.Register("Zone", typeof(Interaction.Zone), typeof(AnimationControl));
        public static readonly DependencyProperty NotificationDistanceProperty = DependencyProperty.Register("NotificationDistance", typeof(double), typeof(AnimationControl));

        public Interaction.Zone Zone
        {
            get { return (Interaction.Zone)GetValue(ZoneProperty); }
            set { SetValue(ZoneProperty, value); }
        }

        public double NotificationDistance
        {
            get { return (double)GetValue(NotificationDistanceProperty); }
            set { SetValue(NotificationDistanceProperty, value); }
        }

        private Interaction.Zone copyOfZone = Interaction.Zone.Ambient;
        private double copyOfNotificationDistance = 0;
        private List<AnimatedRectControl> rectangles = new List<AnimatedRectControl>();

        public AnimationControl()
        {
            triggerTimer.Elapsed += new ElapsedEventHandler(triggerTimer_Elapsed);
            InitializeComponent();
        }

        void triggerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime start = DateTime.Now;
            triggerTimer.Enabled = false;
            if (copyOfZone == Interaction.Zone.Interaction ||
                copyOfZone == Interaction.Zone.Close) 
            {
                triggerTimer.Enabled = true;
                return;
            }

            if (copyOfZone == Interaction.Zone.Ambient)
            {
                for (int index = 0; index < triggers.Length; index++)
                {
                    if (triggers[index] <= DateTime.Now)
                    {
                        rectangles[index].AnimationStart();
                        triggers[index] = DateTime.MaxValue;
                    }
                    else
                    {
                        rectangles[index].CalculateNewSize();
                    }
                }
            }
            else if (copyOfZone == Interaction.Zone.Notification)
            {
                for (int index = 0; index < triggers.Length; index++)
                    rectangles[index].CalculateNewSize();
            }

            //Console.WriteLine("PreRender: " + (DateTime.Now - start).TotalMilliseconds);
            Dispatcher.Invoke((Action)delegate
            {
                for (int index = 0; index < triggers.Length; index++)
                    rectangles[index].UpdateUI();
            }, System.Windows.Threading.DispatcherPriority.Render, null);
            triggerTimer.Enabled = true;
            //Console.WriteLine("PostRender: " + (DateTime.Now - start).TotalMilliseconds);
        }

        private void animationControl_Loaded(object sender, RoutedEventArgs e)
        {
            int index = 0;
            foreach (UIElement ui in gRectangles.Children)
            {
                AnimatedRectControl rectangle = ui as AnimatedRectControl;
                rectangle.Tag = index++;
                rectangle.AnimationFinished += new EventHandler(AnimationControl_AnimationFinished);
                rectangle.TimeSpan = 2000;
                rectangles.Add(rectangle);
            }

            triggers = new DateTime[index];
            CalculateTriggers();
            triggerTimer.Start();
        }

        private void CalculateTriggers()
        {
            if (triggers == null)
                return;

            //A single tick represents one hundred nanoseconds or one ten-millionth of a second. There are 10,000 ticks in a millisecond.
            for (int index = 0; index < triggers.Length; index++)
                triggers[index] = new DateTime(DateTime.Now.Ticks + (randomGenerator.Next() % MAX_TIMER_WAIT) * 10000);
        }

        void AnimationControl_AnimationFinished(object sender, EventArgs e)
        {
            int index = rectangles.IndexOf(sender as AnimatedRectControl);
            triggers[index] = new DateTime(DateTime.Now.Ticks + (randomGenerator.Next() % MAX_TIMER_WAIT) * 10000);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == AnimationControl.ZoneProperty)
            {
                copyOfZone = Zone;
                if (Zone == Interaction.Zone.Ambient)
                    CalculateTriggers();
                foreach (UIElement ui in gRectangles.Children)
                    (ui as AnimatedRectControl).Zone = Zone;
            }
            else if (e.Property == AnimationControl.NotificationDistanceProperty)
            {
                copyOfNotificationDistance = NotificationDistance;
                foreach (UIElement ui in gRectangles.Children)
                    (ui as AnimatedRectControl).UserDisplayDistance = NotificationDistance;
            }
        }

    }
}
