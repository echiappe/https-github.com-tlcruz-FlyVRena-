using OpenCV.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye;
using VRLibrary.Visualizers;

namespace VRLibrary.ExternalCamera
{
    public class uEyeCameraCapture : IDisposable
    {
        public ConcurrentStack<Frame> queue = new ConcurrentStack<Frame>();
        public BlockingCollection<Frame> queueR = new BlockingCollection<Frame>(500);
        //public ManualResetEventSlim MREvent = new ManualResetEventSlim(false);
        CancellationTokenSource source = new CancellationTokenSource();
        public bool record = false;
        public uint frameNo;
        public uint frameMiss;
        //private readonly object _lock = new object();
        private uEye.Camera Camera;
        private bool bLive = false;
        TypeVisualizerDialog vis;
        ImageVisualizer imVis;
        ServiceProvider provider;
        

        public uEyeCameraCapture()
        {
            uEye.Types.CameraInformation[] cameraList;
            uEye.Info.Camera.GetCameraList(out cameraList);
            frameNo = 0;
            Camera = new uEye.Camera();
            Camera.Size.AOI.Set(354, 0, 600, 600);
            uEye.Defines.Status statusRet = 0;
            // Open Camera
            statusRet = Camera.Init(Convert.ToInt32(cameraList[0].CameraID));
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Camera initializing failed");
                Environment.Exit(-1);
            }

            // Allocate Memory
            statusRet = Camera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Allocate Memory failed");
                Environment.Exit(-1);
            }

            // Start Live Video
            statusRet = Camera.Acquisition.Capture();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Start Live Video failed");
            }
            else
            {
                bLive = true;
            }

            Camera.Acquisition.Stop();

            Int32[] memList;
            statusRet = Camera.Memory.GetList(out memList);
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Get memory list failed: " + statusRet);
                Environment.Exit(-1);
            }

            statusRet = Camera.Memory.Free(memList);
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Free memory list failed: " + statusRet);
                Environment.Exit(-1);
            }

            statusRet = Camera.Parameter.Load("C:\\Users\\Chiappee\\Desktop\\p1 600x600.ini");
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Loading parameter failed: " + statusRet);
            }

            statusRet = Camera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.SUCCESS)
            {
                MessageBox.Show("Allocate Memory failed");
                Environment.Exit(-1);
            }

            if (bLive == true)
            {
                Camera.Acquisition.Capture();
            }
            provider = new ServiceProvider();
            vis = new TypeVisualizerDialog();
            provider.services.Add(vis);
            imVis = new ImageVisualizer();
            imVis.Load(provider, 512, 512);
            vis.Show();
            vis.Location = new System.Drawing.Point(950, 0);
        }

        public void StartCamera(int size)
        {
            Camera.EventFrame += onFrameEvent;

            Task t = Task.Factory.StartNew(() =>
            {
                int[] v = new int[size];
                for (int i = 0; i < size; i++)
                {
                    v[i] = 3600 + i * 3600;
                }
                Frame img = new Frame(new IplImage(new OpenCV.Net.Size(600, 600), IplDepth.U8, 1), 0);
                Thread th = Thread.CurrentThread;
                th.Priority = ThreadPriority.Highest;
                Thread.BeginThreadAffinity();
                VideoWriter[] writer = new VideoWriter[size];
                for (int i = 0; i < size; i++)
                {
                    writer[i] = new VideoWriter("C:\\Users\\Chiappee\\Desktop\\Camera1_" + i.ToString() + ".avi", 1, 600, 600, 60, "Y800");
                }
                int currentFile = 0;

                while (!source.Token.IsCancellationRequested)
                {
                    if (queueR.TryTake(out img))
                    {
                        writer[currentFile].Writer.WriteFrame(img.image);
                        if (img.frameNo > v[currentFile])
                            currentFile++;
                        img.Dispose();
                    }
                }

                for (int i = 0; i < size; i++)
                {
                    writer[i].Dispose();
                }
                Thread.EndThreadAffinity();
                img.Dispose();
            },
            source.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        }

        public void StartRecording()
        {
            record = true;
        }

        bool miss = false;
        private void onFrameEvent(object sender, EventArgs e)
        {
            IntPtr handle = new IntPtr();
            uEye.Camera Camera = sender as uEye.Camera;
            Camera.Memory.GetActive(out handle);

            using (IplImage image = new IplImage(new OpenCV.Net.Size(600, 600), IplDepth.U8, 1, handle))
            {
                if (queue.Count > 60)
                    queue.Clear();

                queue.Push(new Frame(image.Clone(), frameNo));
                frameNo = frameNo + 1;
                if (record)
                {
                    frameNo = frameNo + 1;
                    Frame f = new Frame(image.Clone(), frameNo);
                    miss = queueR.TryAdd(f);
                    if (!miss)
                    {
                        frameMiss = frameMiss + 1;
                        f.Dispose();
                    }
                }
                //if (frameNo % 6 == 0)
                //{
                    Task task1 = Task.Factory.StartNew(img =>
                         ShowFrame((IplImage)img), image.Clone());
                //}
            }
        }

        public void ShowFrame(IplImage img)
        {
            imVis.Show(img.Clone());
            img.Dispose();
        }

        public void Dispose()
        {
            Camera.EventFrame -= onFrameEvent;
            Camera.Acquisition.Stop();
            source.Cancel();
            source.Dispose();
            this.imVis.Unload();
            this.vis.Dispose();
            Camera.Video.Stop();
            Camera.Exit();
        }
    }
}
