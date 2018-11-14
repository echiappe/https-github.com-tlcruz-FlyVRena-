using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Security.Permissions;

namespace VRLibrary.ExternalCamera
{
    public class VideoWriter : IDisposable
    {
        static readonly object SyncRoot = new object();
        Size writerFrameSize;
        public string FourCC { get; set; }
        public double FrameRate { get; set; }
        public Size FrameSize { get; set; }
        public SubPixelInterpolation ResizeInterpolation { get; set; }
        public OpenCV.Net.VideoWriter Writer { get; private set; }
        //public ConcurrentQueue<IplImage> queue = new ConcurrentQueue<IplImage>();
        //private readonly object _lock = new object();
        //public ManualResetEventSlim MREvent = new ManualResetEventSlim(false);

        public VideoWriter(string fileName, int channels, int width, int height, int FR, string fourcc)
        {
            FourCC = fourcc;
            FrameRate = FR;
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("A valid video file path was not specified.");
            }

            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                throw new InvalidOperationException("The specified video file path must have a valid container extension (e.g. .avi).");
            }
            Size frameSize = new Size(width, height);
            var fourCCText = FourCC;
            var fourCC = fourCCText.Length != 4 ? 0 : OpenCV.Net.VideoWriter.FourCC(fourCCText[0], fourCCText[1], fourCCText[2], fourCCText[3]);
            writerFrameSize = frameSize;
            ResizeInterpolation = SubPixelInterpolation.NearestNeighbor;
            Writer = new OpenCV.Net.VideoWriter(fileName, fourCC, FrameRate, frameSize, channels > 1);
        }

        //[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlThread)]
        //public void Write()
        //{
        //    IplImage image;
        //    Thread.BeginThreadAffinity();
        //    while (Thread.CurrentThread.IsAlive)
        //    {
        //        if (queue.TryDequeue(out image))
        //        {
        //            //if (image.Width != writerFrameSize.Width || image.Height != writerFrameSize.Height)
        //            //{
        //            //    var resized = new IplImage(new Size(writerFrameSize.Width, writerFrameSize.Height), image.Depth, image.Channels);
        //            //    CV.Resize(image, resized, ResizeInterpolation);
        //            //    image = resized;
        //            //}
        //            Writer.WriteFrame(image);
        //            image.Dispose();
        //        }
        //        else
        //        {
        //            MREvent.Wait();
        //            MREvent.Reset();
        //        }
        //    }
        //}

        //public void WriteTask(object[] stateObj)
        //{
        //    //Thread.BeginThreadAffinity();
        //    //while (queue2.Count > 0)
        //    //{
        //    //    if (queue2.TryDequeue(out image))
        //    //    {
        //    //var Writer = (OpenCV.Net.VideoWriter)stateObj[1];
        //    //var image = (IplImage)stateObj[0];
        //    //Writer.WriteFrame(image.Clone());
        //            //image.Dispose();
        //        //}
        //    //}
        //}
        public void Dispose()
        {
            Writer.Close();
            Writer.Dispose();
        }
    }
}
