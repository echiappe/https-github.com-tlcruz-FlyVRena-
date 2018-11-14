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
        //SpriteBatch spriteBatch;
        private bool start = false;
        bool startRecord = true;

        uEyeCamera cam1;
        uEyeCamera cam2;
        Frame frame;

        // Vars Image Processing
        FastBlobTracking ft;
        KalmanFilterTrack kft;
        PulsePal pulsePal;
        float[] c = new float[12];

        // Vars Stimuli
        WorldObject root = new WorldObject();
        public RenderSubsystem render;
        public UpdateSubsystem update;
        FileStream filestream;
        TextWriter textWriter;
        public VRProtocol pType;
        Stopwatch stopwatch = new Stopwatch();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 800;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferMultiSampling = true;
            this.TargetElapsedTime = TimeSpan.FromTicks(333);
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            Process process = Process.GetCurrentProcess();
            process.PriorityClass = ProcessPriorityClass.RealTime;
            process.PriorityBoostEnabled = true;
        }

        protected override void Initialize()
        {
            this.graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Services.AddService(typeof(GraphicsDeviceManager), graphics);
            Services.AddService(typeof(GraphicsDevice), this.GraphicsDevice);
            pType = new VRProtocol();
            Services.AddService(typeof(VRProtocol), pType);
            render = new RenderSubsystem(this);
            update = new UpdateSubsystem(this);
            //update.Enabled = true;
            Components.Add(render);
            Components.Add(update);
            ft = new FastBlobTracking();
            kft = new KalmanFilterTrack();
            Services.AddService(typeof(KalmanFilterTrack), kft);
            GetStimulus();
            pulsePal = new PulsePal();
            cam1 = new uEyeCamera(0, "C:\\Users\\Chiappee\\Desktop\\p1 600x600.ini", true, true, 850);
            cam2 = new uEyeCamera(1, "C:\\Users\\Chiappee\\Desktop\\p2 - 1024x544.ini", false, true, 250, pulsePal);
            filestream = File.OpenWrite("C:\\Users\\Chiappee\\Desktop\\Cameras.txt");
            textWriter = new StreamWriter(filestream);
            textWriter.Flush();
            Thread t = Thread.CurrentThread;
            t.Priority = ThreadPriority.Highest;
            Thread.BeginThreadAffinity();
            SetForegroundWindow(this.Window.Handle);
            var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            form.TopMost = true;
            form.Location = new System.Drawing.Point(-1280, 0);
            // form.Location = new System.Drawing.Point(-1280,-1280);

            c[0] = 0.0011f;
            c[1] = -0.7660f;
            c[2] = 0.0000f;
            c[3] = 0.0004f;
            c[4] = 1;
            c[5] = 0.0012f;
            c[6] = -0.0000f;
            c[7] = -0.3676f;
            c[8] = 12.5535f;
            c[9] = 4.6354f+0.07f;
            c[10] = -12.9056f;
            c[11] = -1.0711f;
            //c[0] = 0.0012f;
            //c[1] = -0.8937f;
            //c[2] = 0.0000f;
            //c[3] = 0.0005f;
            //c[4] = 1;
            //c[5] = 0.0013f;
            //c[6] = -0.0000f;
            //c[7] = -0.3741f;
            //c[8] = 11.6243f;
            //c[9] = 5.6421f;
            //c[10] = -12.9017f;
            //c[11] = -0.9338f;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //spriteBatch = new SpriteBatch(GraphicsDevice);
            while (frame == null)
            {
                cam1.queue.TryPop(out frame);
            }
            ft.SetMask(frame.image.Clone());
        }

        protected override void UnloadContent() { }

        string s;
        float[] f = new float[3];
        float[] fprev = new float[3];
        bool isjump = false;
        float[] fb = new float[3];
        //KeyboardState oldstate = new KeyboardState();
        //KeyboardState state = new KeyboardState();
        long frameCount = 0;
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) && gameTime.TotalGameTime.Seconds > 20)
            {
                start = true;
            }


            if (start)
            {
                if (startRecord)
                {
                    //cam1.RecordVideo("C:\\Users\\Chiappee\\Desktop\\1");
                    //cam2.RecordVideo("C:\\Users\\Chiappee\\Desktop\\2");
                    pulsePal.StartCommunication("COM3");
                    stopwatch.Start();
                    startRecord = false;
                    this.render.Visible = true;
                }
                if (cam1.queue.TryPop(out frame))
                {
                    frameCount = frameCount + 1;
                    if (!frame.image.IsClosed && !frame.image.IsInvalid && frame.image.Size != Size.Zero)
                    {
                        //state = Keyboard.GetState();
                        //if (oldstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up) && state.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Up))
                        //    pulsePal.auxy += 1;
                        //if (oldstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down) && state.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Down))
                        //    pulsePal.auxy -= 1;
                        //if (oldstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left) && state.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Left))
                        //    pulsePal.auxx += 1;
                        //if (oldstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right) && state.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Right))
                        //    pulsePal.auxx -= 1;
                        //if (oldstate.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z) && state.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Z))
                        //    Console.Write("");
                        pType.currentFrame = frameCount;// cam1.m_s32FrameCoutTotal;
                        pType.currentFrame2 = cam2.m_s32FrameCoutTotal;
                        pType.lostFrames = cam1.m_s32FrameLost;
                        pType.lostFrames2 = cam2.m_s32FrameLost;
                        fb = ft.GetParams(frame.image);
                        kft.filterPoints(fb);
                        f = GetVrPos(fb);
                        update.UpdateAsync(gameTime);
                        s = pType.currentFrame.ToString() + " " + pType.lostFrames.ToString() + " " + pType.currentFrame2.ToString() + " " + pType.lostFrames2.ToString() + " " + f[1].ToString() + " " + f[2].ToString() + " " + kft.pars[2].ToString();
                        FileWrite(s, textWriter);
                        if (pulsePal.queue.Count > 100)
                            pulsePal.queue.Clear();
                        pulsePal.queue.Push(f);
                        //oldstate = state;
                    }
                    frame.Dispose();
                }
                if (stopwatch.ElapsedMilliseconds >= 24 * 60 * 1000)//24*60*1000)//18 * 60 * 1000)//10 * 60 * 1000) //
                {
                    this.Exit();
                }
            }
            else
            {
                if (cam1.queue.TryPop(out frame))
                {
                    frame.Dispose();
                }
                this.render.Visible = false;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            base.Draw(gameTime);
        }

        public float[] GetVrPos(float[] pars)
        {
            float[] VRPos = new float[3];
            VRPos[0] = pType.currentFrame;
            VRPos[2] = c[11] + c[10] * (c[5] * pars[0] + c[6] * pars[1] + c[7]) / (c[2] * pars[0] + c[3] * pars[1] + c[4]);
            VRPos[1] = c[9] + c[8] * (c[0] * pars[1] + c[1]) / (c[2] * pars[0] + c[3] * pars[1] + c[4]);
            return VRPos;
        }

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
        }

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

        public WorldObject LoadStimulus(string fileName)
        {
            XmlReader reader = XmlReader.Create(fileName);
            return IntermediateSerializer.Deserialize<WorldObject>(reader, Assembly.GetExecutingAssembly().Location);
        }

        public void FileWrite(string toWrite, TextWriter textWriter)
        {
            textWriter.Write(toWrite);
            textWriter.Write("\r\n");
            textWriter.Flush();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            this.cam1.Dispose();
            this.cam2.Dispose();
            stopwatch.Stop();
            this.textWriter.Close();
            this.textWriter.Dispose();
            this.filestream.Dispose();
            this.filestream.Close();
            this.kft.Dispose();
            this.ft.Dispose();
            pulsePal.Dispose();
            Thread.EndThreadAffinity();
            this.render.Dispose();
            this.update.Dispose();
            this.Dispose();
        }
    }
}
