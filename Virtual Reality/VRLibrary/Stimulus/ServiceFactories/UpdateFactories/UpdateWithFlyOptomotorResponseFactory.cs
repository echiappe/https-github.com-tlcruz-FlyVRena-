using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using VRLibrary.Stimulus.Services.UpdateServices;

namespace VRLibrary.Stimulus.ServiceFactories.UpdateFactories
{
    //Factory for an object to be updated with an optomotor response protocol
    public class UpdateWithFlyOptomotorResponseFactory : ServiceFactory
    {
        public override void Initialize(IServiceProvider provider, Game game)
        {
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            var update = new UpdateWithFlyOptomotorResponse(wo, game);
            wo.AddService(typeof(UpdateWithFlyOptomotorResponse), update);
        }
    }
}
