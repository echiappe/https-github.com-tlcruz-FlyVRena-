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
        //vars to store vr services
        KalmanFilterTrack flyPos;
        PositionService posServ;
        NameService name;

        //calibration and auxiliary variables
        float[] c = new float[12];
        string[] v;
        int[] frames;

        public UpdateWithFlyPositionOnly(IServiceContainer wObj, Game game)
            : base(wObj, game)
        {
            //load all necessary services
            flyPos = (KalmanFilterTrack)game.Services.GetService(typeof(KalmanFilterTrack));
            posServ = (PositionService)wObj.GetService(typeof(PositionService));
            pType = (VRProtocol)game.Services.GetService(typeof(VRProtocol));
            name = (NameService)wObj.GetService(typeof(NameService));
            if ((UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem)) != null)
            {
                //add this update service to the update subsystem list
                UpdateSubsystem us = (UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem));
                us.AddUpdateService(name.ObjectName() + "UpdateWithFly", this);
            }
            pType.pType = VRProtocolType.ClosedLoop;

            //calibration values
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

            //protocol trial structure load
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
        int bs = 0;
        public override void Update(GameTime gametime)
        {
            //wait for a new frame
            if (pframe != pType.currentFrame)
            {
                //if the trial type is correct
                if (name.name == "Mask")
                {
                    //update position and orientation of object based on fly position
                    posServ.position.Y = c[11] + c[10] * (c[5] * flyPos.pars[0] + c[6] * flyPos.pars[1] + c[7]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.position.X = c[9] + c[8] * (c[0] * flyPos.pars[1] + c[1]) / (c[2] * flyPos.pars[0] + c[3] * flyPos.pars[1] + c[4]);
                    posServ.rotation.Z = 1f * (float)Math.PI * (flyPos.pars[2]) / 180f;

                    //bring the object to the top
                    posServ.position.Z = 18f;
                }

                //otherwise send object to the back
                else
                {
                    posServ.position.Z = 15;
                }
            }

            //if current frame reaches the end of the trial change to next trial
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
