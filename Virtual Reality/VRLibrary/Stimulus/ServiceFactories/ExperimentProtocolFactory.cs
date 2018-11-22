using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using VRLibrary.Stimulus.Services;

namespace VRLibrary.Stimulus.ServiceFactories
{
    public class ExperimentProtocolFactory : ServiceFactory
    {
        public float duration;
        public float durationTrials;
        public List<string> stimTypes;
        public override void Initialize(IServiceProvider provider, Game game)
        {
            var wo = (IServiceContainer)provider.GetService(typeof(IServiceContainer));
            var ep = new ExperimentProtocol(provider, duration, durationTrials, stimTypes);
            wo.AddService(typeof(ExperimentProtocol), ep);
        }
    }
}
