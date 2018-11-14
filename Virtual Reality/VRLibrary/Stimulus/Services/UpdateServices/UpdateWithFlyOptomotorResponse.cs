using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class UpdateWithFlyOptomotorResponse : UpdateService
    {

        KalmanFilterTrack flyPos;
        PositionService posServ;
        int[] frames;
        float[] protocol;
        int aux = 0;
        long pframe = 0;
        public UpdateWithFlyOptomotorResponse(IServiceContainer wObj, Game game)
            : base(wObj, game)
        {
            flyPos = (KalmanFilterTrack)game.Services.GetService(typeof(KalmanFilterTrack));
            posServ = (PositionService)wObj.GetService(typeof(PositionService));
            pType = (VRProtocol)game.Services.GetService(typeof(VRProtocol));
            NameService name = (NameService)wObj.GetService(typeof(NameService));
            if ((UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem)) != null)
            {
                UpdateSubsystem us = (UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem));
                us.AddUpdateService(name.ObjectName() + "UpdateWithFlyOptomotorResponse", this);
            }
            pType.pType = VRProtocolType.ClosedLoop;
            frames = GetFrames();
            protocol = GetProtocol();
        }


        public override void Update(GameTime gametime)
        {
            posServ.position.X = 0.008889f * flyPos.pars[1] - 3.213f;
            posServ.position.Y = -0.008897f * flyPos.pars[0] + 3.298f;


            if (pframe != pType.currentFrame)
            {
                posServ.rotation.Z = posServ.rotation.Z + protocol[aux];
            }

            pframe = pType.currentFrame;

            if (pType.currentFrame >= frames[aux])
            {
                if (aux >= 120) { }
                else
                    aux++;
            }

        }




        public int[] GetFrames()
        {
            int[] frameChange = new int[121];
            frameChange[0] = 18000;
            frameChange[1] = 18120;
            frameChange[2] = 19800;
            frameChange[3] = 19920;
            frameChange[4] = 21600;
            frameChange[5] = 21720;
            frameChange[6] = 23400;
            frameChange[7] = 23520;
            frameChange[8] = 25200;
            frameChange[9] = 25320;
            frameChange[10] = 27000;
            frameChange[11] = 27120;
            frameChange[12] = 28800;
            frameChange[13] = 28920;
            frameChange[14] = 30600;
            frameChange[15] = 30720;
            frameChange[16] = 32400;
            frameChange[17] = 32520;
            frameChange[18] = 34320;
            frameChange[19] = 34200;
            frameChange[20] = 36000;
            frameChange[21] = 36120;
            frameChange[22] = 37800;
            frameChange[23] = 37920;
            frameChange[24] = 39600;
            frameChange[25] = 39720;
            frameChange[26] = 41400;
            frameChange[27] = 41520;
            frameChange[28] = 43200;
            frameChange[29] = 43320;
            frameChange[30] = 45000;
            frameChange[31] = 45120;
            frameChange[32] = 46800;
            frameChange[33] = 46920;
            frameChange[34] = 48600;
            frameChange[35] = 48720;
            frameChange[36] = 50400;
            frameChange[37] = 50520;
            frameChange[38] = 52200;
            frameChange[39] = 52320;
            frameChange[40] = 54000;
            frameChange[41] = 54120;
            frameChange[42] = 55800;
            frameChange[43] = 55920;
            frameChange[44] = 57600;
            frameChange[45] = 57720;
            frameChange[46] = 59400;
            frameChange[47] = 59520;
            frameChange[48] = 61200;
            frameChange[49] = 61320;
            frameChange[50] = 63000;
            frameChange[51] = 63120;
            frameChange[52] = 64800;
            frameChange[53] = 64920;
            frameChange[54] = 66600;
            frameChange[55] = 66720;
            frameChange[56] = 68400;
            frameChange[57] = 68520;
            frameChange[58] = 70200;
            frameChange[59] = 70320;
            frameChange[60] = 72000;
            frameChange[61] = 72120;
            frameChange[62] = 73800;
            frameChange[63] = 73920;
            frameChange[64] = 75600;
            frameChange[65] = 75720;
            frameChange[66] = 77400;
            frameChange[67] = 77520;
            frameChange[68] = 79200;
            frameChange[69] = 79320;
            frameChange[70] = 81000;
            frameChange[71] = 81120;
            frameChange[72] = 82800;
            frameChange[73] = 82920;
            frameChange[74] = 84600;
            frameChange[75] = 84720;
            frameChange[76] = 86400;
            frameChange[77] = 86520;
            frameChange[78] = 88200;
            frameChange[79] = 88320;
            frameChange[80] = 90000;
            frameChange[81] = 90120;
            frameChange[82] = 91800;
            frameChange[83] = 91920;
            frameChange[84] = 93600;
            frameChange[85] = 93720;
            frameChange[86] = 95400;
            frameChange[87] = 95520;
            frameChange[88] = 97200;
            frameChange[89] = 97320;
            frameChange[90] = 99000;
            frameChange[91] = 99120;
            frameChange[92] = 100800;
            frameChange[93] = 100920;
            frameChange[94] = 102600;
            frameChange[95] = 102720;
            frameChange[96] = 104400;
            frameChange[97] = 104520;
            frameChange[98] = 106200;
            frameChange[99] = 106320;
            frameChange[100] = 108000;
            frameChange[101] = 108120;
            frameChange[102] = 109800;
            frameChange[103] = 109920;
            frameChange[104] = 111600;
            frameChange[105] = 111720;
            frameChange[106] = 113400;
            frameChange[107] = 113520;
            frameChange[108] = 115200;
            frameChange[109] = 115320;
            frameChange[110] = 117000;
            frameChange[111] = 117120;
            frameChange[112] = 118800;
            frameChange[113] = 118920;
            frameChange[114] = 120600;
            frameChange[115] = 120720;
            frameChange[116] = 122400;
            frameChange[117] = 122520;
            frameChange[118] = 124200;
            frameChange[119] = 124320;
            frameChange[120] = 126000 + 18000;

            return frameChange;
        }

        public float[] GetProtocol()
        {
            float[] protocolChange = new float[121];
            protocolChange[0] = 0f;
            protocolChange[1] = -1f * 0.0262f;
            protocolChange[2] = 0f;
            protocolChange[3] = 5.0f * 0.0262f;
            protocolChange[4] = 0f;
            protocolChange[5] = 5.0f * 0.0262f;
            protocolChange[6] = 0f;
            protocolChange[7] = -1.0f * 0.0262f;
            protocolChange[8] = 0f;
            protocolChange[9] = 5.0f * 0.0262f;
            protocolChange[10] = 0f;
            protocolChange[11] = -10.0f * 0.0262f;
            protocolChange[12] = 0f;
            protocolChange[13] = 10.0f * 0.0262f;
            protocolChange[14] = 0f;
            protocolChange[15] = 1.0f * 0.0262f;
            protocolChange[16] = 0f;
            protocolChange[17] = 1.0f * 0.0262f;
            protocolChange[18] = 0f;
            protocolChange[19] = -1.0f * 0.0262f;
            protocolChange[20] = 0f;
            protocolChange[21] = -5.0f * 0.0262f;
            protocolChange[22] = 0f;
            protocolChange[23] = -5.0f * 0.0262f;
            protocolChange[24] = 0f;
            protocolChange[25] = 1.0f * 0.0262f;
            protocolChange[26] = 0f;
            protocolChange[27] = 1.0f * 0.0262f;
            protocolChange[28] = 0f;
            protocolChange[29] = -1.0f * 0.0262f;
            protocolChange[30] = 0f;
            protocolChange[31] = 5.0f * 0.0262f;
            protocolChange[32] = 0f;
            protocolChange[33] = -10.0f * 0.0262f;
            protocolChange[34] = 0f;
            protocolChange[35] = -1.0f * 0.0262f;
            protocolChange[36] = 0f;
            protocolChange[37] = -1.0f * 0.0262f;
            protocolChange[38] = 0f;
            protocolChange[39] = -5.0f * 0.0262f;
            protocolChange[40] = 0f;
            protocolChange[41] = 10.0f * 0.0262f;
            protocolChange[42] = 0f;
            protocolChange[43] = -10.0f * 0.0262f;
            protocolChange[44] = 0f;
            protocolChange[45] = 1.0f * 0.0262f;
            protocolChange[46] = 0f;
            protocolChange[47] = 10.0f * 0.0262f;
            protocolChange[48] = 0f;
            protocolChange[49] = 1.0f * 0.0262f;
            protocolChange[50] = 0f;
            protocolChange[51] = -10.0f * 0.0262f;
            protocolChange[52] = 0f;
            protocolChange[53] = -1.0f * 0.0262f;
            protocolChange[54] = 0f;
            protocolChange[55] = -1.0f * 0.0262f;
            protocolChange[56] = 0f;
            protocolChange[57] = 10.0f * 0.0262f;
            protocolChange[58] = 0f;
            protocolChange[59] = -10.0f * 0.0262f;
            protocolChange[60] = 0f;
            protocolChange[61] = -10.0f * 0.0262f;
            protocolChange[62] = 0f;
            protocolChange[63] = 5.0f * 0.0262f;
            protocolChange[64] = 0f;
            protocolChange[65] = 10.0f * 0.0262f;
            protocolChange[66] = 0f;
            protocolChange[67] = 5.0f * 0.0262f;
            protocolChange[68] = 0f;
            protocolChange[69] = -5.0f * 0.0262f;
            protocolChange[70] = 0f;
            protocolChange[71] = 1.0f * 0.0262f;
            protocolChange[72] = 0f;
            protocolChange[73] = -10.0f * 0.0262f;
            protocolChange[74] = 0f;
            protocolChange[75] = 5.0f * 0.0262f;
            protocolChange[76] = 0f;
            protocolChange[77] = -5.0f * 0.0262f;
            protocolChange[78] = 0f;
            protocolChange[79] = 1.0f * 0.0262f;
            protocolChange[80] = 0f;
            protocolChange[81] = 5.0f * 0.0262f;
            protocolChange[82] = 0f;
            protocolChange[83] = -10.0f * 0.0262f;
            protocolChange[84] = 0f;
            protocolChange[85] = 10.0f * 0.0262f;
            protocolChange[86] = 0f;
            protocolChange[87] = -5.0f * 0.0262f;
            protocolChange[88] = 0f;
            protocolChange[89] = -1.0f * 0.0262f;
            protocolChange[90] = 0f;
            protocolChange[91] = -5.0f * 0.0262f;
            protocolChange[92] = 0f;
            protocolChange[93] = -10.0f * 0.0262f;
            protocolChange[94] = 0f;
            protocolChange[95] = -1.0f * 0.0262f;
            protocolChange[96] = 0f;
            protocolChange[97] = 1.0f * 0.0262f;
            protocolChange[98] = 0f;
            protocolChange[99] = 10.0f * 0.0262f;
            protocolChange[100] = 0f;
            protocolChange[101] = -5.0f * 0.0262f;
            protocolChange[102] = 0f;
            protocolChange[103] = 5.0f * 0.0262f;
            protocolChange[104] = 0f;
            protocolChange[105] = -5.0f * 0.0262f;
            protocolChange[106] = 0f;
            protocolChange[107] = 5.0f * 0.0262f;
            protocolChange[108] = 0f;
            protocolChange[109] = 10.0f * 0.0262f;
            protocolChange[110] = 0f;
            protocolChange[111] = 10.0f * 0.0262f;
            protocolChange[112] = 0f;
            protocolChange[113] = -5.0f * 0.0262f;
            protocolChange[114] = 0f;
            protocolChange[115] = 10.0f * 0.0262f;
            protocolChange[116] = 0f;
            protocolChange[117] = -10.0f * 0.0262f;
            protocolChange[118] = 0f;
            protocolChange[119] = 1.0f * 0.0262f;
            protocolChange[120] = 0f;


            return protocolChange;
        }

    }
}
