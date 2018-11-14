using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class UpdateWithFlyOpenClosedDelay : UpdateService
    {
        KalmanFilterTrack flyPos;
        PositionService posServ;
        Queue<float> queue = new Queue<float>();
        int[] delay = new int[3];
        int aux = 0;
        int aux2 = 0;
        long pframe = 0;
        long[] v = new long[7];
        float ang = 0;

        public UpdateWithFlyOpenClosedDelay(IServiceContainer wObj, Game game)
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
            v[0] = 9000;
            v[1] = 27000;
            v[2] = 36000;
            v[3] = 54000;
            v[4] = 63000;
            v[5] = 81000;
            v[6] = 90000;
            delay[0] = 29;
            delay[1] = 4;
            delay[2] = 60;
        }

        public override void Update(GameTime gametime)
        {
            if (pframe != pType.currentFrame)
            {
                queue.Enqueue(flyPos.pars[2]);
                if (queue.Count == delay[aux2])
                {
                    ang = queue.Dequeue();
                }
            }
            pframe = pType.currentFrame;


            if (aux == 0)
            {
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 0.0f * ang;
                //posServ.rotation.X = 0.0f;
            }
            if (aux == 1)
            {
                aux2 = 0;
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 2.0f * ang;
                //posServ.rotation.X = 3.141592f;
            }
            if (aux == 2)
            {
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 0.0f * ang;
                //posServ.rotation.X = 0.0f;
            }
            if (aux == 3)
            {
                aux2 = 1;
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 2.0f * ang;
                //posServ.rotation.X = 3.141592f;
            }
            if (aux == 4)
            {
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 0.0f * ang;
                //posServ.rotation.X = 0.0f;
            }
            if (aux == 5)
            {
                aux2 = 2;
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 2.0f * ang;
                //posServ.rotation.X = 3.141592f;
            }
            if (aux == 6)
            {
                posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
                posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;
                posServ.rotation.Z = 0.0f * ang;
                //posServ.rotation.X = 0.0f;
            }
            if (pType.currentFrame >= v[aux])
            {
                if (aux >= 6) { }
                else
                {
                    aux++;
                    queue.Clear();
                }
            }

        }
    }
}
