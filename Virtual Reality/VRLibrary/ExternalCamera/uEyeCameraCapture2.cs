using Microsoft.Xna.Framework;
using OpenCV.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye;
using VRLibrary.Stimulus.Services;
using VRLibrary.Visualizers;

namespace VRLibrary.ExternalCamera
{
    public class uEyeCameraCapture2 : IDisposable
    {
        public bool record = false;
        public BlockingCollection<Frame> queue = new BlockingCollection<Frame>(500);
        public uint frameNo;
        public uint frameMiss;
        //private readonly object _lock = new object();
        private uEye.Camera Camera;
        private bool bLive = false;
        TypeVisualizerDialog vis;
        ImageVisualizer imVis;
        ServiceProvider provider;

        CancellationTokenSource source = new CancellationTokenSource();

        public uEyeCameraCapture2()
        {
            uEye.Types.CameraInformation[] cameraList;
            uEye.Info.Camera.GetCameraList(out cameraList);
            frameNo = 0;
            Camera = new uEye.Camera();
            Camera.Size.AOI.Set(0, 0, 1024, 544);
            uEye.Defines.Status statusRet = 0;
            // Open Camera
            statusRet = Camera.Init(Convert.ToInt32(cameraList[1].CameraID));
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
            statusRet = Camera.Parameter.Load("C:\\Users\\Chiappee\\Desktop\\p2 - 1024x544.ini");
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
            imVis.Load(provider, 800, 400);
            vis.Show();
            vis.Location = new System.Drawing.Point(100, 0);
        }

        public void StartCamera(int size)
        {
            //GC.SuppressFinalize(queue);
            Camera.EventFrame += onFrameEvent;

            Task t = Task.Factory.StartNew(() =>
            {
                int[] v = new int[size];
                for (int i = 0; i < size; i++)
                {
                    v[i] = 7200 + i * 7200;
                }
                Frame img = new Frame(new IplImage(new OpenCV.Net.Size(1024, 544), IplDepth.U8, 1), 0);
                Thread th = Thread.CurrentThread;
                th.Priority = ThreadPriority.Highest;
                Thread.BeginThreadAffinity();
                VideoWriter[] writer = new VideoWriter[size];
                for (int i = 0; i < size; i++)
                {
                    writer[i] = new VideoWriter("C:\\Users\\Chiappee\\Desktop\\Camera2_" + i.ToString() + ".avi", 1, 1024, 544, 120, "Y800");
                }
                int currentFile = 0;
                while (!source.Token.IsCancellationRequested)
                {
                    if (queue.TryTake(out img))
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
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

        int CurrSave = 0;
        private void onFrameEvent(object sender, EventArgs e)
        {
            bool miss = false;
            IntPtr handle = new IntPtr();
            uEye.Camera Camera = sender as uEye.Camera;
            Camera.Memory.GetActive(out handle);
            using (IplImage image = new IplImage(new OpenCV.Net.Size(1024, 544), IplDepth.U8, 1, handle))
            {
                
                if (record)
                {
                    frameNo = frameNo + 1;
                    //if (frameNo > 7200 * CurrSave && frameNo < 7200 * (CurrSave + 1))
                    //{
                    //    miss = queue.TryAdd(new Frame(image.Clone(), frameNo));
                    //    if (!miss)
                    //        frameMiss = frameMiss + 1;
                    //}
                    //else if (frameNo >= 7200 * (CurrSave + 1))
                    //{
                    //        CurrSave = CurrSave + 2;
                    //}
                    //queue.Enqueue(new Frame(image.Clone(), frameNo));
                    //MREvent.Set();
                    //if (frameNo % 12 == 0)
                    //{

                    //}
                }
                Task task1 = Task.Factory.StartNew(img =>
                    ShowFrame((IplImage)img), image.Clone());
            }
        }


        public void ShowFrame(IplImage img)
        {
            imVis.Show(img.Clone());
            img.Dispose();
        }

        public void FileWrite(string toWrite, TextWriter textWriter)
        {
            textWriter.Write(toWrite);
            textWriter.Write("\r\n");
            textWriter.Flush();
        }

        public void Dispose()
        {
            Camera.EventFrame -= onFrameEvent;
            Camera.Acquisition.Stop();
            source.Cancel();
            source.Dispose();
            this.imVis.Unload();
            this.vis.Dispose();
            Camera.Exit();
        }
    }
}
