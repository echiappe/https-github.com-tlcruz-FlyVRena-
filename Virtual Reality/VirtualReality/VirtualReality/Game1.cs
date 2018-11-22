using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using OpenCV.Net;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using VRLibrary.ExternalCamera;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus;
using VRLibrary.Stimulus.Subsystems;
using VRLibrary.Stimulus.ServiceFactories;
using VRLibrary.Stimulus.Services;

namespace VirtualReality
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        private bool start = false;
        bool startRecord = true;

        // Vars Frame Acquisition
        uEyeCamera cam1;
        uEyeCamera cam2;
        PulsePal pulsePal;
        Frame frame;

        // Vars Image Processing
        FastBlobTracking ft;
        KalmanFilterTrack kft;
        float[] c = new float[12];

        // Vars Stimuli
        WorldObject root = new WorldObject();
        ExperimentProtocol EP;
        public RenderSubsystem render;
        public UpdateSubsystem update;

        // Vars Data Storage
        FileStream filestream;
        TextWriter textWriter;
        public VRProtocol pType;
        Stopwatch stopwatch = new Stopwatch();

        //Set the VR window to foreground
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public Game1()
        {
            // Define the VR window properties
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 800;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferMultiSampling = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            this.TargetElapsedTime = TimeSpan.FromTicks(333);
            graphics.ApplyChanges();

            // Set application priority to maximum
            Content.RootDirectory = "Content";
            Process process = Process.GetCurrentProcess();
            process.PriorityClass = ProcessPriorityClass.RealTime;
            process.PriorityBoostEnabled = true;

            // Set current Thread to maximum priority
            Thread t = Thread.CurrentThread;
            t.Priority = ThreadPriority.Highest;
            Thread.BeginThreadAffinity();
            SetForegroundWindow(this.Window.Handle);

            // Set the location of the VR window
            var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            form.TopMost = true;
            form.Location = new System.Drawing.Point(-1280, 0);
        }

        protected override void Initialize()
        {
            // Add some objects as services
            Services.AddService(typeof(GraphicsDeviceManager), graphics);
            Services.AddService(typeof(GraphicsDevice), this.GraphicsDevice);

            // Initialize vr components
            render = new RenderSubsystem(this);
            update = new UpdateSubsystem(this);
            Components.Add(render);
            Components.Add(update);

            // Load virtual world
            GetStimulus();
            pType = new VRProtocol();
            pType.tDuration = EP.durationTrial;
            pType.trials = EP.stimTypes;
            Services.AddService(typeof(VRProtocol), pType);

            // Initialize frame acquisition objects
            pulsePal = new PulsePal();
            cam1 = new uEyeCamera(0, "C:\\Users\\Chiappee\\Desktop\\p1 600x600.ini", true, true, 850);
            cam2 = new uEyeCamera(1, "C:\\Users\\Chiappee\\Desktop\\p2 - 1024x544.ini", false, true, 250, pulsePal);

            if (cam1.m_IsLive == true)
            {
                // Initialize objects for online tracking
                ft = new FastBlobTracking();
                kft = new KalmanFilterTrack();
                Services.AddService(typeof(KalmanFilterTrack), kft);

                // Initalize file for data storage
                filestream = File.OpenWrite("C:\\Users\\Chiappee\\Desktop\\Cameras.txt");
                textWriter = new StreamWriter(filestream);
                textWriter.Flush();

                // Calibration values
                c[0] = 0.0011f;
                c[1] = -0.7660f;
                c[2] = 0.0000f;
                c[3] = 0.0004f;
                c[4] = 1;
                c[5] = 0.0012f;
                c[6] = -0.0000f;
                c[7] = -0.3676f;
                c[8] = 12.5535f;
                c[9] = 4.6354f + 0.07f;
                c[10] = -12.9056f;
                c[11] = -1.0711f;
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
            if (cam1.m_IsLive == true)
            {
                //Set the first frame as background
                while (frame == null)
                {
                    cam1.queue.TryPop(out frame);
                }
                ft.SetMask(frame.image.Clone());
            }
        }

        protected override void UnloadContent() { }

        // Initalize auxiliary variables
        string s;
        float[] f = new float[3];
        float[] fprev = new float[3];
        float[] fb = new float[3];
        long frameCount = 0;
        protected override void Update(GameTime gameTime)
        {
            // if ESC exit the program
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();

            // Start the experiment by pressing ENTER after a 20s period
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) && gameTime.TotalGameTime.Seconds > 20)
            {
                start = true;
            }

            if (start)
            {
                if (cam1.m_IsLive)
                {
                    if (startRecord)
                    {
                        //Select to store video recordings
                        //cam1.RecordVideo("C:\\Users\\Chiappee\\Desktop\\1");
                        //cam2.RecordVideo("C:\\Users\\Chiappee\\Desktop\\2");
                        pulsePal.StartCommunication("COM3");

                        // Start experiment timer
                        stopwatch.Start();
                        startRecord = false;
                        this.render.Visible = true;
                    }

                    // if a frame is acquired
                    if (cam1.queue.TryPop(out frame))
                    {
                        frameCount = frameCount + 1;
                        // if the frame is valid
                        if (!frame.image.IsClosed && !frame.image.IsInvalid && frame.image.Size != Size.Zero)
                        {
                            // save frame properties 
                            pType.currentFrame = frameCount;// cam1.m_s32FrameCoutTotal;
                            pType.currentFrame2 = cam2.m_s32FrameCoutTotal;
                            pType.lostFrames = cam1.m_s32FrameLost;
                            pType.lostFrames2 = cam2.m_s32FrameLost;

                            // Get the values from the online tracking
                            fb = ft.GetParams(frame.image);
                            kft.filterPoints(fb);
                            f = GetVrPos(fb);

                            // Call all objects from the virtual world to update
                            update.UpdateAsync(gameTime);

                            // Save the data
                            s = pType.currentFrame.ToString() + " " + pType.lostFrames.ToString() + " " + pType.currentFrame2.ToString() + " " + pType.lostFrames2.ToString() + " " + f[1].ToString() + " " + f[2].ToString() + " " + kft.pars[2].ToString();
                            FileWrite(s, textWriter);

                            // Algin the galvo mirrors
                            if (pulsePal.queue.Count > 100)
                                pulsePal.queue.Clear();
                            pulsePal.queue.Push(f);
                        }
                        // Dispose of the acquired frame
                        frame.Dispose();
                    }

                    // If timer passes the duration of the experiment, terminate the program
                    if (stopwatch.ElapsedMilliseconds >= this.EP.duration * 60 * 1000)//24*60*1000)//18 * 60 * 1000)//10 * 60 * 1000) //
                    {
                        this.Exit();
                    }
                }
                else
                {
                    if (startRecord)
                    {
                        stopwatch.Start();
                        startRecord = false;
                        this.render.Visible = true;
                    }
                    if (stopwatch.ElapsedMilliseconds >= this.EP.duration * 60 * 1000)//24*60*1000)//18 * 60 * 1000)//10 * 60 * 1000) //
                    {
                        this.Exit();
                    }
                }
            }
            else
            {
                if (cam1.m_IsLive == true)
                {
                    if (cam1.queue.TryPop(out frame))
                    {
                        frame.Dispose();
                    }
                }
                this.render.Visible = false;
            }
            base.Update(gameTime);
        }

        // Render the virtual world
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            base.Draw(gameTime);
        }

        // Auxliary function to calibrate the onlne tracking data
        public float[] GetVrPos(float[] pars)
        {
            float[] VRPos = new float[3];
            VRPos[0] = pType.currentFrame;
            VRPos[2] = c[11] + c[10] * (c[5] * pars[0] + c[6] * pars[1] + c[7]) / (c[2] * pars[0] + c[3] * pars[1] + c[4]);
            VRPos[1] = c[9] + c[8] * (c[0] * pars[1] + c[1]) / (c[2] * pars[0] + c[3] * pars[1] + c[4]);
            return VRPos;
        }

        // Funtion to open a new dialog window toload the virtual world file
        public void GetStimulus()
        {
            Thread t = new Thread((ThreadStart)(() =>
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.InitialDirectory = Assembly.GetExecutingAssembly().Location;
                fileDialog.Title = "Open File";
                fileDialog.Filter = "XML Files (*.xml)|*.xml|" +
                                        "All Files (*.*)|*.*";

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    root = LoadStimulus(fileDialog.FileName);
                }
            }));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (root != null)
            {
                Services.AddService(typeof(WorldObject), root);
                Init(root);
            }
            ExperimentProtocolFactory epf = (ExperimentProtocolFactory)root.objectBuilder[0];
            epf.Initialize(root, this);
            this.EP = (ExperimentProtocol)root.GetService(typeof(ExperimentProtocol));
        }

        // Function to initialize the virtual world
        public void Init(WorldObject WObj)
        {
            foreach (WorldObject obj in WObj.WObjects)
            {
                if (obj.GetService(typeof(NameService)) == null)
                {
                    foreach (ServiceFactory s in obj.objectBuilder)
                    {
                        s.Initialize(obj, this);
                    }
                    Init(obj);
                }
            }
        }

        // Auxiliary function to load XML file
        public WorldObject LoadStimulus(string fileName)
        {
            XmlReader reader = XmlReader.Create(fileName);
            return IntermediateSerializer.Deserialize<WorldObject>(reader, Assembly.GetExecutingAssembly().Location);
        }

        // Auxiliary function to write the data log file
        public void FileWrite(string toWrite, TextWriter textWriter)
        {
            textWriter.Write(toWrite);
            textWriter.Write("\r\n");
            textWriter.Flush();
        }

        // Disposal of all initiated objects
        protected override void OnExiting(object sender, EventArgs args)
        {
            if (cam1.m_IsLive == true)
            {
                this.cam1.Dispose();
                if (cam2.m_IsLive == true)
                    this.cam2.Dispose();
                stopwatch.Stop();

                this.textWriter.Close();
                this.textWriter.Dispose();
                this.filestream.Dispose();
                this.filestream.Close();
                this.kft.Dispose();
                this.ft.Dispose();
                pulsePal.Dispose();
            }
            Thread.EndThreadAffinity();
            this.render.Dispose();
            this.update.Dispose();
            this.Dispose();
        }
    }
}
