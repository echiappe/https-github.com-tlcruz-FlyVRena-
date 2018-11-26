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
        //Vars tracking parameters
        double MinArea;
        double MaxArea;
        double bfArea;
        double contourArea;
        double thr;
        Moments moments;

        //outputs and inputs
        public float[] param;
        public IplImage Mask;
        public IplImage output;

        public FastBlobTracking()
        {
            //initialize with pre-defined parameters. Modify and re-compile 
            //to alter tracking parameters 
            MinArea = 20;
            MaxArea = 10000;
            bfArea = 0;
            thr = 30;
            output = new IplImage(new Size(600, 600), IplDepth.U8, 1);
            Mask = new IplImage(new Size(600, 600), IplDepth.U8, 1);
            param = new float[3];
            contourArea = 0;
            moments = new Moments();
        }
        
        //define the Mask used to subtract background
        public void SetMask(IplImage mask)
        {
            this.Mask = mask;
        }

        //tracking routine
        public float[] GetParams(IplImage input)
        {
            //subtract mask from frame
            CV.Sub(Mask, input, output);
            
            //threshold to remove any small background noise
            CV.Threshold(output, output, thr, 255, ThresholdTypes.Binary);

            //find countours of the rest of the pixels
            Seq currentContour;
            using (var storage = new MemStorage())
            using (var scanner = CV.StartFindContours(output, storage, Contour.HeaderSize, ContourRetrieval.External, ContourApproximation.ChainApproxNone, new Point(0, 0)))
            {
                bfArea = 0;
                while ((currentContour = scanner.FindNextContour()) != null)
                {
                    //calculate the number of pixels inside the contour
                    contourArea = CV.ContourArea(currentContour, SeqSlice.WholeSeq);

                    //if number of pixels fit the expected for the fly, calculate the distribution moments
                    if (contourArea > bfArea && (contourArea > MinArea && contourArea < MaxArea))
                    {
                        scanner.SubstituteContour(null);
                        //calculate the pixel distribution moments
                        moments = new Moments(currentContour);
                        if (moments.M00 > 0)
                        {
                            //transform moments into X,Y and orentation
                            param[0] = Convert.ToSingle(moments.M10 / moments.M00);
                            param[1] = Convert.ToSingle(moments.M01 / moments.M00);
                            param[2] = 180f * Convert.ToSingle(0.5 * Math.Atan2(2 * (moments.M11 / moments.M00 - param[0] * param[1]), (moments.M20 / moments.M00 - param[0] * param[0]) - (moments.M02 / moments.M00 - param[1] * param[1])))/Convert.ToSingle(Math.PI);
                        }
                    }
                    bfArea = contourArea;
                }
            }
            //return position and orientation
            return param;
        }

        //Dispose of all initiated objects
        public void Dispose()
        {
            output.Dispose();
        }
    }
}
