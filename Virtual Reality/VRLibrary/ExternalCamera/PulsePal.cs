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
    public class PulsePal
    {
        public ConcurrentStack<float[]> queue = new ConcurrentStack<float[]>();
        public ManualResetEventSlim MREvent = new ManualResetEventSlim(false);
        CancellationTokenSource source = new CancellationTokenSource();
        public const int BaudRate = 115200;
        const int MaxDataBytes = 35;

        const byte Acknowledge = 0x4B;
        const byte OpMenu = 0xD5;
        const byte HandshakeCommand = 0x48;
        const byte ProgramParamCommand = 0x4A;
        const byte PulseTrain1Command = 0x4B;
        const byte PulseTrain2Command = 0x4C;
        const byte TriggerCommand = 0x4D;
        const byte SetDisplayCommand = 0x4E;
        const byte SetVoltageCommand = 0x4F;
        const byte AbortCommand = 0x50;
        const byte DisconnectCommand = 0x51;
        const byte LoopCommand = 0x52;
        const byte ClientIdCommand = 0x59;
        const byte LineBreak = 0xFE;
        SerialPort serialPort;
        bool initialized;
        byte[] readBuffer;
        public void StartCommunication(string portID)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                serialPort = new SerialPort(portID);
                serialPort.BaudRate = BaudRate;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.Parity = Parity.None;
                serialPort.DtrEnable = false;
                serialPort.RtsEnable = true;
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
                byte[] responseBuffer = new byte[4];
                byte[] commandBuffer = new byte[MaxDataBytes];
                readBuffer = new byte[serialPort.ReadBufferSize];


                serialPort.Open();
                serialPort.ReadExisting();
                commandBuffer[0] = OpMenu;
                commandBuffer[1] = HandshakeCommand;
                serialPort.Write(commandBuffer, 0, 2);

                float[] vals = new float[3];
                int frameNo = 0;
                Thread.BeginThreadAffinity();

                while (!source.Token.IsCancellationRequested)
                {
                    if (queue.TryPop(out vals))
                    {
                        int[] data = CalibratedMirrorPosition(vals);
                        if (data[0] != 0)
                        {
                            commandBuffer[0] = OpMenu;
                            commandBuffer[1] = SetVoltageCommand;
                            commandBuffer[2] = 1;
                            commandBuffer[3] = (byte)data[1];
                            serialPort.Write(commandBuffer, 0, 4);
                            commandBuffer[2] = 2;
                            commandBuffer[3] = (byte)data[2];
                            serialPort.Write(commandBuffer, 0, 4);
                        }
                    }
                    MREvent.Wait();
                    MREvent.Reset();
                }
                serialPort.Close();
                serialPort.Dispose();
                //textWriter.Close();
                //textWriter.Dispose();
                //filestream.Close();
                //filestream.Dispose();
            }, source.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
            );
        }

        void WriteInt(BinaryWriter writer, int value)
        {
            writer.Write((byte)value);
            writer.Write((byte)(value >> 8));
            writer.Write((byte)(value >> 16));
            writer.Write((byte)(value >> 24));
        }

        public int auxy = 128;
        public int auxx = 128;
        public int[] CalibratedMirrorPosition(float[] vals)
        {
            int[] cmp = new int[3];
            cmp[0] = (int)vals[0];
            cmp[2] = (int)Math.Round(28.8 * vals[1] + 0.6 * vals[2] + 164+7, MidpointRounding.AwayFromZero); //+28.8 * vals[1] + 1 * vals[2] + 178
            if (cmp[2] > 255)
            {
                cmp[2] = 255;
            }
            else if (cmp[2] < 0)
            {
                cmp[2] = 0;
            }
            cmp[1] = (int)Math.Round(-1 * vals[1] - 26 * vals[2] + 107, MidpointRounding.AwayFromZero); //-9.8*vals[1]+28
            if (cmp[1] > 255)
            {
                cmp[1] = 255;
            }
            else if (cmp[1] < 0)
            {
                cmp[1] = 0;
            }
            //cmp[1] = auxx % 255;
            //cmp[2] = auxy % 255;
            return cmp;
        }



        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var bytesToRead = serialPort.BytesToRead;
            if (serialPort.IsOpen && bytesToRead > 0)
            {
                bytesToRead = serialPort.Read(readBuffer, 0, bytesToRead);
                for (int i = 0; i < bytesToRead; i++)
                {
                    ProcessInput(readBuffer[i]);
                }
            }
        }


        void ProcessInput(byte inputData)
        {
            if (!initialized && inputData != Acknowledge)
            {
                throw new InvalidOperationException("Unexpected return value from PulsePal.");
            }

            switch (inputData)
            {
                case Acknowledge:
                    initialized = true;
                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            queue.Clear();
            MREvent.Dispose();
            source.Dispose();
        }
    }
}
