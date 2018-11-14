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
        string[] v = new string[20];
        //string[] v = new string[3];
        int bs = 0;
        int[] frames = new int[20];
        //int[] frames = new int[3];
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


            //v[0] = "WindmillFull";
            //v[1] = "WindmillFull";
            //v[2] = "WindmillFull";


            //v[0] = "1";
            //v[1] = "4";

            /* Bilateral Head */          //CHANGE HALF TIME!!!!!!!

            //v[0] = "3";
            //v[1] = "1";
            //v[2] = "4";

            //frames[0] = 1 * 18000;
            //frames[1] = 2 * 18000;
            //frames[2] = 3 * 18000;


            /* Unilateral Size */        //CHANGE HALF TIME!!!!!!!
            //v[0] = "4";
            //v[1] = "1";
            //v[2] = "3";
            //v[3] = "3";
            //v[4] = "1";
            //v[5] = "4";


            //frames[0] = 1 * 10800;
            //frames[1] = 2 * 10800;
            //frames[2] = 3 * 10800;
            //frames[3] = 4 * 10800;
            //frames[4] = 5 * 10800;
            //frames[5] = 6 * 10800;





            v[0] = "4";
            v[1] = "4";
            v[2] = "4";
            v[3] = "4";
            v[4] = "3";
            v[5] = "4";
            v[6] = "1";
            v[7] = "2";

            frames[0] = 1 * 10800;
            frames[1] = 2 * 10800;
            frames[2] = 3 * 10800;
            frames[3] = 4 * 10800;
            frames[4] = 5 * 10800;
            frames[5] = 6 * 10800;
            frames[6] = 7 * 10800;
            frames[7] = 8 * 10800;

            /* Unilateral Density */          //CHANGE HALF TIME!!!!!!!

            //v[0] = "9";
            //v[1] = "10";
            //v[2] = "7";
            //v[3] = "9";
            //v[4] = "7";
            //v[5] = "8";
            //v[6] = "8";
            //v[7] = "10";

            //frames[0] = 1 * 10800;
            //frames[1] = 2 * 10800;
            //frames[2] = 3 * 10800;
            //frames[3] = 4 * 10800;
            //frames[4] = 5 * 10800;
            //frames[5] = 6 * 10800;
            //frames[6] = 7 * 10800;
            //frames[7] = 8 * 10800;

            //v[0] = "4";
            //v[1] = "1";
            //v[2] = "1";
            //v[3] = "3";
            //v[4] = "2";
            //v[5] = "4";
            //v[6] = "3";
            //v[7] = "2";

            //v[0] = "5";
            //v[1] = "4";
            //v[2] = "3";
            //v[3] = "7";
            //v[4] = "8";
            //v[5] = "2";
            //v[6] = "6";
            //v[7] = "2";
            //v[8] = "4";
            //v[9] = "1";
            //v[10] = "7";
            //v[11] = "5";
            //v[12] = "8";
            //v[13] = "1";
            //v[14] = "3";
            //v[15] = "6";


            //frames[0] = 1 * 27000;
            //frames[1] = 2 * 27000;
            //frames[2] = 3 * 27000;

            //frames[0] = 1 * 18000;
            //frames[1] = 2 * 18000;

            //frames[0] = 1 * 7200;
            //frames[1] = 2 * 7200;
            //frames[2] = 3 * 7200;
            //frames[3] = 4 * 7200;
            //frames[4] = 5 * 7200;
            //frames[5] = 6 * 7200;
            //frames[6] = 7 * 7200;
            //frames[7] = 8 * 7200;
            //frames[8] = 9 * 7200;
            //frames[9] = 10 * 7200;
            //frames[10] = 11 * 7200;
            //frames[11] = 12 * 7200;
            //frames[12] = 13 * 7200;
            //frames[13] = 14 * 7200;
            //frames[14] = 15 * 7200;
            //frames[15] = 15 * 7200;
            //frames[16] = 16 * 7200;
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
                    if (pType.currentFrame -bs >= 5400)//3600)//3600)//13500)//5400)//18000)//9000)//
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
