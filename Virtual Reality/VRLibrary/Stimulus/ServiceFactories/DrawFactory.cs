using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Stimulus.Services;

namespace VRLibrary.Stimulus.ServiceFactories
{
    /* Factory for a DrawService */
    public class DrawFactory : ServiceFactory
    {
        public override void Initialize(IServiceProvider provider, Game game)
        {
            var toDraw = new DrawService(provider, game);
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            wo.AddService(typeof(DrawService), toDraw);
        }
    }
}
