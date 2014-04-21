using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using smgiFuncs;

namespace ReplayAPI
{
    public class Replay
    {
        public GameMode gameMode;
        public int fileFormat;
        public string mapHash;
        public string playerName;
        public string replayHash;
        public int totalScore;
        public int count_300;
        public int count_100;
        public int count_50;
        public int count_geki;
        public int count_katu;
        public int count_miss;

        public int maxCombo;
        public int isPerfect;
        public Modifications mods;
        public List<LifeInfo> lifeData = new List<LifeInfo>();
        public List<ReplayInfo> replayData = new List<ReplayInfo>();
        public List<ReplayInfo> clicks = new List<ReplayInfo>();

        public Replay(string replayFile)
        {
            Parse(replayFile);
        }

        public void Parse(string replayFile)
        {
            using (FileStream fs = new FileStream(replayFile, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                gameMode = (GameMode)Enum.Parse(typeof(GameMode), br.ReadByte().ToString());

                fileFormat = int.Parse(GetReversedString(br, 4), NumberStyles.HexNumber);

                br.ReadByte();
                mapHash = Encoding.ASCII.GetString(br.ReadBytes(GetChunkLength(br))); //Hash type: MD5

                br.ReadByte();
                playerName = Encoding.ASCII.GetString(br.ReadBytes(GetChunkLength(br)));

                br.ReadByte();
                replayHash = Encoding.ASCII.GetString(br.ReadBytes(GetChunkLength(br))); //Hash type: MD5

                count_300 = int.Parse(GetReversedString(br, 2), NumberStyles.HexNumber);
                count_100 = int.Parse(GetReversedString(br, 2), NumberStyles.HexNumber);
                count_50 = int.Parse(GetReversedString(br, 2), NumberStyles.HexNumber);
                count_geki = int.Parse(GetReversedString(br, 2), NumberStyles.HexNumber);
                count_katu = int.Parse(GetReversedString(br, 2), NumberStyles.HexNumber);
                count_miss = int.Parse(GetReversedString(br, 2), NumberStyles.HexNumber);

                totalScore = int.Parse(GetReversedString(br, 4), NumberStyles.HexNumber);

                maxCombo = int.Parse(GetReversedString(br, 2), NumberStyles.HexNumber);

                isPerfect = br.ReadByte();

                mods = (Modifications)Enum.Parse(typeof(Modifications), int.Parse(GetReversedString(br, 4), NumberStyles.HexNumber).ToString(CultureInfo.InvariantCulture));
                br.ReadByte();

                long currentpos = br.BaseStream.Position;

                try
                {
                    string tempLifeStr = Encoding.ASCII.GetString(br.ReadBytes(GetChunkLength(br)));
                    foreach (string splitStr in Regex.Split(tempLifeStr, ","))
                    {
                        if (splitStr == "")
                            continue;
                        sString tempStr = splitStr;
                        LifeInfo tempLife = new LifeInfo();
                        tempLife.Time = Convert.ToInt32(tempStr.SubString(0, tempStr.nthDexOf("|", 0)));
                        tempLife.Percentage = Convert.ToDouble(tempStr.SubString(tempStr.nthDexOf("|", 0) + 1));
                        lifeData.Add(tempLife);
                    }

                    string s = GetReversedString(br, 8);
                    //     timeStamp = Int64.Parse(s, System.Globalization.NumberStyles.HexNumber);  WhatAmIDoing.jpg?

                    int replayLength = int.Parse(GetReversedString(br, 2), NumberStyles.HexNumber);
                    br.ReadBytes(2);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] bytesToWrite = br.ReadBytes((int)br.BaseStream.Length);
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
                                throw (new Exception("Can't Read 1"));
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
                        foreach (string splitStr in Regex.Split(outString, ","))
                        {
                            if (splitStr == "")
                                continue;
                            sString tempStr = splitStr;
                            ReplayInfo tempInfo = new ReplayInfo();
                            tempInfo.TimeDiff = Convert.ToInt64(tempStr.SubString(0, tempStr.nthDexOf("|", 0)));
                            lastTime += (int)tempInfo.TimeDiff;
                            tempInfo.Time = lastTime;
                            tempInfo.X = Convert.ToDouble(tempStr.SubString(tempStr.nthDexOf("|", 0) + 1, tempStr.nthDexOf("|", 1)));
                            tempInfo.Y = Convert.ToDouble(tempStr.SubString(tempStr.nthDexOf("|", 1) + 1, tempStr.nthDexOf("|", 2)));
                            tempInfo.Keys = (KeyData)Enum.Parse(typeof(KeyData), tempStr.SubString(tempStr.nthDexOf("|", 2) + 1));
                            replayData.Add(tempInfo);
                        }
                    }
                }
                catch
                {
                    br.BaseStream.Position = currentpos + 12;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] bytesToWrite = br.ReadBytes((int)br.BaseStream.Length);
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
                                throw (new Exception("Can't Read 1"));
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
                        foreach (string splitStr in Regex.Split(outString, ","))
                        {
                            if (splitStr == "")
                                continue;
                            sString tempStr = splitStr;
                            ReplayInfo tempInfo = new ReplayInfo();
                            tempInfo.TimeDiff = Convert.ToInt64(tempStr.SubString(0, tempStr.nthDexOf("|", 0)));
                            lastTime += (int)tempInfo.TimeDiff;
                            tempInfo.Time = lastTime;
                            tempInfo.X = Convert.ToDouble(tempStr.SubString(tempStr.nthDexOf("|", 0) + 1, tempStr.nthDexOf("|", 1)));
                            tempInfo.Y = Convert.ToDouble(tempStr.SubString(tempStr.nthDexOf("|", 1) + 1, tempStr.nthDexOf("|", 2)));
                            tempInfo.Keys = (KeyData)Enum.Parse(typeof(KeyData), tempStr.SubString(tempStr.nthDexOf("|", 2) + 1));
                            replayData.Add(tempInfo);
                        }
                    }
                }
            }
        }

        static string GetReversedString(BinaryReader br, int length)
        {
            byte[] readBytes = br.ReadBytes(length).Reverse().ToArray();
            return readBytes.Aggregate("", (current, b) => current + b.ToString("X"));
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

        public enum GameMode
        {
            osu = 0,
            Taiko = 1,
            CtB = 2,
            Mania = 3
        }
    }
}
