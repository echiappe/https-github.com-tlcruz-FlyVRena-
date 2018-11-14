using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace VRLibrary.ImageProcessing
{
    public class BackgroundModel
    {
        public bool IsStable;
        public double precisionStability;
        public int nFrames;
        public List<IplImage> images;
        public IplImage BackgroundMean;
        public IplImage BackgroundStd;

        public BackgroundModel()
        {
            nFrames = 50;
            precisionStability = 52100000;
            images = new List<IplImage>();
            IsStable = false;

        }

        public bool CalculateStability()
        {
            if (images.Count < nFrames)
                return false;
            else
            {
                IplImage difference = new IplImage(images.First().Size, IplDepth.U8, images.First().Channels);
                Scalar diff = new Scalar(0, 0, 0, 0);
                for (int i = 0; i < nFrames - 1; i++)
                {
                    CV.AbsDiff(images.ElementAt(i), images.ElementAt(i + 1), difference);
                    diff.Val0 = diff.Val0 + CV.Sum(difference).Val0;
                }
                if (diff.Val0 < precisionStability)
                {
                    IsStable = true;
                    return true;
                }
                else
                {
                    images.Clear();
                    return false;
                }
            }
        }

        public void AddImage(IplImage image)
        {
            if (images.Count < nFrames)
                images.Add(image);
        }

        public bool CalculateBackgroundModel()
        {
            bool re = CalculateStability();
            if (re)
            {
                BackgroundMean = new IplImage(images.First().Size, IplDepth.F32, images.First().Channels);
                BackgroundMean.SetZero();
                BackgroundStd = new IplImage(images.First().Size, IplDepth.F32, images.First().Channels);
                BackgroundStd.SetZero();
                IplImage auxImg = new IplImage(images.First().Size, IplDepth.F32, images.First().Channels);
                for (int i = 0; i < nFrames; i++)
                {
                    CV.Add(BackgroundMean, images.ElementAt(i), BackgroundMean);
                }
                CV.ConvertScale(BackgroundMean, BackgroundMean, 1.0 / images.Count, 0);

                for (int i = 0; i < nFrames; i++)
                {
                    CV.Sub(images.ElementAt(i), BackgroundMean, auxImg);
                    CV.Abs(auxImg, auxImg);
                    CV.Add(BackgroundStd, auxImg, BackgroundStd);
                }
                CV.ConvertScale(BackgroundStd, BackgroundStd, 30.0 * 1.0 / (images.Count - 1));
            }
            return re;
        }

        public IplImage BackgroudSubtractedImage(IplImage input)
        {
            IplImage output = new IplImage(images.First().Size, IplDepth.U8, images.First().Channels);
            IplImage auxImg = new IplImage(images.First().Size, IplDepth.F32, images.First().Channels);
            IplImage auxImg2 = new IplImage(images.First().Size, IplDepth.F32, images.First().Channels);
            if (IsStable)
            {
                CV.Sub(input, BackgroundMean, auxImg);
                CV.Abs(auxImg, auxImg);
                CV.Sub(auxImg, BackgroundStd, auxImg);
                CV.Threshold(auxImg, auxImg2, 0.0, 1.0, ThresholdTypes.Binary);
                CV.Threshold(auxImg, auxImg, 0.0, 255, ThresholdTypes.BinaryInv);
                CV.Mul(input, auxImg2, output);
                CV.Add(output, auxImg, output);
                return output;
            }
            else
            {
                return input;
            }
        }

    }
}
