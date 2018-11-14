using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Stimulus.Services;

namespace VRLibrary.Stimulus.ServiceFactories
{
    /* Factory for a PositionService */
    public class PositionFactory : ServiceFactory
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 reference;

        public override void Initialize(IServiceProvider provider, Game game)
        {
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            var pos = new PositionService(game);
            pos.position = position;
            pos.rotation = rotation;
            pos.reference = reference;
            wo.AddService(typeof(PositionService), pos);
        }
    }
}
