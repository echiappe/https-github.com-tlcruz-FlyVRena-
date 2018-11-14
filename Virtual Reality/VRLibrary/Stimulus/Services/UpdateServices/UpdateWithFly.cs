using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class UpdateWithFly : UpdateService
    {
        KalmanFilterTrack flyPos;
        PositionService posServ;
        NameService name;
        //long[] v = new long[4];
        int aux = 0;
        float[] c = new float[12];
        Game g;

        public UpdateWithFly(IServiceContainer wObj, Game game)
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
            //v[1] = 54000;
            //v[2] = 72000;
            //c[0] = 0.0011f;
            //c[1] = -0.7660f;
            //c[2] = 0.0000f;
            //c[3] = 0.0004f;
            //c[4] = 1;
            //c[5] = 0.0012f;
            //c[6] = -0.0000f;
            //c[7] = -0.3676f;
            //c[8] = 12.5535f;
            //c[9] = 4.6354f + 0.2f;
            //c[10] = -12.9056f;
            //c[11] = -1.0711f;

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
            this.g = game;
        }

        public override void Update(GameTime gametime)
        {
            //if (aux == 0)
            //{
            posServ.position.Y = c[11] + c[10] * (c[5] * flyPos.pars[0] + c[6] * flyPos.pars[1] + c[7]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
            posServ.position.X = c[9] + c[8] * (c[0] * flyPos.pars[1] + c[1]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
            posServ.rotation.Z = 0.0f * (float)Math.PI * flyPos.pars[2] / 180f;
                //posServ.rotation.X = 3.141592f;
            //}
            //if (aux == 1)
            //{
                //posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                //posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                //posServ.rotation.Z = 2.0f * (float)Math.PI * flyPos.pars[2] / 180f;
                //posServ.rotation.X = 0 * 3.141592f;
            //}
            //if (aux == 2)
            //{
            //    posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
            //    posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
            //    posServ.rotation.Z = 0.0f * (float)Math.PI * flyPos.pars[2] / 180f;
                //posServ.rotation.X = 3.141592f;
            //}

            //if (pType.currentFrame >= v[aux])
            //{
            //    if (aux >= 3) { }
            //    else
            //    {
            //        aux++;
            //    }
            //}

        }
    }
}
