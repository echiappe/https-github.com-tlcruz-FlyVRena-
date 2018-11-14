using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Stimulus.Services;

namespace VRLibrary.Stimulus.ServiceFactories
{
    /* Factory for a CameraService */
    public class CameraFactory : ServiceFactory
    {
        /* Projection properties */
        public bool IsPerspective;
        public float fieldOfView;
        public float aspectRatio;
        public float farPlane;
        public float nearPlane;
        public float left;
        public float right;
        public float bottom;
        public float top;
        public override void Initialize(IServiceProvider provider, Game game)
        {
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            if (IsPerspective)
            {
                var camera = new CameraService(provider, game, fieldOfView, aspectRatio, nearPlane, farPlane);
                wo.AddService(typeof(CameraService), camera);
            }
            else
            {
                var camera = new CameraService(provider, game, left, right, bottom, top, nearPlane, farPlane);
                wo.AddService(typeof(CameraService), camera);
            }

        }
    }
}
