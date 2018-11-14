using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Stimulus.Services;

namespace VRLibrary.Stimulus.Subsystems
{
    /* Subsystem for render the world */
    public class RenderSubsystem : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public bool enable = true;
        WorldObject syncObj;
        WorldObject cam;
        CameraService camServ;
        public Color[] syncColor;
        RenderTarget2D renderSync;
        Model syncMod;
        PositionService synchPos;
        public bool isSynch = false;
        Effect effect;
        GraphicsDevice graphicsDevice;
        public RenderSubsystem(Game game)
            : base(game)
        {
            /* Add this subsystem as a component of the game */
            game.Services.AddService(typeof(RenderSubsystem), this);
            graphicsDevice = (GraphicsDevice)game.Services.GetService(typeof(GraphicsDevice));
            effect = game.Content.Load<Effect>("Effect");
            this.Enabled = false;
        }
        // List of services registered to draw and a camera (only one camera should be attributed at a time)
        public Dictionary<string, DrawService> DrawServices = new Dictionary<string, DrawService>();

        // Add a WO to the draw list
        public void AddDrawService(string name, DrawService obj)
        {
            this.DrawServices.Add(name, obj);
        }

        // Count number of WO are registered (except for the camera)
        public int NumberOfDrawObjects()
        {
            int i = 0;
            foreach (string name in DrawServices.Keys)
            {
                i++;
            }
            return i - 1;
        }

        public override void Initialize() {}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            this.Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            this.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            foreach (string n in DrawServices.Keys)
            {
                if (n != "Camera")
                {
                    DrawModel(DrawServices[n].model, DrawServices[n].pos.PosMatrix(), DrawServices["Camera"].cam.CamView(), DrawServices["Camera"].cam.CamPerspective());
                }
            }
            //if (isSynch)
            //{
            //    foreach (ModelMesh mesh in syncMod.Meshes)
            //    {
            //        foreach (ModelMeshPart part in mesh.MeshParts)
            //        {
            //            part.Effect = effect;
            //        }
            //        mesh.Draw();
            //    }
            //}
            base.Draw(gameTime);
        }

        public new void Dispose()
        {
            //renderSync.Dispose();
            effect.Dispose();
            graphicsDevice.Dispose();
        }
        // Draw the model without special effects 
        public void DrawModel(Model model, Matrix world, Matrix view, Matrix perspective)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = perspective;
                }
                mesh.Draw();
            }
        }

        public void UpdateSync(int step)
        {
            graphicsDevice.SetRenderTarget(renderSync);
            graphicsDevice.Clear(syncColor[step]);
            graphicsDevice.SetRenderTarget(null);
        }

        public void InitializeSyncObj(int nSteps, int width, int height, WorldObject root)
        {
            this.syncColor = new Color[nSteps];
            for (int i = 0; i < nSteps; i++)
            {
                if (i == 1)
                    syncColor[i] = new Color(new Vector3((float)(1.27) / (nSteps-1)));
                else
                    syncColor[i] = new Color(new Vector3((float)(i) / (nSteps - 1)));
            }
            this.cam = root.GetWorldObject("Fly");
            this.camServ = (CameraService)cam.GetService(typeof(CameraService));
            this.syncObj = root.GetWorldObject("Sync");
            this.syncMod = (Model)syncObj.GetService(typeof(Model));
            this.synchPos = (PositionService)syncObj.GetService(typeof(PositionService));
            this.renderSync = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            effect.Parameters["World"].SetValue(synchPos.PosMatrix());
            effect.Parameters["View"].SetValue(camServ.CamView());
            effect.Parameters["Projection"].SetValue(camServ.CamPerspective());
            effect.Parameters["ModelTexture"].SetValue((Texture2D)renderSync);
        }
    }
}
