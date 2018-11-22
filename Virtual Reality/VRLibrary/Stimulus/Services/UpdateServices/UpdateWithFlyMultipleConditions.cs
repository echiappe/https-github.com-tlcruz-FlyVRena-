using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class UpdateWithFlyMultipleConditions : UpdateService
    {
        KalmanFilterTrack flyPos;
        PositionService posServ;
        float[] posServ0=new float[3];
        NameService name;
        float[] c = new float[12];
        string[] v;
        int bs = 0;
        int[] frames;
        public UpdateWithFlyMultipleConditions(IServiceContainer wObj, Game game)
            : base(wObj, game)
        {
            flyPos = (KalmanFilterTrack)game.Services.GetService(typeof(KalmanFilterTrack));
            posServ = (PositionService)wObj.GetService(typeof(PositionService));
            posServ0[0] = posServ.rotation.X;
            posServ0[1] = posServ.rotation.Y;
            posServ0[2] = posServ.rotation.Z;
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

            v = new string[pType.trials.Count];
            frames = new int[pType.trials.Count];

            for (int i = 0; i < pType.trials.Count; i++)
            {
                frames[i] = (int)(1 + i) * Convert.ToInt32(pType.tDuration);
                v[i] = pType.trials.ElementAt(i);
            }
        }

        int aux = 0;
        long pframe = 0;
        float prevOri = 0;
        public override void Update(GameTime gametime)
        {
            if (pframe != pType.currentFrame)
            {
                if (name.name == v[aux])
                {
                    posServ.position.Y = c[11] + c[10] * (c[5] * flyPos.pars[0] + c[6] * flyPos.pars[1] + c[7]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.position.X = c[9] + c[8] * (c[0] * flyPos.pars[1] + c[1]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.position.Z = 16f;
                    if (pType.currentFrame - bs >= 5400)//3600)//3600)//13500)//5400)//18000)//9000)//
                    {
                        posServ.rotation.Z += 2.0f * ((float)Math.PI * flyPos.pars[2] / 180f - prevOri);
                        prevOri = (float)Math.PI * flyPos.pars[2] / 180f;
                    }
                    else
                    {
                        posServ.rotation.Z += -0.0f * ((float)Math.PI * flyPos.pars[2] / 180f - prevOri);
                        prevOri = (float)Math.PI * flyPos.pars[2] / 180f;
                    }
                }
                else if (name.name == "b")
                {
                    posServ.position.Y = c[11] + c[10] * (c[5] * flyPos.pars[0] + c[6] * flyPos.pars[1] + c[7]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.position.X = c[9] + c[8] * (c[0] * flyPos.pars[1] + c[1]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.position.Z = 16f;
                    posServ.rotation.X = -(posServ0[0] + posServ0[1]) * (float)Math.Sin(Math.PI * flyPos.pars[2] / 180f);
                    posServ.rotation.Y = (posServ0[0] + posServ0[1]) * (float)Math.Cos(Math.PI * flyPos.pars[2] / 180f);
                    prevOri = (float)Math.PI * flyPos.pars[2] / 180f;
                }
                else
                {
                    posServ.position.Z = 15f;
                }

            }
            pframe = pType.currentFrame;
            if (pType.currentFrame >= frames[aux])
            {
                bs = frames[aux];
                if (aux >= 120) { }
                else
                    aux++;
            }
        }
    }
}
