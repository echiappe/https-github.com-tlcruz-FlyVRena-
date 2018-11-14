using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCV.Net;

namespace VRLibrary.Visualizers
{
    public static class IplImageHelper
    {
        public static IplImage EnsureImageFormat(IplImage output, Size size, IplDepth depth, int channels)
        {
            if (output == null || output.Size != size || output.Depth != depth || output.Channels != channels)
            {
                if (output != null) output.Dispose();
                return new IplImage(size, depth, channels);
            }

            return output;
        }

        public static IplImage EnsureColorCopy(IplImage output, IplImage image)
        {
            output = EnsureImageFormat(output, image.Size, image.Depth, 3);
            if (image.Channels == 1) CV.CvtColor(image, output, ColorConversion.Gray2Bgr);
            else CV.Copy(image, output);
            return output;
        }
    }
}
