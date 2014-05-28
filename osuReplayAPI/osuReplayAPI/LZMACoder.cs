using System;
using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace ReplayAPI
{
    internal class LZMACoder : IDisposable
    {
        //These properties are straight copy from the SDK.
        //Actually, I don't know what these mean.
        private static Int32 posStateBits = 2;
        private static Int32 litContextBits = 3;   // for normal files  // UInt32 litContextBits = 0; // for 32-bit data                                             
        private static Int32 litPosBits = 0;       // UInt32 litPosBits = 2; // for 32-bit data
        private static Int32 numFastBytes = 128;

        private static CoderPropID[] propIDs = 
    {
        CoderPropID.PosStateBits,  
        CoderPropID.LitContextBits,
        CoderPropID.LitPosBits,
        CoderPropID.Algorithm,
        CoderPropID.NumFastBytes,
    };
        private static object[] properties = 
    {
        (Int32)(posStateBits),  
        (Int32)(litContextBits),
        (Int32)(litPosBits),
        (Int32)(numFastBytes),
    };

        private bool isDisposed;

        public LZMACoder()
        {
            if (BitConverter.IsLittleEndian == false)
            {
                Dispose();
                throw new Exception("Not implemented");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public MemoryStream Decompress(MemoryStream inStream)
        {
            return Decompress(inStream, false);
        }

        public MemoryStream Decompress(MemoryStream inStream, bool closeInStream)
        {
            inStream.Position = 0;

            var decoder = new Decoder();

            byte[] prop = new byte[5];
            if (inStream.Read(prop, 0, 5) != 5) { }
            decoder.SetDecoderProperties(prop);
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = inStream.ReadByte();
                if (v < 0)
                    break;
                outSize |= ((long)(byte)v) << (8 * i);
            }
            long compressedSize = inStream.Length - inStream.Position;

            var outStream = new MemoryStream();
            decoder.Code(inStream, outStream, compressedSize, outSize, null);
            outStream.Flush();
            outStream.Position = 0;

            if (closeInStream)
                inStream.Close();

            return outStream;
        }

        public MemoryStream Compress(MemoryStream inStream)
        {
            return Compress(inStream, false);
        }

        public MemoryStream Compress(MemoryStream inStream, bool closeInStream)
        {
            inStream.Position = 0;
            var outStream = new MemoryStream();

            var encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);

            byte[] lengthHeader = BitConverter.GetBytes(inStream.Length);
            outStream.Write(lengthHeader, 0, lengthHeader.Length);

            encoder.Code(inStream, outStream, -1, -1, null);
            outStream.Position = 0;
            if (closeInStream)
                inStream.Close();

            return outStream;
        }

        ~LZMACoder()
        {
            Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed == false)
            {
                if (disposing)
                {
                    //Console.WriteLine("dispose"); 
                    GC.SuppressFinalize(this);
                }
            }
            isDisposed = true;
        }
    }
}