using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Stimulus.Services;

namespace VRLibrary.Stimulus.Subsystems
{
    /* Subsystem for running the update routine of each component of the world */
    public class UpdateSubsystem : Microsoft.Xna.Framework.GameComponent
    {
        //int i = 0;
        public bool enable = true;
        public UpdateSubsystem(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(UpdateSubsystem), this);
            this.Enabled = false;
        }

        // List of objects registered to draw and a camera (only one camera should be attributed at a time)
        public Dictionary<string, UpdateService> UpdateServices = new Dictionary<string, UpdateService>();

        // Add a WO to the draw list
        public void AddUpdateService(string name, UpdateService obj)
        {
            this.UpdateServices.Add(name, obj);
        }

        // Count number of WO are registered (except for the camera)
        public int NumberOfUpdateObjects()
        {
            int i = 0;
            foreach (string name in UpdateServices.Keys)
            {
                i++;
            }
            return i;
        }

        public override void Initialize() { }

        public override void Update(GameTime gameTime)
        {
            // Run the update of each component
            foreach (UpdateService upd in UpdateServices.Values)
            {
                upd.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public void UpdateAsync(GameTime gameTime)
        {
            foreach (UpdateService upd in UpdateServices.Values)
            {
                upd.Update(gameTime);
            }
        }
    }
}
