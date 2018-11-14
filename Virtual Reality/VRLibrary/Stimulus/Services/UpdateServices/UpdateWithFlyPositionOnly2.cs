using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class UpdateWithFlyPositionOnly2 : UpdateService
    {
        KalmanFilterTrack flyPos;
        PositionService posServ;
        NameService name;
        float[] c = new float[12];
        public UpdateWithFlyPositionOnly2(IServiceContainer wObj, Game game)
            : base(wObj, game)
        {
            flyPos = (KalmanFilterTrack)game.Services.GetService(typeof(KalmanFilterTrack));
            posServ = (PositionService)wObj.GetService(typeof(PositionService));
            pType = (VRProtocol)game.Services.GetService(typeof(VRProtocol));
            name = (NameService)wObj.GetService(typeof(NameService));
            if ((UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem)) != null)
            {
                UpdateSubsystem us = (UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem));
                us.AddUpdateService(name.ObjectName() + "UpdateWithFly", this);
            }
            pType.pType = VRProtocolType.ClosedLoop;
            c[0] = 0.0012f;
            c[1] = -0.8937f;
            c[2] = 0.0000f;
            c[3] = 0.0005f;
            c[4] = 1;
            c[5] = 0.0013f;
            c[6] = -0.0000f;
            c[7] = -0.3741f;
            c[8] = 11.6243f;
            c[9] = 5.6421f;
            c[10] = -12.9017f;
            c[11] = -0.9338f;
        }

        public override void Update(GameTime gametime)
        {
            posServ.position.Y = c[11] + c[10] * (c[5] * flyPos.pars[0] + c[6] * flyPos.pars[1] + c[7]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
            posServ.position.X = c[9] + c[8] * (c[0] * flyPos.pars[1] + c[1]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
            posServ.rotation.Z = 1f * (float)Math.PI * (flyPos.pars[2]) / 180f;
            posServ.position.Z = 18;
        }
    }
}
