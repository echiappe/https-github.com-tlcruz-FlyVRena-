using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Collections.Concurrent;
using System.IO;
using System.Globalization;
using System.Threading;

namespace VRLibrary.ExternalCamera
{
    public class ReadCOMPort : IDisposable
    {
        //private bool start = true;
        public ConcurrentStack<int> queue = new ConcurrentStack<int>();
        public int currentFrame;
        private readonly object _lock = new object();
        public SerialPort serialPort;
        public bool flag = false;
        public string val = "";
        private int currentVal;
        private int toStore;
        private int toStorePrev;
        private int toStoreI;
        private int toStorePrevI=5;
        //FileStream filestream;
        //StreamWriter textWriter;
        public NumberFormatInfo provider = new NumberFormatInfo();

        public ReadCOMPort(string portID, int bRate)
        {
            provider.NumberDecimalSeparator = ".";
            //filestream = File.OpenWrite("C:\\Users\\User\\Desktop\\FromSerial.txt");
            //textWriter = new StreamWriter(filestream);
            serialPort = new SerialPort(portID, bRate);
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.DataBits = 8;
            serialPort.Handshake = Handshake.None;
            serialPort.DtrEnable = true;
            serialPort.Open();
        }

        public void ReadPort()
        {
            Thread.BeginThreadAffinity();
            while (serialPort.IsOpen)
            {
                val = serialPort.ReadLine();
                toStorePrevI = toStoreI;
                toStorePrev = toStore;
                if (Int32.TryParse(val, NumberStyles.Float, provider, out currentVal))
                {
                    if (currentVal > 600)
                    {
                        if (currentVal > 695)
                        {
                            toStoreI = 3;
                        }
                        else if(currentVal < 670)
                        {
                            toStoreI = 1;
                        }
                        else
                        {
                            toStoreI = 2;
                        }
                    }
                    else
                    {
                        toStoreI = 0;
                    }

                    //if (toStorePrevI != 5)
                    //{
                    //    if (toStoreI == toStorePrevI)
                    //    {
                            toStore = toStoreI;
                    //    }
                    //    else
                    //    {
                    //        toStore = toStorePrev;
                    //    }
                    //}

                    lock (_lock)
                    {
                        currentFrame = toStore;
                    }
                }
            }
        }

        public static void FileWrite(string toWrite, TextWriter textWriter)
        {
            textWriter.Write(toWrite);
            textWriter.Write("\r\n");
            textWriter.Flush();
        }

        public void Dispose()
        {
            serialPort.Close();
            serialPort.Dispose();
        }
    }
}
