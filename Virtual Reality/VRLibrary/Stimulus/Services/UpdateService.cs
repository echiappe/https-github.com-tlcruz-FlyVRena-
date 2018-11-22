using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRLibrary.Stimulus.Services
{
    /* Service that gives the object an update routine so it can change in runtime */
    public abstract class UpdateService
    {
        public IServiceProvider WO;
        public VRProtocol pType;
        public UpdateService(IServiceProvider WObj, Game game)
        {
            WO = WObj;
        }

        /* Update routine */
        public virtual void Update(GameTime gametime) { }

        /* Routine to Exit safely from the Update (for example finish filestreams) */
        public virtual void IsExiting() { }
    }

    public enum VRProtocolType
    {
        Static,
        ClosedLoop,
        ClosedLoopBias,
        OpenLoop,
    }

    public class VRProtocol
    {
        public VRProtocolType pType;
        public long currentFrame;
        public long lostFrames;
        public long currentFrame2;
        public long lostFrames2;
        public float tDuration;
        public List<string> trials;
    }

}
