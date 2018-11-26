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
        // This object uses the drivers for IDS cameras, please install those first
        uEye.Camera m_Camera;
        
        //Vars Status
        public Boolean m_IsLive;
        public Int32 m_s32FrameCoutTotal;
        public uint m_s32FrameLost;
        uint m_s32FrameSaved;
        bool stack;
        bool display;

        //FrameList
        public ConcurrentStack<Frame> queue;

        //Visualizers
        TypeVisualizerDialog vis;
        ImageVisualizer imVis;
        ServiceProvider provider;

        //Object responsible to move the galvo mirrors
        PulsePal pulsePal;
        string path;

        public uEyeCamera(int ID, string parPath, bool stack, bool disp, int X)
        {
            this.stack = stack;
            this.display = disp;
            
            //check all the available IDS cameras
            m_Camera = new uEye.Camera();
            uEye.Types.CameraInformation[] cameraList;
            uEye.Info.Camera.GetCameraList(out cameraList);

            //if there are no cameras available then return with a false state
            if (cameraList.Length == 0)
            {
                m_IsLive = false;
            }
            else
            {
                //initialize the camera with the appropriate ID
                uEye.Defines.Status statusRet;
                statusRet = CameraInit(Convert.ToInt32(cameraList[ID].CameraID));

                //if initialization is successful
                if (statusRet == uEye.Defines.Status.SUCCESS)
                {
                    //try to capture a frame
                    statusRet = m_Camera.Acquisition.Capture();
                    if (statusRet != uEye.Defines.Status.SUCCESS)
                    {
                        m_IsLive = false;
                        MessageBox.Show("Starting Live Video Failed");
                    }
                    else
                    {
                        m_IsLive = true;

                        //load camera parameters from external file
                        LoadParametersFromFile(parPath);
                        m_Camera.Video.ResetCount();

                        //if frames are to save then initialize frame queue
                        if (stack)
                            queue = new ConcurrentStack<Frame>();

                        //if frames are to display initialize display window
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
                //if initialization was unsuccessful then exit
                if (statusRet != uEye.Defines.Status.SUCCESS && m_Camera.IsOpened)
                    m_Camera.Exit();
            }
        }

        public uEyeCamera(int ID, string parPath, bool stack, bool disp, int X, PulsePal pulsep)
        {
            this.stack = stack;
            this.display = disp;
            this.pulsePal = pulsep;

            //check all the available IDS cameras
            m_Camera = new uEye.Camera();
            uEye.Types.CameraInformation[] cameraList;
            uEye.Info.Camera.GetCameraList(out cameraList);

            //if there are no cameras available then return with a false state
            if (cameraList.Length == 0)
            {
                m_IsLive = false;
            }
            else
            {
                //initialize the camera with the appropriate ID
                uEye.Defines.Status statusRet;
                statusRet = CameraInit(Convert.ToInt32(cameraList[ID].CameraID));

                //if initialization is successful
                if (statusRet == uEye.Defines.Status.SUCCESS)
                {
                    //try to capture a frame
                    statusRet = m_Camera.Acquisition.Capture();
                    if (statusRet != uEye.Defines.Status.SUCCESS)
                    {
                        m_IsLive = false;
                        MessageBox.Show("Starting Live Video Failed");
                    }
                    else
                    {
                        m_IsLive = true;
                        
                        //load camera parameters from external file
                        LoadParametersFromFile(parPath);
                        m_Camera.Video.ResetCount();

                        //if frames are to save then initialize frame queue
                        if (stack)
                            queue = new ConcurrentStack<Frame>();

                        //if frames are to display initialize display window
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

                //if initialization was unsuccessful then exit
                if (statusRet != uEye.Defines.Status.SUCCESS && m_Camera.IsOpened)
                    m_Camera.Exit();
            }
        }

        //Function to initialize camera
        private uEye.Defines.Status CameraInit(Int32 camID)
        {
            uEye.Defines.Status statusRet = uEye.Defines.Status.NO_SUCCESS;
            statusRet = m_Camera.Init(camID);

            //check if camera initialized well and there is enough memory
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

            //perform onFrameEvent routine when a EventFrame occurs
            m_Camera.EventFrame += onFrameEvent;

            //if we have to move the mirrors
            if (pulsePal != null)
            {
                //move mirrors when new frame is captured: this triggering serves 
                //the porpuse of not moving the mirrors during frame acquisition
                m_Camera.EventFirstPacket += TriggerMirror;
            }

            //reset the frame count
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
            //is the camera is on, stop frame acquisition
            if (m_IsLive)
                m_Camera.Acquisition.Stop();

            //release the memory
            Int32[] memList;
            m_Camera.Memory.GetList(out memList);
            m_Camera.Memory.Free(memList);

            //load parameters from file
            m_Camera.Parameter.Load(path);

            //re-allocate memory
            m_Camera.Memory.Allocate();

            //restart frame acquisition
            if (m_IsLive)
                m_Camera.Acquisition.Capture();
        }

        public void RecordVideo(string path)
        {
            //if video already runnig, then stop it
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

            //Reset frame count and start recording vido in the defined directory
            m_Camera.Video.ResetCount();
            uEye.Defines.Status statusRet = uEye.Defines.Status.SUCCESS;
            statusRet = m_Camera.Video.Start(path + ".avi");

            if (statusRet != uEye.Defines.Status.SUCCESS)
                MessageBox.Show("Could Not Start Video Recording");
        }

        //Dispose of all initiated objects
        public void Dispose()
        {
            m_IsLive = false;
            uEye.Defines.Status statusRet = uEye.Defines.Status.SUCCESS; 

            //Termminate video recording (if initiated)
            if (m_Camera.Video.Running)
                statusRet = m_Camera.Video.Stop();

            //Termnate frame acquisition
            statusRet = m_Camera.Acquisition.Stop();
            if (m_Camera.IsOpened)
                m_Camera.Exit();

            //clear frame queue
            if (stack)
                queue.Clear();

            //close display window
            if (display)
            {
                imVis.Unload();
                vis.Dispose();
            }
        }
    }
}
