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
    public class SmallObjectVelocityUpdate : UpdateService
    {
        KalmanFilterTrack flyPos;
        
        PositionService posServ;

        // convenient constants
        const float pi = (float)Math.PI;
        const float radians = pi * 2.0f;

        const float maxspeed = 100.0f;              // max speed ~ 80 px/s = 10 mm/s 
        const float obj_radius = 80.0f;              // size of object in px
        const float crit_dist = 80.0f + obj_radius; // critical distance to start object motion (object radius plus 5 mm)
        const float per_second = 1.0f/60.0f;        // step time
        const float gain = 0.01f;
        Vector3 offset = new Vector3(0.61f, -0.62f, 15.5f);               // X offset projection-to-camera
        const float offY = -0.62f;              // Y offset projection-to-camera
        float speed;                            // object motion speed
        Vector3 velocity;                       // object velocity vector
        Vector3 rw_vec;                         // random walk velocity vector
        float rwdir = 0.0f;
        double counter = 0.0;
        float posX = 0.0f;
        float posY = 0.0f;
        float rotrad = 2.5f;
        float rot =  0.1f * radians * per_second;
        KeyboardState oldState;
        long pframe = 0;
        
        public SmallObjectVelocityUpdate(IServiceContainer wObj, Game game)
            : base(wObj, game)
        {
            flyPos = (KalmanFilterTrack)game.Services.GetService(typeof(KalmanFilterTrack));
            posServ = (PositionService)wObj.GetService(typeof(PositionService));
            posServ.position = offset;
            pType = (VRProtocol)game.Services.GetService(typeof(VRProtocol));
            NameService name = (NameService)wObj.GetService(typeof(NameService));
            if ((UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem)) != null)
            {
                UpdateSubsystem us = (UpdateSubsystem)game.Services.GetService(typeof(UpdateSubsystem));
                us.AddUpdateService(name.ObjectName() + "UpdateWithFly", this);
            }
            pType.pType = VRProtocolType.ClosedLoop;
        }

        public override void Update(GameTime gametime)
        {
            if (pframe != pType.currentFrame)
            {
                //updateAttract(0.5);
                //updateRandomWalk();
                //updateVelocity();
                updateRotation();
                /*Console.Write("X = ");
                Console.Write(posServ.position.X);
                Console.Write(" Y = ");
                Console.WriteLine(posServ.position.Y);*/
                updateBounds();
            }
            pframe = pType.currentFrame;


            // Manual calibration
            // manualCalibration();
        }

        private float getAngleToFly(Vector3 val)
        {
            Vector3 flyPosVec = new Vector3(flyPos.pars[0], flyPos.pars[1], 0.0f);
            return (float)Math.Atan2(flyPosVec.Y - getInPixel(val).Y, flyPosVec.X - getInPixel(val).X);
        }

        private float getAngleToCenter(Vector3 val)
        {
            return (float)Math.Atan2(val.Y, val.X);
        }

        private float getDistToCenter(Vector3 val)
        {
            Vector3 corrected = val;
            corrected = Vector3.Add(corrected, -offset);
            return corrected.Length();
        }

        private float getDistToFly(Vector3 val)
        {
            return Vector3.Distance(new Vector3(flyPos.pars[0], flyPos.pars[1], 0.0f), getInPixel(val));
        }

        private Vector3 getInPixel(Vector3 val)
        {
            Vector3 outvec = new Vector3((-1.0f / 0.008889f) * (val.Y - 3.298f), (1.0f / 0.008889f) * (val.X + 3.213f), 0.0f);
            return outvec;
        }

        private Vector3 getInPU(Vector3 val)
        {
            Vector3 outvec = new Vector3(0.008889f * val.Y - 3.213f, -0.008897f * val.X + 3.298f, 0.0f);
            return outvec;
        }

        private float randomNormal(double mean, double SD)
        {
            Random rand = new Random();
            double u1 = rand.NextDouble(); //uniform samples
            double u2 = rand.NextDouble(); //uniform samples
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(u2 * radians); // Box-Muller transform
            double randNormal = mean + SD * randStdNormal;
            return (float)randNormal;
        }

        private float randomUnif(double min, double max)
        {
            Random rand = new Random();
            double randNum = rand.NextDouble(); //uniform samples
            randNum = (max - min) * randNum + min;
            return (float)randNum;
        }

        private void rotateConstant()
        {
            posServ.position.X = rotrad * (float)Math.Cos(counter) + offset.X;
            posServ.position.Y = rotrad * (float)Math.Sin(counter) + offset.Y;
            counter += rot;
        }

        private void rotateInOut()
        {
            float inner_freq = 6.0f;
            float inout_rad = 0.5f * (float)Math.Sin(inner_freq * counter) + 2.5f;
            posServ.position.X = inout_rad * (float)Math.Cos(counter) + offset.X;
            posServ.position.Y = inout_rad * (float)Math.Sin(counter) + offset.Y;
            counter += rot;
        }

        private void updateAttract(double instrength)
        {
            float strength = (float)instrength;      // Strength of attraction to fly
            float attr_radius = 200.0f; // Minimum radius for attraction 
            if (getDistToFly(posServ.position) > attr_radius)
            {
                float angle = getAngleToFly(posServ.position) + (float)Math.PI;
                Console.WriteLine(getAngleToFly(posServ.position));
                //Console.WriteLine("Attracting...");
                Vector3 attract = new Vector3(-(float)Math.Sin(angle), (float)Math.Cos(angle), 0.0f);
                attract = Vector3.Multiply(attract, strength);
                posServ.position += attract * per_second;
            }
        }

        private void updateBounds()
        {
            float objDist = getDistToCenter(posServ.position);
            //Console.Write("r = ");
            //Console.WriteLine(objDist);

            // if length of object vector is larger than 3.0
            float out_radius = 3.0f;
            if (objDist > out_radius)
            {
                double return_angle = getAngleToCenter(posServ.position-offset);
                posServ.position.X = out_radius * (float)Math.Cos(return_angle) + offset.X;
                posServ.position.Y = out_radius * (float)Math.Sin(return_angle) + offset.Y;
            }
        }

        private void updateRandomWalk()
        {
            float rwspeed = 0.4f;
            rwdir += 5.0f * randomNormal(0.0, radians) * per_second;
            rw_vec = new Vector3(-(float)Math.Sin(rwdir), (float)Math.Cos(rwdir), 0.0f);
            rw_vec = Vector3.Multiply(rw_vec, rwspeed);
            posServ.position += rw_vec * per_second;
        }

        private void updateRotation()
        {
            float dist = getDistToFly(posServ.position);

            // object only moves when the fly is close to the object
            if (dist < crit_dist)
            {
                speed = 1.0f * radians * (crit_dist - dist) / crit_dist;
            }
            else
            {
                speed = 0.0f;
            }
            Vector3 flyPosVec = new Vector3(flyPos.pars[0], flyPos.pars[1], 0.0f);
            flyPosVec = getInPU(flyPosVec);
            float fly_angle = getAngleToCenter(flyPosVec);
            float obj_angle = getAngleToCenter(posServ.position - offset);
            float angle = obj_angle + (float)Math.Sign(obj_angle - fly_angle) * speed * per_second;
            Console.Write("Obj = ");
            Console.Write(obj_angle);
            Console.Write(" Fly = ");
            Console.WriteLine(fly_angle);

            // velocity vector (magnitude is given by distance of fly to object; direction is given by angle fly to object)
            posServ.position.X = rotrad * (float)Math.Cos(angle) + offset.X;
            posServ.position.Y = rotrad * (float)Math.Sin(angle) + offset.Y;
        }

        private void updateVelocity()
        {
            float dist = getDistToFly(posServ.position);

            // object only moves when the fly is close to the object
            if (dist < crit_dist)
            {
                speed = maxspeed * (crit_dist - dist) / crit_dist;
            }
            else
            {
                speed = 0.0f;
            }
            float angle = getAngleToFly(posServ.position);

            // velocity vector (magnitude is given by distance of fly to object; direction is given by angle fly to object)
            velocity = new Vector3(-(float)Math.Sin(angle), (float)Math.Cos(angle), 0.0f);
            velocity = Vector3.Multiply(velocity, speed);

            // Update new position
            posServ.position += velocity * per_second + rw_vec * per_second;
        }

        private void manualCalibration()
        {
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.W))
            {
                if (oldState.IsKeyUp(Keys.W))
                {
                    posX += gain;
                    Console.Write("X = ");
                    Console.Write(posX);
                    Console.Write(" Y = ");
                    Console.WriteLine(posY);
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                if (oldState.IsKeyUp(Keys.S))
                {
                    posX -= gain;
                    Console.Write("X = ");
                    Console.Write(posX);
                    Console.Write(" Y = ");
                    Console.WriteLine(posY);
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (oldState.IsKeyUp(Keys.D))
                {
                    posY += gain;
                    Console.Write("X = ");
                    Console.Write(posX);
                    Console.Write(" Y = ");
                    Console.WriteLine(posY);
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                if (oldState.IsKeyUp(Keys.A))
                {
                    posY -= gain;
                    Console.Write("X = ");
                    Console.Write(posX);
                    Console.Write(" Y = ");
                    Console.WriteLine(posY);
                }
            }
            oldState = newState;
        }

    }
}