using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRLibrary.Stimulus.Services
{
    public class ExperimentProtocol
    {
        public float duration;
        public float durationTrial;
        public List<string> stimTypes;
        public ExperimentProtocol(IServiceProvider WObj, float duration)
        {
            this.duration = duration;
        }
        public ExperimentProtocol(IServiceProvider WObj, float duration, float dTrial, List<string> sTypes)
        {
            this.duration = duration;
            this.durationTrial = dTrial;
            this.stimTypes = sTypes;
        }
    }
}
