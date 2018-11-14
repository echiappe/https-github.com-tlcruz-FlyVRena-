using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services
{
    /* Service that gives the World Object necessary components for rendering */
    public class DrawService
    {
        public PositionService pos;
        public Model model;
        public CameraService cam;

        public DrawService(IServiceProvider WObj, Game game)
        {
            if ((RenderSubsystem)game.Services.GetService(typeof(RenderSubsystem)) != null)
            {
                RenderSubsystem rs = (RenderSubsystem)game.Services.GetService(typeof(RenderSubsystem));
                NameService objectName = (NameService)WObj.GetService(typeof(NameService));
                // Check if the World Object have a position in space, if not do not add it to render
                if (WObj.GetService(typeof(PositionService)) != null)
                {
                    pos = (PositionService)WObj.GetService(typeof(PositionService));
                }
                // Check if the World Object is a Camera, if yes add it to render as a Camera
                if (WObj.GetService(typeof(CameraService)) != null)
                {
                    cam = (CameraService)WObj.GetService(typeof(CameraService));
                    rs.AddDrawService("Camera", this);
                }
                // Check if the World Object has a Model, if yes add it to render
                if (WObj.GetService(typeof(Model)) != null)
                {
                    model = (Model)WObj.GetService(typeof(Model));
                    rs.AddDrawService(objectName.ObjectName(), this);
                }
            }
        }
    }
}
