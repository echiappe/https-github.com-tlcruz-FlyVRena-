using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using VRLibrary.Stimulus.Services.UpdateServices;

namespace VRLibrary.Stimulus.ServiceFactories.UpdateFactories
{
    //Factory for an object to be updated with NGRG protocol
    public class UpdateWithFlyNGRGFactory : ServiceFactory
    {
        public override void Initialize(IServiceProvider provider, Game game)
        {
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            var update = new UpdateWithFlyNGRG(wo, game);
            wo.AddService(typeof(UpdateWithFlyNGRG), update);
        }
    }
}
