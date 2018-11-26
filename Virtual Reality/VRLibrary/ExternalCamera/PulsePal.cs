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
        //queue of mirror movements
        public ConcurrentStack<float[]> queue = new ConcurrentStack<float[]>();
        public ManualResetEventSlim MREvent = new ManualResetEventSlim(false);
        CancellationTokenSource source = new CancellationTokenSource();

        //Vars. to communicate with the PulsePal
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

        //Comuunicate with PulsePal via serial port
        public void StartCommunication(string portID)
        {
            //Do the communication in a separate Thread
            Task t = Task.Factory.StartNew(() =>
            {
                //Establish communication
                serialPort = new SerialPort(portID);
                serialPort.BaudRate = BaudRate;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.Parity = Parity.None;
                serialPort.DtrEnable = false;
                serialPort.RtsEnable = true;

                //when data received run the dataReceved routine
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
                byte[] responseBuffer = new byte[4];
                byte[] commandBuffer = new byte[MaxDataBytes];
                readBuffer = new byte[serialPort.ReadBufferSize];

                //open serial port
                serialPort.Open();
                serialPort.ReadExisting();
                commandBuffer[0] = OpMenu;
                commandBuffer[1] = HandshakeCommand;
                serialPort.Write(commandBuffer, 0, 2);

                float[] vals = new float[3];
                Thread.BeginThreadAffinity();

                //While there is no cancellation request
                while (!source.Token.IsCancellationRequested)
                {
                    //read mirror movements from queue
                    if (queue.TryPop(out vals))
                    {
                        //re-calibrate mirror movements
                        int[] data = CalibratedMirrorPosition(vals);

                        //send values to PulsePal
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
                    //wait for another call to update postion - for synch purposes
                    MREvent.Wait();
                    MREvent.Reset();
                }
                serialPort.Close();
                serialPort.Dispose();
            }, source.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
            );
        }

        //re-calibrate mirror movements
        //callibration values are pre-defined. Re-compile for new callibration
        public int auxy = 128;
        public int auxx = 128;
        public int[] CalibratedMirrorPosition(float[] vals)
        {
            int[] cmp = new int[3];
            cmp[0] = (int)vals[0];
            cmp[2] = (int)Math.Round(28.8 * vals[1] + 0.6 * vals[2] + 164+7, MidpointRounding.AwayFromZero);
            if (cmp[2] > 255)
            {
                cmp[2] = 255;
            }
            else if (cmp[2] < 0)
            {
                cmp[2] = 0;
            }
            cmp[1] = (int)Math.Round(-1 * vals[1] - 26 * vals[2] + 107, MidpointRounding.AwayFromZero);
            if (cmp[1] > 255)
            {
                cmp[1] = 255;
            }
            else if (cmp[1] < 0)
            {
                cmp[1] = 0;
            }
            return cmp;
        }

        //function to handle received data
        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //get number of bytes to read
            var bytesToRead = serialPort.BytesToRead;
            if (serialPort.IsOpen && bytesToRead > 0)
            {
                //read and then process
                bytesToRead = serialPort.Read(readBuffer, 0, bytesToRead);
                for (int i = 0; i < bytesToRead; i++)
                {
                    //process input from PulsePal
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
            //if input data is acknowledge, then declare initialized, otherwise break
            switch (inputData)
            {
                case Acknowledge:
                    initialized = true;
                    break;
                default:
                    break;
            }
        }

        //Dispose of all initiated objects
        public void Dispose()
        {
            queue.Clear();
            MREvent.Dispose();
            source.Dispose();
        }
    }
}
