using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class FixedOnBasedFlyPos : UpdateService
    {
        KalmanFilterTrack flyPos;
        PositionService posServ;
        long[] v = new long[2];
        int aux = 0;
        Game g;
        float x;
        float y;
        public FixedOnBasedFlyPos(IServiceContainer wObj, Game game, float x, float y)
            : base(wObj, game)
        {
            this.x = x;
            this.y = y;
            flyPos = (KalmanFilterTrack)game.Services.GetService(typeof(KalmanFilterTrack));
            posServ = (PositionService)wObj.GetService(typeof(PositionService));
            pType = (VRProtocol)game.Services.GetService(typeof(VRProtocol));
            NameService name = (NameService)wObj.GetService(typeof(NameService));
            if ((UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem)) != null)
            {
                UpdateSubsystem us = (UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem));
                us.AddUpdateService(name.ObjectName() + "UpdateWithFly", this);
            }
            pType.pType = VRProtocolType.ClosedLoop;
            v[0] = 54000;
            v[1] = 90000;
            this.g = game;
        }

        public override void Update(GameTime gametime)
        {
            float dist = Vector2.Distance(new Vector2(flyPos.pars[0], flyPos.pars[1]), new Vector2(x, y));
            if (aux == 0)
            {
                    posServ.position.Z = 13f;
            }
            if (aux == 1)
            {
                if (dist > 150)
                {
                    posServ.position.Z = 15f;
                }
                else
                {
                    posServ.position.Z = 13f;
                }
            }
            if (pType.currentFrame >= v[aux])
            {
                if (aux >= 1) { }
                else
                    aux++;
            }
        }
    }
}
