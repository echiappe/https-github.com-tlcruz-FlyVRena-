using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class UpdateWithFlyClosedAndOpen : UpdateService
    {
        KalmanFilterTrack flyPos;
        PositionService posServ;
        NameService name;
        long[] v = new long[4];
        int aux = 0;
        Game g;
        public UpdateWithFlyClosedAndOpen(IServiceContainer wObj, Game game)
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
            //v[0] = 18000;
            //v[1] = 36000;
            //v[2] = 54000;
            //v[3] = 72000;
            //v[4] = 90000;
            //v[5] = 108000;
            //v[6] = 126000;
            //v[7] = 144000;
            //v[8] = 166800;
            //v[9] = 202800;
            //v[10] = 204000;
            //v[11] = 240000;
            //v[12] = 241200;
            //v[13] = 277200; 
            //v[14] = 278400;
            //v[15] = 314400;
            //v[16] = 315600;
            //v[17] = 351600;
            //v[18] = 352800;
            //v[19] = 388800;
            //v[20] = 390000;
            //v[21] = 426000;
            //v[22] = 427200;
            //v[23] = 463200;
            //v[24] = 500000;

            v[0] = 0;
            //v[1] = 54000;
            v[1] = 72000;
            //v[3] = 90000;

            //v[1] = 36000;
            //v[2] = 36600;
            //v[3] = 72600;
            //v[4] = 73200;
            //v[5] = 109200;
            //v[6] = 109800;
            //v[7] = 127800;
            //v[8] = 145800;

            //v[0] = 9000;
            //v[1] = 27000;
            //v[2] = 36000;
            //v[3] = 54000;
            //v[4] = 63000;
            //v[5] = 81000;
            //v[6] = 90000;
            this.g = game;
        }

        public override void Update(GameTime gametime)
        {
            //if (Math.Floor((double)(pType.currentFrame) / 18000) % 2 == 0)
            if (aux == 0)
            {
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 0.0f * (float)Math.PI * flyPos.pars[2] / 180f;
                posServ.rotation.X = 3.141592f;
            }
            if (aux == 1)
            {
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 0.0f * (float)Math.PI * flyPos.pars[2] / 180f;
                posServ.rotation.X = 0*3.141592f;
            }
            //if (aux == 2)
            //{
            //    posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
            //    posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
            //    posServ.rotation.Z = 0.0f * (float)Math.PI * flyPos.pars[2] / 180f;
            //    //posServ.rotation.X = 0.0f;
            //}
            //if (aux == 3)
            //{
            //    posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
            //    posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
            //    posServ.rotation.Z = 2.0f * (float)Math.PI * flyPos.pars[2] / 180f;
            //    //posServ.rotation.X = 3.141592f;
            //}
            //if (aux == 4)
            //{
            //    posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
            //    posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
            //    posServ.rotation.Z = 0.0f * flyPos.pars[2];
            //    //posServ.rotation.X = 0.0f;
            //}
            //if (aux == 5)
            //{
            //    posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
            //    posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
            //    posServ.rotation.Z = 2.0f * flyPos.pars[2];
            //    //posServ.rotation.X = 3.141592f;
            //}
            //if (aux == 6)
            //{
            //    posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
            //    posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
            //    posServ.rotation.Z = 0.0f * flyPos.pars[2];
            //    //posServ.rotation.X = 0.0f;
            //}
            if (pType.currentFrame >= v[aux])
            {
                if (aux >= 3) { }
                else
                {
                    aux++;
                    //if (name.name == "RandDotsA15D0.16" && (aux > 1 && aux < 4))
                    //{
                    //    posServ.position.Z = 15;
                    //}
                    //else if (name.name == "RandDotsA15D0.16" && (aux <= 1 || aux >= 4))
                    //    posServ.position.Z = 10;

                    //if (name.name == "RandDotsA5D0.2" && (aux < 2))
                    //{
                    //    posServ.position.Z = 15;
                    //}
                    //else if (name.name == "RandDotsA5D0.2" && (aux >= 2))
                    //    posServ.position.Z = 10;

                    //if (name.name == "RandDotsA5D0.25" && (aux > 3))
                    //{
                    //    posServ.position.Z = 15;
                    //}
                    //else if (name.name == "RandDotsA5D0.25" && (aux <= 3))
                    //    posServ.position.Z = 10;

                }
            }
        }
    }
}
