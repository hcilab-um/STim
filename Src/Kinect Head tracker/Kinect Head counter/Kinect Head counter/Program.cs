using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;

namespace Kinect_Head_counter
{
    class Program
    {
        int frameNumber;
        int seconds;
        int timepassed;
        KinectSensor sensor;
        Skeleton[] skeletons;

        private void LoadKinect()
        {
            frameNumber = 0;
            timepassed = 0;
            seconds = 0;
            sensor = KinectSensor.KinectSensors[0];
            sensor.ColorFrameReady += sensor_ColorFrameReady;


            skeletons = new Skeleton[6];
            sensor.SkeletonStream.Enable();
            sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
            sensor.Start();
        }

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //frameNumber = frameNumber + 1;
            timepassed++;
            seconds = timepassed / 30;

            e.OpenSkeletonFrame().CopySkeletonDataTo(skeletons);
            int counter = 0;
            foreach(Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    counter++;
            }

            //Console.WriteLine("Frame: {0}, Number of Skeletons: {1}",  frameNumber, counter);
            if (timepassed == seconds * 30)
                Console.WriteLine("Seconds passed: {0}, Number of Skeletons: {1}", seconds, counter);

            if (counter > 0)
            {
                if (timepassed == seconds * 30)
                {
                    Console.WriteLine("Location of head: " + skeletons[counter].Joints[JointType.Head].Position.X + ", " + skeletons[counter].Joints[JointType.Head].Position.Y + ", " + skeletons[counter].Joints[JointType.Head].Position.Z + "; Found at: " + DateTime.Now);
                }
            }

            
        }

        private void UnloadKinect()
        {
            sensor.Stop();
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.LoadKinect();
            Console.Read();
            program.UnloadKinect();
        }
    }
}
