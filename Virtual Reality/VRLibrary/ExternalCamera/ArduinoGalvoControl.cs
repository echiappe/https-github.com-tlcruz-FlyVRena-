using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VRLibrary.ExternalCamera
{
    public class ArduinoGalvoControl : IDisposable
    {
        public ConcurrentStack<float[]> queue = new ConcurrentStack<float[]>();
        public ManualResetEventSlim MREvent = new ManualResetEventSlim(false);
        CancellationTokenSource source = new CancellationTokenSource();

        public void StartCommunication(string portID, int bRate, string path)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                NumberFormatInfo provider = new NumberFormatInfo();
                FileStream filestream = File.OpenWrite(path);
                StreamWriter textWriter = new StreamWriter(filestream);
                SerialPort serialPort = new SerialPort(portID, bRate);
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.DataBits = 8;
                serialPort.Open();
                float[] vals = new float[3];
                int frameNo = 0;
                Thread.BeginThreadAffinity();

                while(!source.Token.IsCancellationRequested)
                {
                    if(queue.TryPop(out vals))
                    {
                        int[] data = CalibratedMirrorPosition(vals);
                        if (data[0] != 0 && frameNo != data[0])
                        {
                            frameNo = data[0];
                            serialPort.Write((1*data[2]).ToString() + "\r");
                            Thread.Sleep(1);
                            serialPort.Write((1*data[1]).ToString() + "\r");
                            Thread.Sleep(1);
                        }
                    }
                    //MREvent.Wait();
                }
                serialPort.Close();
                serialPort.Dispose();
                textWriter.Close();
                textWriter.Dispose();
                filestream.Close();
                filestream.Dispose();
            },source.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
            );
        }

        public int auxy=0;
        public int auxx=0;
        public int[] CalibratedMirrorPosition(float[] vals)
        {
            int[] cmp = new int[3];
            cmp[0] = (int)vals[0];
            cmp[1] = (int)(64*9.8*vals[1]+64*28); //8.6 * vals[2] + 21
            if (cmp[1] > 4096)
            { 
                cmp[1] = 64;
            }
            else if (cmp[1] < 0)
            {
                cmp[1] = 0;
            }
            cmp[2] = (int)(-64*8.6 * vals[2] + 64*21); //-9.8*vals[1]+28
            if (cmp[2] > 4096)
            {
                cmp[2] = 64;
            }
            else if (cmp[2] < 0)
            {
                cmp[2] = 0;
            }
            //cmp[1] = (int)(1023 * (vals[1] + 3.81f) / (4.896f + 3.8050f));//cmp[0] % 1023;//
            //cmp[2] = 0;// (int)(1023 * (vals[2] + 5.72f) / (3.7995f + 5.7130f));//cmp[0] % 1023;//
            return cmp; 
        }
        public void Dispose()
        {
            queue.Clear();
            MREvent.Dispose();
            source.Dispose();
        }
    }
}
