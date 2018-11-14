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
    public class UpdateWithFlyPositionOnly : UpdateService
    {
        KalmanFilterTrack flyPos;
        PositionService posServ;
        NameService name;
        float[] c = new float[12];
        int[] frames = new int[10];
        float[] prevPos = new float[3];
        string[] vc = new string[10];
        int bs = 0;
        public UpdateWithFlyPositionOnly(IServiceContainer wObj, Game game)
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


            frames[0] = 1 * 10800;
            frames[1] = 2 * 10800;
            frames[2] = 3 * 10800;
            frames[3] = 4 * 10800;
            frames[4] = 5 * 10800;
            frames[5] = 6 * 10800;
            frames[6] = 7 * 10800;
            frames[7] = 8 * 10800;
            //vc[0] = "4s";
            //vc[1] = "1s";
            //vc[2] = "4s";
            //vc[3] = "1s";
            //vc[4] = "3s";
            //vc[5] = "3s";
            vc[0] = "1h";
            vc[1] = "4h";
            vc[2] = "1h";
            vc[3] = "3h";
            vc[4] = "4h";
            vc[5] = "3h";
        }

        int aux = 0;
        long pframe = 0;
        float prevOri = 0;
        public override void Update(GameTime gametime)
        {
            if (pframe != pType.currentFrame)
            {
                if (name.name == "Mask10DM" && pType.currentFrame < 3 * 10800)//vc[aux])
                {
                    posServ.position.Y = c[11] + c[10] * (c[5] * flyPos.pars[0] + c[6] * flyPos.pars[1] + c[7]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.position.X = c[9] + c[8] * (c[0] * flyPos.pars[1] + c[1]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.rotation.Z = 1f * (float)Math.PI * (flyPos.pars[2]) / 180f;
                    posServ.position.Z = 18f;
                }
                else if (name.name == "Mask10DP" && pType.currentFrame > 3 * 10800)//vc[aux])
                {
                    posServ.position.Y = c[11] + c[10] * (c[5] * flyPos.pars[0] + c[6] * flyPos.pars[1] + c[7]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.position.X = c[9] + c[8] * (c[0] * flyPos.pars[1] + c[1]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.rotation.Z = 1f * (float)Math.PI * (flyPos.pars[2]) / 180f;
                    posServ.position.Z = 18f;
                }
                else
                {
                    posServ.position.Z = 15;
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
