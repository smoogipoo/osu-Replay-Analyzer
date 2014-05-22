﻿//Todo: Try/Catch Load() Methods

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace ReplayAPI
{
    public class Replay : IDisposable
    {
        public GameModes GameMode;
        public string Filename;
        public int FileFormat;
        public string MapHash;
        public string PlayerName;
        public string ReplayHash;
        public int TotalScore;
        public int Count_300;
        public int Count_100;
        public int Count_50;
        public int Count_Geki;
        public int Count_Katu;
        public int Count_Miss;
        public int MaxCombo;
        public int IsPerfect;
        public Modifications Mods;
        public List<LifeInfo> LifeData = new List<LifeInfo>();
        public DateTime PlayTime;
        public int ReplayLength;
        public List<ReplayInfo> ReplayFrames = new List<ReplayInfo>();
        public List<ReplayInfo> ClickFrames = new List<ReplayInfo>();

        private FileStream replayFileStream;
        private BinaryReader replayReader;

        public Replay() { }
        public Replay(string replayFile)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
            Open(replayFile);
            LoadReplayData();
        }

        ~Replay()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool state)
        {
            if (replayFileStream != null)
                replayFileStream.Dispose();
            if (replayReader != null)
                replayReader.Dispose();
            ReplayFrames.Clear();
            LifeData.Clear();
            ClickFrames.Clear();
        }


        public void Open(string replayFile)
        {
            Filename = replayFile;
            Dispose(true); //Clear any previous data
            try
            {
                replayFileStream = new FileStream(replayFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                replayReader = new BinaryReader(replayFileStream);
            }
            catch
            {
                throw new IOException("The replay file could not be opened! Make sure that the replay file is not in use and try again.");
            }
        }

        public void LoadReplayData()
        {
            if (replayReader != null)
            {
                if (replayReader.BaseStream.Position == 0)
                {
                    LoadMetadata();
                }
                bool lifeExists = int.Parse(GetReversedString(replayReader, 1), NumberStyles.HexNumber) == 0x0B;
                if (lifeExists)
                {
                    string tempLifeStr = Encoding.ASCII.GetString(replayReader.ReadBytes(GetChunkLength(replayReader)));
                    foreach (LifeInfo tempLife in tempLifeStr.Split(',').Select(lifeStr => lifeStr.Split('|')).Where(lifeStr => lifeStr.Length > 1).Select(lifeStr => new LifeInfo
                    {
                        Time = Convert.ToInt32(lifeStr[0]),
                        Percentage = Convert.ToDouble(lifeStr[1]),
                    }))
                    {
                        LifeData.Add(tempLife);
                    }
                }
                long timeStamp = long.Parse(GetReversedString(replayReader, 8), NumberStyles.HexNumber);
                PlayTime = new DateTime(timeStamp);

                ReplayLength = int.Parse(GetReversedString(replayReader, 4), NumberStyles.HexNumber);

                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] bytesToWrite = replayReader.ReadBytes(ReplayLength + 1);
                    ms.Write(bytesToWrite, 0, bytesToWrite.Length);
                    ms.Position = 0;

                    byte[] properties = new byte[5];
                    if (ms.Read(properties, 0, 5) != 5) { }
                    SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
                    decoder.SetDecoderProperties(properties);
                    long outSize = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        int v = ms.ReadByte();
                        if (v < 0)
                            break;
                        outSize |= ((long)(byte)v) << (8 * i);
                    }
                    long compressedSize = ms.Length - ms.Position;
                    MemoryStream outStream = new MemoryStream();
                    decoder.Code(ms, outStream, compressedSize, outSize, null);
                    outStream.Flush();
                    outStream.Position = 0;

                    string outString;
                    using (StreamReader reader = new StreamReader(outStream))
                    {
                        outString = reader.ReadToEnd();
                    }
                    int lastTime = 0;
                    KeyData lastKey = KeyData.None;
                    foreach (string splitStr in outString.Split(',').Where(splitStr => splitStr != ""))
                    {
                        string[] reSplit = splitStr.Split('|');
                        ReplayInfo tempInfo = new ReplayInfo();
                        tempInfo.TimeDiff = Convert.ToInt64(reSplit[0]);
                        lastTime += (int)tempInfo.TimeDiff;
                        tempInfo.Time = lastTime;
                        tempInfo.X = Convert.ToDouble(reSplit[1]);
                        tempInfo.Y = Convert.ToDouble(reSplit[2]);
                        tempInfo.Keys = (KeyData)Convert.ToInt32(reSplit[3]);
                        if (tempInfo.Keys != KeyData.None && lastKey != tempInfo.Keys)
                        {
                            ClickFrames.Add(tempInfo);
                        }
                        ReplayFrames.Add(tempInfo);
                        lastKey = tempInfo.Keys;
                    }
                }
            }      
        }

        public void LoadMetadata()
        {
            if (replayReader != null)
            {
                GameMode = (GameModes)Enum.Parse(typeof(GameModes), replayReader.ReadByte().ToString(CultureInfo.InvariantCulture));

                FileFormat = int.Parse(GetReversedString(replayReader, 4), NumberStyles.HexNumber);

                replayReader.ReadByte();
                MapHash = Encoding.ASCII.GetString(replayReader.ReadBytes(GetChunkLength(replayReader))); //Hash type: MD5

                replayReader.ReadByte();
                PlayerName = Encoding.ASCII.GetString(replayReader.ReadBytes(GetChunkLength(replayReader)));

                replayReader.ReadByte();
                ReplayHash = Encoding.ASCII.GetString(replayReader.ReadBytes(GetChunkLength(replayReader))); //Hash type: MD5

                Count_300 = int.Parse(GetReversedString(replayReader, 2), NumberStyles.HexNumber);
                Count_100 = int.Parse(GetReversedString(replayReader, 2), NumberStyles.HexNumber);
                Count_50 = int.Parse(GetReversedString(replayReader, 2), NumberStyles.HexNumber);
                Count_Geki = int.Parse(GetReversedString(replayReader, 2), NumberStyles.HexNumber);
                Count_Katu = int.Parse(GetReversedString(replayReader, 2), NumberStyles.HexNumber);
                Count_Miss = int.Parse(GetReversedString(replayReader, 2), NumberStyles.HexNumber);

                TotalScore = int.Parse(GetReversedString(replayReader, 4), NumberStyles.HexNumber);

                MaxCombo = int.Parse(GetReversedString(replayReader, 2), NumberStyles.HexNumber);

                IsPerfect = replayReader.ReadByte();

                Mods = (Modifications)int.Parse(GetReversedString(replayReader, 4), NumberStyles.HexNumber);
            }
        }

        static string GetReversedString(BinaryReader br, int length)
        {
            byte[] readBytes = br.ReadBytes(length).Reverse().ToArray();
            return readBytes.Aggregate("", (current, b) => current + (b < 16 ? "0" : "") + b.ToString("X"));
        }
        static int GetChunkLength(BinaryReader br)
        {
            int shift = 0;
            int chunkLength = 0;
            while (true)
            {
                byte b = br.ReadByte();
                chunkLength |= (b & 127) << shift;
                if ((b & 128) == 0)
                    break;
                shift += 7;
            }
            return chunkLength;
        }

        public enum GameModes
        {
            osu = 0,
            Taiko = 1,
            CtB = 2,
            Mania = 3
        }
    }
}
