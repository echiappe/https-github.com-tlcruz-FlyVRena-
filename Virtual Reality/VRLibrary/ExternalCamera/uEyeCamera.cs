using OpenCV.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VRLibrary.Visualizers;

namespace VRLibrary.ExternalCamera
{
    public class uEyeCamera : IDisposable
    {
        uEye.Camera m_Camera;
        public Boolean m_IsLive;
        public Int32 m_s32FrameCoutTotal;
        public uint m_s32FrameLost;
        uint m_s32FrameSaved;
        public ConcurrentStack<Frame> queue;
        TypeVisualizerDialog vis;
        ImageVisualizer imVis;
        ServiceProvider provider;
        bool stack;
        bool display;
        PulsePal pulsePal;
        string path;
        public uEyeCamera(int ID, string parPath, bool stack, bool disp, int X)
        {
            this.stack = stack;
            this.display = disp;
            m_Camera = new uEye.Camera();
            m_IsLive = false;
            uEye.Types.CameraInformation[] cameraList;
            uEye.Info.Camera.GetCameraList(out cameraList);
            if (cameraList.Length == 0)
            {
                m_IsLive = false;
            }
            else
            {
                uEye.Defines.Status statusRet;
                statusRet = CameraInit(Convert.ToInt32(cameraList[ID].CameraID));
                if (statusRet == uEye.Defines.Status.SUCCESS)
                {
                    statusRet = m_Camera.Acquisition.Capture();
                    if (statusRet != uEye.Defines.Status.SUCCESS)
                    {
                        MessageBox.Show("Starting Live Video Failed");
                    }
                    else
                        m_IsLive = true;
                }
                if (statusRet != uEye.Defines.Status.SUCCESS && m_Camera.IsOpened)
                    m_Camera.Exit();

                LoadParametersFromFile(parPath);

                if (stack)
                    queue = new ConcurrentStack<Frame>();

                if (display)
                {
                    provider = new ServiceProvider();
                    vis = new TypeVisualizerDialog();
                    provider.services.Add(vis);
                    imVis = new ImageVisualizer();
                    imVis.Load(provider);
                    vis.Show();
                    vis.Location = new System.Drawing.Point(X, 0);
                }
            }
        }

        public uEyeCamera(int ID, string parPath, bool stack, bool disp, int X, PulsePal pulsep)
        {
            this.stack = stack;
            this.display = disp;
            this.pulsePal = pulsep;
            m_Camera = new uEye.Camera();
            m_IsLive = false;
            uEye.Types.CameraInformation[] cameraList;
            uEye.Info.Camera.GetCameraList(out cameraList);
            if (cameraList.Length == 0)
            {
                m_IsLive = false;
            }
            else
            {
                uEye.Defines.Status statusRet;
                statusRet = CameraInit(Convert.ToInt32(cameraList[ID].CameraID));
                if (statusRet == uEye.Defines.Status.SUCCESS)
                {
                    statusRet = m_Camera.Acquisition.Capture();
                    if (statusRet != uEye.Defines.Status.SUCCESS)
                    {
                        MessageBox.Show("Starting Live Video Failed");
                    }
                    else
                        m_IsLive = true;
                }
                if (statusRet != uEye.Defines.Status.SUCCESS && m_Camera.IsOpened)
                    m_Camera.Exit();

                LoadParametersFromFile(parPath);
                m_Camera.Video.ResetCount();
                if (stack)
                    queue = new ConcurrentStack<Frame>();

                if (display)
                {
                    provider = new ServiceProvider();
                    vis = new TypeVisualizerDialog();
                    provider.services.Add(vis);
                    imVis = new ImageVisualizer();
                    imVis.Load(provider);
                    vis.Show();
                    vis.Location = new System.Drawing.Point(X, 0);
                }
            }
        }

        private uEye.Defines.Status CameraInit(Int32 camID)
        {
            uEye.Defines.Status statusRet = uEye.Defines.Status.NO_SUCCESS;
            statusRet = m_Camera.Init(camID);
            if (statusRet != uEye.Defines.Status.SUCCESS)
            {
                MessageBox.Show("Initializing the Camera Failed");
                return statusRet;
            }
            statusRet = m_Camera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.SUCCESS)
            {
                MessageBox.Show("Allocating Memory Failed");
                return statusRet;
            }
            m_Camera.EventFrame += onFrameEvent;

            if (pulsePal != null)
            {
                m_Camera.EventFirstPacket += TriggerMirror;
            }

            m_s32FrameCoutTotal = 0;
            m_Camera.Video.ResetCount();
            return statusRet;
        }

        int aux = 1;
        private void onFrameEvent(object sender, EventArgs e)
        {
            uEye.Camera camera = sender as uEye.Camera;
            if (camera.IsOpened)
            {
                ++m_s32FrameCoutTotal;
                if (camera.Video.Running)
                {
                    camera.Video.GetLostCount(out m_s32FrameLost);
                    camera.Video.GetFrameCount(out m_s32FrameSaved);
                }
                if (m_s32FrameSaved > 0 && m_s32FrameSaved >= 2 * 9000)
                {
                    RecordVideo(path + "_" + aux.ToString());
                    aux = aux + 1;
                }

                if (stack || display)
                {
                    IntPtr handle = new IntPtr();
                    camera.Memory.GetActive(out handle);
                    System.Drawing.Rectangle rect;
                    camera.Size.AOI.Get(out rect);
                    using (IplImage image = new IplImage(new OpenCV.Net.Size(rect.Width, rect.Height), IplDepth.U8, 1, handle))
                    {
                        if (stack)
                        {
                            if (queue.Count > 100)
                                queue.Clear();
                            queue.Push(new Frame(image.Clone(), m_s32FrameCoutTotal));
                        }
                        if (display)
                            imVis.Show(image.Clone());
                    }
                }
            }
        }

        private void TriggerMirror(object sender, EventArgs e)
        {
            pulsePal.MREvent.Set();
        }

        private void LoadParametersFromFile(string path)
        {
            if (m_IsLive)
                m_Camera.Acquisition.Stop();

            Int32[] memList;
            m_Camera.Memory.GetList(out memList);
            m_Camera.Memory.Free(memList);
            m_Camera.Parameter.Load(path);
            m_Camera.Memory.Allocate();

            if (m_IsLive)
                m_Camera.Acquisition.Capture();
        }

        public void RecordVideo(string path)
        {
            if (m_Camera.Video.Running)
            {
                m_Camera.Video.Stop();
            }
            else
            {
                this.path = path;
                m_s32FrameCoutTotal = 0;
                path = path + "_0";
            }
            m_Camera.Video.ResetCount();
            uEye.Defines.Status statusRet = uEye.Defines.Status.SUCCESS;
            statusRet = m_Camera.Video.Start(path + ".avi");

            if (statusRet != uEye.Defines.Status.SUCCESS)
                MessageBox.Show("Could Not Start Video Recording");
        }

        public void Dispose()
        {
            uEye.Defines.Status statusRet = uEye.Defines.Status.SUCCESS;
            m_IsLive = false;
            if (m_Camera.Video.Running)
                statusRet = m_Camera.Video.Stop();

            statusRet = m_Camera.Acquisition.Stop();
            if (m_Camera.IsOpened)
                m_Camera.Exit();

            if (stack)
                queue.Clear();

            if (display)
            {
                imVis.Unload();
                vis.Dispose();
            }
        }
    }
}
