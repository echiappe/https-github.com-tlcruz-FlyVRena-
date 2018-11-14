using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using VRLibrary.ImageProcessing;
using VRLibrary.Stimulus.Subsystems;

namespace VRLibrary.Stimulus.Services.UpdateServices
{
    public class SmallObjectOpenLoop : UpdateService
    {
        static KalmanFilterTrack flyPos;
        static PositionService posServ;
        
        //Random rand = new Random(Guid.NewGuid().GetHashCode());
        const float minChangeVel = 1.5f;
        const float maxChangeVel = 5.0f;
        const float changePrecision = 0.001f;
        const float minChangeTheta = 3.0f;
        const float maxChangeTheta = 20.0f;
        const float changeTheta = 0.1f;

        // convenient constants
        const float pi = (float)Math.PI;
        const float radians = pi * 2.0f;

        const float maxspeed = 100.0f;              // max speed ~ 80 px/s = 10 mm/s 
        const float obj_radius = 80.0f;              // size of object in px
        const float crit_dist = 80.0f + obj_radius; // critical distance to start object motion (object radius plus 5 mm)
        const float per_second = 1.0f / 60.0f;        // step time
        const float gain = 0.01f;

        const float offY = -0.62f;              // Y offset projection-to-camera
        static float objectSpeed = 5.0f;
        static float objectOrientation = 5.00f;
        static float posX = 0.0f;
        static float posY = 0.0f;

        static float[] allChangesVels = linspace2(minChangeVel, maxChangeVel, changePrecision);

        const float aExp = 0.1990f;                    // exponential constants fits obtained from all female flies, for the velocity
        const float bExp = -3.9579f;                  // 
        const float cExp = 0.0071f;

        static float[] edgesExpVel = getExpPD(allChangesVels, aExp, bExp, cExp);

        static float[] allChangesTheta = linspace2(minChangeTheta, maxChangeTheta, changeTheta);

        const float aThetaExp = 0.7598f;
        const float bThetaExp = -0.3269f;
        const float cThetaExp = 0.8615f;

        static float[] edgesExpTheta = getExpPD(allChangesTheta, aThetaExp, bThetaExp, cThetaExp);

        static int timeInteracting = 0;
        static float prevFlyX = -1.0f;
        static float prevFlyY = -1.0f;
        const float minInteractDistance = 3.0f;
        const float minInteractAngle = 25.0f;
        const float criticalDist = 1.3f;

        float rwdir = 0.0f;
        float counter = 0.0f;
        float rotrad = 2.5f;
        float rot = 0.1f * radians * per_second;
        //KeyboardState oldState;
        long pframe = 0;
        float[] c = new float[12];
        Game g;
        NameService name;

        public SmallObjectOpenLoop(IServiceContainer wObj, Game game)
            : base(wObj, game)
        {
            flyPos = (KalmanFilterTrack)game.Services.GetService(typeof(KalmanFilterTrack));
            posServ = (PositionService)wObj.GetService(typeof(PositionService));
            pType = (VRProtocol)game.Services.GetService(typeof(VRProtocol));
            name = (NameService)wObj.GetService(typeof(NameService));
            if ((UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem)) != null)
            {
                UpdateSubsystem us = (UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem));
                us.AddUpdateService(name.ObjectName() + "SmallObjectOpenLoop", this);
            }
            pType.pType = VRProtocolType.ClosedLoop;
            //v[0] = 18000;
            //v[1] = 54000;
            //v[2] = 72000;
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


        private static float[] linspace(float x1, float x2, int n)
        {
            float step = (x2 - x1) / (n - 1);
            float[] y = new float[n];
            for (int i = 0; i < n; i++)
            {
                y[i] = x1 + step * i;
            }
            return y;
        }

        private static float[] linspace2(float startP, float endP, float step)
        {
            int vectorLength = (int)Math.Floor((endP - startP) / (step)) + 1;
            float[] outputVec = new float[vectorLength];
            for (int i = 0; i < vectorLength; i++)
            {
                outputVec[i] = startP + step * i;
            }
            return outputVec;
        }

        private static float[] getExpPD(float[] allChangeVels, float aExp, float bExp, float cExp)
        {
            float[] yExp = getExponentialDistribution(allChangeVels, aExp, bExp, cExp);
            yExp = divideArr(yExp, yExp.Sum());
            float[] outputVect = new float[yExp.Length + 1];
            float[] initialVal = new float[1];
            initialVal[0] = 0.00f;
            outputVect = initialVal.Concat(yExp).ToArray();
            outputVect = cumsum(outputVect);
            return outputVect;
        }

        private static float[] cumsum(float[] arr)
        {
            float[] outputArr = new float[arr.Length];
            outputArr[0] = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                outputArr[i] = outputArr[i - 1] + arr[i];
            }
            return outputArr;
        }

        private static float getNextVelocity(float[] edges, float[] allChangesVel, float prevVel, float prob)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            float randomVal = (float)rnd.NextDouble();
            int isPos = Convert.ToInt32(rnd.NextDouble() >= prob);
            int edgesIdx = Array.FindIndex(edges, x => x > randomVal) - 1;
            float currVel = allChangesVel[edgesIdx];
            float nextVel;
            if (prevVel - currVel <= 0)
            {
                nextVel = prevVel + currVel;
            }
            else if (prevVel + currVel >= 25)
            {
                nextVel = prevVel - currVel;
            }
            else {
                nextVel = prevVel + (float)Math.Pow(-1, 1 + isPos) * currVel;
            }
            return nextVel;
        }

        private static float[] getNextOrientation(float[] edges, float[] allChangesVel, float prevTheta, float prevX, float prevY, float speed, bool isChasing)
        {
            speed = speed / 60;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            int isPos = rnd.Next(0, 1);
            float[] objVars = getRandomVariable(edges, allChangesVel, prevTheta, prevX, prevY, speed, isPos);
            float newTheta = objVars[0];
            float newX = objVars[1];
            float newY = objVars[2];

            if (isChasing == true)
            {
                float[] objVars2 = getRandomVariable(edges, allChangesVel, prevTheta, prevX, prevY, speed, Math.Abs(isPos - 1));
                float newTheta2 = objVars[0];
                float newX2 = objVars[1];
                float newY2 = objVars[2];

                float distFirst = dist(newX, newY, flyPos.pars[0], flyPos.pars[1]);
                float distSecond = dist(newX2, newY2, flyPos.pars[0], flyPos.pars[1]);

                if (distFirst > distSecond)
                {

                }
                else {
                    newTheta = newTheta2;
                    newX = newX2;
                    newY = newY2;
                }
            }


            if (newX >= 16 || newY >= 16 || newX < 0 || newY < 0)
            { // change these limits for the arena values

                objVars = getRandomVariable(edges, allChangesVel, prevTheta, prevX, prevY, speed, isPos + 1);
                newTheta = objVars[0];
                newX = objVars[1];
                newY = objVars[2];

                //if (newX >= 16 || newY >= 16 || newX < 0 || newY < 0)
                //{
                //    int cnt = 0;
                //    while (newX >= 16 || newY >= 16 || newX < 0 || newY < 0)
                //    {
                //        cnt += cnt;
                //        allChangesVel = sumArr(allChangesVel, 1);
                //        int isPos2 = rnd.Next(0, 1);
                //        if (isChasing == true)
                //        {
                //            isPos2 = isPos;
                //        }
                //        objVars = getRandomVariable(edges, allChangesVel, prevTheta, prevX, prevY, speed, isPos2);
                //        newTheta = objVars[0];
                //        newX = objVars[1];
                //        newY = objVars[2];
                //        //Console.Write(cnt);
                //    }
                //}
            }
            return objVars;
        }

        private static float[] getRandomVariable(float[] edges, float[] allChangesVel, float prevTheta, float prevX, float prevY, float speed, int isPos)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            double randomVal = rnd.NextDouble();
            int edgesIdx = Array.FindIndex(edges, x => x > randomVal) - 1;
            float currTheta = allChangesVel[edgesIdx];
            float[] outputArray = new float[3];
            float newTheta = degToRad(prevTheta + (float)Math.Pow(-1, 1 + isPos) * currTheta);
            float newX = prevX + (float)Math.Cos(newTheta) * speed;
            float newY = prevY + (float)Math.Sin(newTheta) * speed;
            outputArray[0] = radToDeg(newTheta);
            outputArray[1] = newX;
            outputArray[2] = newY;
            return outputArray;
        }

        private static float dist(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }


        private static void isInteracting()
        {
            if (prevFlyX < 0 && prevFlyY < 0)
            {
                return;
            }
            float distToFly = dist(posX, posY, flyPos.pars[0], flyPos.pars[0]);
            float angleToFly = (float)Math.Atan2(posY - flyPos.pars[1], posX - flyPos.pars[0]);

            if (distToFly <= minInteractDistance && Math.Abs(angleToFly) <= minInteractAngle)
            {
                timeInteracting += 1;
            }
            else {
                timeInteracting = 0;
            }
        }


        public override void Update(GameTime gametime)
        {
            Console.Write("Before");
            Console.Write(posServ.position.X);
            posServ.rotation.X = 90;
            float speed;
            bool isChasing;
            isInteracting();
            float currDist = dist(flyPos.pars[0], flyPos.pars[1], posX, posY);
            if (timeInteracting > 20 || currDist <= criticalDist)
            {
                speed = getNextVelocity(edgesExpVel, allChangesVels, objectSpeed, 0.25f);
                isChasing = true;
            }
            else {
                speed = getNextVelocity(edgesExpVel, allChangesVels, objectSpeed, 0.5f);
                isChasing = false;
            }

            //Console.Write("vel ");
            float[] objVars = getNextOrientation(edgesExpTheta, allChangesTheta, objectOrientation, posServ.position.X, posServ.position.Y, speed, isChasing);
            //float[] objVars = getNextOrientation(edgesExpTheta, allChangesTheta, objectOrientation, posX, posY, speed, isChasing);
            //Console.Write("theta ");
            objectSpeed = speed;
            objectOrientation = objVars[0];
            posServ.position.X = objVars[1];
            posServ.position.Y = objVars[2];
            posX = objVars[1];
            Console.Write("After transformation");
            Console.Write(posX);
            posY = objVars[2];

            prevFlyX = flyPos.pars[0];
            prevFlyY = flyPos.pars[1];
            //  Console.Write("donezo");
        }

        private static float[] sumArr(float[] x, float factor)
        {
            if (x == null) throw new ArgumentNullException();
            return x.Select(r => r + factor).ToArray();
        }

        private static float[] multiplyArr(float[] x, float factor)
        {
            if (x == null) throw new ArgumentNullException();
            return x.Select(r => r * factor).ToArray();
        }

        private static float[] divideArr(float[] x, float factor)
        {
            if (x == null) throw new ArgumentNullException();
            return x.Select(r => r / factor).ToArray();
        }

        private static float[] getExponentialDistribution(float[] allChangeVels, float a, float b, float c)
        {
            int len = allChangeVels.Length;
            float[] expVals = new float[len];
            for (int i = 0; i < len; i++)
            {
                expVals[i] = a * (float)Math.Exp(allChangeVels[i] * b) + c;
            }

            return expVals;
        }

        private static float degToRad(float angle)
        {
            return angle * (float)Math.PI / 180;
        }

        private static float radToDeg(float angle)
        {
            return angle * 180 / (float)Math.PI;
        }

        //public static void Main()
        //{
        //    //var watch = System.Diagnostics.Stopwatch.StartNew();


        //    for (int i = 0; i < 10; i++)
        //    {
        //        //Console.Write(i);
        //        updateStimPosition();
        //        //watch.Stop();
        //        //var elapsedMs = watch.ElapsedMilliseconds;
        //        //Console.Write(elapsedMs);
        //        //Console.Write(nextVal);
        //        Console.Write("X = ");
        //        Console.Write(posX);
        //        Console.Write(" Y = ");
        //        Console.Write(posY);
        //        Console.Write(" Theta = ");
        //        Console.Write(objectOrientation);
        //        Console.Write(" Speed = ");
        //        Console.Write(objectSpeed);
        //        Console.Write("\n");
        //    }
        //    Console.ReadLine();
        //}

    }
}


