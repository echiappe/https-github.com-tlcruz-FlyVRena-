using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Permissions;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using OpenCV.Net;

namespace VRLibrary.ExternalCamera
{
    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlThread)]
    public class CameraCapture
    {
        /* Import functions to read CPU time */
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        //long count;
        //long freq;

        Capture capture;

        bool start;
        public ConcurrentStack<IplImage> queue = new ConcurrentStack<IplImage>();
        private readonly object _lock = new object();

        public CameraCapture(int index)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            capture = OpenCV.Net.Capture.CreateCameraCapture(index);
            var image = capture.QueryFrame();
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlThread)]
        public void GrabImages()
        {
            if (start)
            {
                Thread.BeginThreadAffinity();
                start = false;
            }
            var image = capture.QueryFrame();
            while (image != null)
            {
                image = capture.QueryFrame();
                lock (_lock)
                {
                    queue.Push(image.Clone());
                }
            }
        }
    }
}
