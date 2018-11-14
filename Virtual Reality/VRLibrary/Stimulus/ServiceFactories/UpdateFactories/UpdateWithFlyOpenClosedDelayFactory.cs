using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.Stimulus.Services.UpdateServices;

namespace VRLibrary.Stimulus.ServiceFactories.UpdateFactories
{
    public class UpdateWithFlyOpenClosedDelayFactory : ServiceFactory
    {
        public override void Initialize(IServiceProvider provider, Game game)
        {
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            var update = new UpdateWithFlyOpenClosedDelay(wo, game);
            wo.AddService(typeof(UpdateWithFlyOpenClosedDelay), update);
        }
    }
}
