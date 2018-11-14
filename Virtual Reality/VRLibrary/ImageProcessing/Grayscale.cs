using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace VRLibrary.ImageProcessing
{
    public class Grayscale
    {
        public IplImage ToGrayscale(IplImage input)
        {
            var output = new IplImage(input.Size, IplDepth.U8, 1);
            if (input.Channels == 3)
            {
                CV.CvtColor(input, output, ColorConversion.Bgr2Gray);
                return output;
            }
            else
            {
                return input;
            }
        }
    }
}
