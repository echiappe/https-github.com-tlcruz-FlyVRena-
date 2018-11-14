using OpenCV.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace VRLibrary.ExternalCamera
{
    public class FileCapture : IDisposable
    {
        private Capture capture;
        private double captureFps;
        private bool start = true;
        public ConcurrentStack<Frame> queue = new ConcurrentStack<Frame>();
        private readonly object _lock = new object();

        public FileCapture(string FileName, int fps)
        {
            capture = Capture.CreateFileCapture(FileName);
            captureFps = fps;
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlThread)]
        public void GrabImages()
        {
            Frame currentframe = new Frame();
            double capFps = captureFps;
            long frameNumber = 0;
            double dueTime = 0;
            if (start)
            {
                Thread.BeginThreadAffinity();
                start = false;
            }
            var stopwatch = new Stopwatch();
            var sampleSignal = new ManualResetEvent(false);
            currentframe.image = capture.QueryFrame();

            while (currentframe.image != null)
            {
                stopwatch.Restart();
                frameNumber = frameNumber + 1;
                currentframe.image = capture.QueryFrame();
                currentframe.frameNo = frameNumber;
                if (queue.Count > 60)
                {
                    lock (_lock)
                    {
                        queue.Clear();
                        queue.Push(currentframe);
                    }
                }
                else
                {
                    lock (_lock)
                    {
                        queue.Push(currentframe);
                    }
                }
                dueTime = Math.Max(0, (1000.0 / capFps) - stopwatch.Elapsed.TotalMilliseconds);
                if (dueTime > 0)
                {
                    sampleSignal.WaitOne(TimeSpan.FromMilliseconds(dueTime));
                }
            }
            Thread.EndThreadAffinity();
        }

        public void Dispose()
        {
            capture.Dispose();
        }
    }
}
