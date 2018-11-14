using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Visualizers;

namespace VRLibrary.ImageProcessing
{
    public class FastBlobTracking : IDisposable
    {
        double MinArea;
        double MaxArea;
        double bfArea;
        public IplImage output;
        public float[] param;
        double contourArea;
        Moments moments;
        public IplImage Mask;

        public FastBlobTracking()
        {
            MinArea = 20;
            MaxArea = 10000;
            bfArea = 0;
            output = new IplImage(new Size(600, 600), IplDepth.U8, 1);
            Mask = new IplImage(new Size(600, 600), IplDepth.U8, 1);
            param = new float[3];
            contourArea = 0;
            moments = new Moments();
        }

        public void SetMask(IplImage mask)
        {
            this.Mask = mask;
        }

        public float[] GetParams(IplImage input)
        {
            //CV.CvtColor(IplImageHelper.EnsureImageFormat(input, output.Size, IplDepth.U8, 3), output, ColorConversion.Bgr2Gray);
            CV.Sub(Mask, input, output);
            CV.Threshold(output, output, 30, 255, ThresholdTypes.Binary);
            Seq currentContour;
            using (var storage = new MemStorage())
            using (var scanner = CV.StartFindContours(output, storage, Contour.HeaderSize, ContourRetrieval.External, ContourApproximation.ChainApproxNone, new Point(0, 0)))
            {
                bfArea = 0;
                while ((currentContour = scanner.FindNextContour()) != null)
                {
                    contourArea = CV.ContourArea(currentContour, SeqSlice.WholeSeq);
                    if (contourArea > bfArea && (contourArea > MinArea && contourArea < MaxArea))
                    {
                        scanner.SubstituteContour(null);
                        moments = new Moments(currentContour);
                        if (moments.M00 > 0)
                        {
                            param[0] = Convert.ToSingle(moments.M10 / moments.M00);
                            param[1] = Convert.ToSingle(moments.M01 / moments.M00);
                            param[2] = 180f * Convert.ToSingle(0.5 * Math.Atan2(2 * (moments.M11 / moments.M00 - param[0] * param[1]), (moments.M20 / moments.M00 - param[0] * param[0]) - (moments.M02 / moments.M00 - param[1] * param[1])))/Convert.ToSingle(Math.PI);
                        }
                    }
                    bfArea = contourArea;
                }
            }
            return param;
        }

        public void Dispose()
        {
            output.Dispose();
        }
    }
}
