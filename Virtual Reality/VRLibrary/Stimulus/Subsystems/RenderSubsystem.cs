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

            //render every object in the list of registered services
            foreach (string n in DrawServices.Keys)
            {
                if (n != "Camera")
                {
                    DrawModel(DrawServices[n].model, DrawServices[n].pos.PosMatrix(), DrawServices["Camera"].cam.CamView(), DrawServices["Camera"].cam.CamPerspective());
                }
            }
            base.Draw(gameTime);
        }

        public new void Dispose()
        {
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
     }
}
