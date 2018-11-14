using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class SmallObjectPositionUpdate : UpdateService
    {
        KalmanFilterTrack flyPos;
        PositionService posServ;
        long[] v = new long[4];
        int aux = 0;
        long pframe = 0;
        bool TEST_STIMULUS = false;

        public SmallObjectPositionUpdate(IServiceContainer wObj, Game game)
            : base(wObj, game)
        {
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

            v[0] = 0;       //18000 < 5 Minutes
            v[1] = 72000;       // < 20 Minutes
            //v[2] = 72000;
        }

        public override void Update(GameTime gametime)
        {
            if (pframe != pType.currentFrame)
            {
                if (aux == 0 && !(TEST_STIMULUS))
                {
                    posServ.position.X = 0.008889f * flyPos.pars[1] - 3.35f;//3.213f;
                    posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.35f;//3.298f;
                    posServ.rotation.X = 3.141592f;
                }
                if (aux == 1 && !(TEST_STIMULUS))
                {
                    posServ.position.X = 0.008889f * flyPos.pars[1] - 3.35f;//3.213f;
                    posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.35f + 1.0f;//3.298f;
                    posServ.rotation.X = 0.0f;
                }

                if (pType.currentFrame >= v[aux])
                {
                    if (aux == 2)
                    {
                        Console.WriteLine("STOP recording");
                    }
                    else
                    {
                        aux++;
                    }
                }
                if (TEST_STIMULUS)
                {
                    posServ.position.X = 0.0f;//3.213f;
                    posServ.position.Y = 0.0f;//3.298f;
                    posServ.rotation.X = 0.0f;
                    posServ.rotation.Z = 0.0f;
                }
            }
            pframe = pType.currentFrame;
        }
    }
}