using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace VRLibrary.ExternalCamera
{
    //Frame object is a disposable object containing an image and a number
    public class Frame : IDisposable
    {
        public IplImage image;
        public long frameNo;
        public string status = "";

        public Frame(IplImage image, long frameNo)
        {
            this.frameNo = frameNo;
            this.image = image;
        }

        public Frame(IplImage image, long frameNo, string status)
        {
            this.frameNo = frameNo;
            this.image = image;
            this.status = status;
        }

        public Frame() {}

        public Frame Copy()
        {
            return new Frame(this.image.Clone(), this.frameNo);
        }

        public void Dispose()
        {
            image.Dispose();
        }
    }
}
