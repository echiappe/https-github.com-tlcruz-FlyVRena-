using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Stimulus.Services;

namespace VRLibrary.Stimulus.ServiceFactories
{
    /* Factory for a NameService */
    public class NameFactory : ServiceFactory
    {
        public string objectName;
        public override void Initialize(IServiceProvider provider, Game game)
        {
            var name = new NameService(objectName, game);
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            wo.AddService(typeof(NameService), name);
        }
    }
}
