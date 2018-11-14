using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRLibrary.Stimulus.ServiceFactories
{
    /* Objects that will initialize the services for the world objects */
    public abstract class ServiceFactory
    {
        public abstract void Initialize(IServiceProvider provider, Game game);
    }
}
