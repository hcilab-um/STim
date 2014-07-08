using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STim.Util
{
  static class Constants
  {
    public const int VISITOR_COLOR_SHIFT = 50;
    public const int USER_COLOR_SHIFT = 40;
    public const byte MAX_INTENSITY = 255;

    public const int MINIMUM_JOINT_THRESHOLD = 5;

    public const float RENDER_WIDTH = 640.0f;
    public const float RENDER_HEIGHT = 480.0f;
    public const float PPI = 96f;
           
    public const double KINECT_DETECT_RANGE = 5;
    public const double ESTIMATE_ACCURACY = 0.0001;

    //This are the vertices index for those face features
    public const int FACE_TOP = 35;
    public const int LEFT_EYE = 19;
    public const int RIGHT_EYE = 54;
    public const int FACE_BOTTOM = 43;

    //Used for orientation tracking
    public const int PERIPHERY_MAX_ANGLE = 110;

    //Used for FaceTracker tracking
    public const int FACE_DELAY_THRESHOLD = 5;
    public const int FACE_TRACKER_CAPACITY = 6;
  }
}
