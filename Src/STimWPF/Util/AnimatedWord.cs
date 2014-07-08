using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STimWPF.Util
{
  public class AnimatedWord
  {
    public String Text { get; set; }
    public float Size { get; set; }
    public FontType Language { get; set; }
    public float Lightness { get; set; }
  }

  public enum FontType { Latin, Inuit }

}
