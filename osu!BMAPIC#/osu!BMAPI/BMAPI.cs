﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Reflection;
using System.Globalization;


namespace BMAPI
{
    public class Beatmap : BeatmapInfo
    {
        internal readonly BeatmapInfo Info = new BeatmapInfo();
        internal readonly Dictionary<string, string> BM_Sections = new Dictionary<string, string>();
        internal readonly List<string> WriteBuffer = new List<string>();
        internal readonly Dictionary<string, int> SectionLength = new Dictionary<string, int>();

        /// <summary>
        /// Creates a new Beatmap object
        /// </summary>
        /// <param name="beatmapFile">The beatmap file to open</param>
        public Beatmap(string beatmapFile = "")
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);

            //Variable init
            BM_Sections.Add("AudioFilename,AudioLeadIn,PreviewTime,Countdown,SampleSet,StackLeniency,Mode,LetterboxInBreaks,SpecialStyle,CountdownOffset," +
                            "OverlayPosition,SkinPreference,WidescreenStoryboard,UseSkinSprites,StoryFireInFront,EpilepsyWarning,CustomSamples,EditorDistanceSpacing," +
                            "AudioHash,AlwaysShowPlayfield", "General");
            BM_Sections.Add("GridSize,BeatDivisor,DistanceSpacing,CurrentTime,TimelineZoom", "Editor");
            BM_Sections.Add("Title,TitleUnicode,Artist,ArtistUnicode,Creator,Version,Source,BeatmapID,BeatmapSetID", "Metadata");
            BM_Sections.Add("HPDrainRate,CircleSize,OverallDifficulty,ApproachRate,SliderMultiplier,SliderTickRate", "Difficulty");

            if (beatmapFile != "")
            {
                if (File.Exists(beatmapFile))
                {
                    Parse(beatmapFile);
                }
            }
        }

        private void Parse(string bm)
        {
            Info.Filename = bm;
            Info.BeatmapHash = MD5FromFile(bm);
            using (StreamReader sR = new StreamReader(bm))
            {
                string currentSection = "";

                while (sR.Peek() != -1)
                {
                    string line = sR.ReadLine();

                    //Check for blank lines
                    if ((line == "") || (line.Length < 2))
                        continue;

                    //Check for section tag
                    if (line.Substring(0, 1) == "[")
                    {
                        currentSection = line;
                        continue;
                    }

                    //Check for commented-out line
                    if (line.Substring(0, 2) == "//")
                        continue;

                    //Check for version string
                    if (line.Length > 17)
                    {
                        if (line.Substring(0, 17) == "osu file format v")
                        {
                            Info.Format = Convert.ToInt32(line.Substring(17).Replace(Environment.NewLine, "").Replace(" ", ""));
                        }
                    }

                    //Do work for [General], [Metadata], [Difficulty] and [Editor] sections
                    if ((currentSection == "[General]") || (currentSection == "[Metadata]") || (currentSection == "[Difficulty]") || (currentSection == "[Editor]"))
                    {
                        string[] reSplit = line.Split(':');
                        string cProperty = reSplit[0];

                        //Check for blank value
                        string cValue = line.Length == reSplit[0].Length ? "" : reSplit[1].Trim();

                        //Import properties into Info
                        switch (cProperty)
                        {
                            case "EditorBookmarks":
                            {
                                string[] marks = cValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string m in marks.Where(m => m != ""))
                                {
                                    Info.EditorBookmarks.Add(Convert.ToInt32(m));
                                }
                            }
                                break;
                            case "Bookmarks":
                            {
                                string[] marks = cValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string m in marks.Where(m => m != ""))
                                {
                                    Info.Bookmarks.Add(Convert.ToInt32(m));
                                }
                            }
                                break;
                            case "Tags":
                                string[] tags = cValue.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string t in tags)
                                    Info.Tags.Add(t);
                                break;
                            case "Mode":
                                Info.Mode = (GameMode)Enum.Parse(typeof(GameMode), cValue);
                                break;
                            case "OverlayPosition":
                                Info.OverlayPosition = (OverlayOptions)Enum.Parse(typeof(OverlayOptions), cValue);
                                break;
                            case "AlwaysShowPlayfield":
                                Info.AlwaysShowPlayfield = Convert.ToBoolean(Convert.ToInt32(cValue));
                                break;
                            default:
                                FieldInfo fi = Info.GetType().GetField(cProperty);
                                PropertyInfo pi = Info.GetType().GetProperty(cProperty);
                                if (fi != null)
                                {
                                    if ((fi.FieldType == typeof(double?)) || (fi.FieldType == typeof(double)))
                                        fi.SetValue(Info, Convert.ToDouble(cValue));
                                    else if ((fi.FieldType == typeof(int?)) || (fi.FieldType == typeof(int)))
                                        fi.SetValue(Info, Convert.ToInt32(cValue));
                                    else if (fi.FieldType == typeof(string))
                                        fi.SetValue(Info, cValue);
                                    break;
                                }
                                if ((pi.PropertyType == typeof(double?)) || (pi.PropertyType == typeof(double)))
                                    pi.SetValue(Info, Convert.ToDouble(cValue), null);
                                else if ((pi.PropertyType == typeof(int?)) || (pi.PropertyType == typeof(int)))
                                    pi.SetValue(Info, Convert.ToInt32(cValue), null);
                                else if (pi.PropertyType == typeof(string))
                                    pi.SetValue(Info, cValue, null);
                                break;
                        }
                        continue;
                    }

                    //The following are version-dependent, the version is stored as a numeric value inside Info.Format
                    //Do work for [Events] section
                    if (currentSection == "[Events]")
                    {
                        switch (Info.Format)
                        {
                            case 3: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13:
                                string[] reSplit = line.Split(',');
                                if (reSplit[0] == "0")
                                {
                                    BackgroundInfo tempEvent = new BackgroundInfo();
                                    tempEvent.StartTime = Convert.ToInt32(reSplit[1]);
                                    tempEvent.Filename = reSplit[2].Replace("\"", "");
                                    Info.Events.Add(tempEvent);
                                }
                                else if ((reSplit[0] == "1") || (reSplit[0].ToLower() == "video"))
                                {
                                    VideoInfo tempEvent = new VideoInfo();
                                    tempEvent.StartTime = Convert.ToInt32(reSplit[1]);
                                    tempEvent.Filename = reSplit[2];
                                    Info.Events.Add(tempEvent);
                                }
                                else if (reSplit[0] == "2")
                                {
                                    BreakInfo tempEvent = new BreakInfo();
                                    tempEvent.StartTime = Convert.ToInt32(reSplit[1]);
                                    tempEvent.EndTime = Convert.ToInt32(reSplit[2]);
                                    Info.Events.Add(tempEvent);
                                }
                                else if (reSplit[0] == "3")
                                {
                                    ColourInfo tempEvent = new ColourInfo();
                                    tempEvent.StartTime = Convert.ToInt32(reSplit[1]);
                                    tempEvent.R = Convert.ToInt32(reSplit[2]);
                                    tempEvent.G = Convert.ToInt32(reSplit[3]);
                                    tempEvent.B = Convert.ToInt32(reSplit[4]);
                                    Info.Events.Add(tempEvent);
                                }
                                break;
                        }

                    }

                    //Do work for [TimingPoints] section
                    if (currentSection == "[TimingPoints]")
                    {
                        TimingPointInfo tempTimingPoint = new TimingPointInfo();
                        string[] reSplit = line.Split(',');
                        switch (Info.Format)
                        {
                            case 3:
                                tempTimingPoint.Time = Convert.ToDouble(reSplit[0]);
                                tempTimingPoint.BpmDelay = Convert.ToDouble(reSplit[1]);
                                Info.TimingPoints.Add(tempTimingPoint);
                                break;
                            case 4:
                                switch (reSplit.Length)
                                {
                                    case 4:
                                        tempTimingPoint.Time = Convert.ToDouble(reSplit[0]);
                                        tempTimingPoint.BpmDelay = Convert.ToDouble(reSplit[1]);
                                        tempTimingPoint.TimeSignature = Convert.ToInt32(reSplit[2]);
                                        tempTimingPoint.SampleSet = Convert.ToInt32(reSplit[3]);
                                        tempTimingPoint.CustomSampleSet = Convert.ToInt32(reSplit[4]);
                                        Info.TimingPoints.Add(tempTimingPoint);
                                        break;
                                    case 5:
                                        tempTimingPoint.Time = Convert.ToDouble(reSplit[0]);
                                        tempTimingPoint.BpmDelay = Convert.ToDouble(reSplit[1]);
                                        tempTimingPoint.TimeSignature = Convert.ToInt32(reSplit[2]);
                                        tempTimingPoint.SampleSet = Convert.ToInt32(reSplit[3]);
                                        tempTimingPoint.CustomSampleSet = Convert.ToInt32(reSplit[4]);
                                        Info.TimingPoints.Add(tempTimingPoint);
                                        break;
                                }

                                break;
                            case 5:
                                switch (reSplit.Length)
                                {
                                    case 6:
                                        tempTimingPoint.Time = Convert.ToDouble(reSplit[0]);
                                        tempTimingPoint.BpmDelay = Convert.ToDouble(reSplit[1]);
                                        tempTimingPoint.TimeSignature = Convert.ToInt32(reSplit[2]);
                                        tempTimingPoint.SampleSet = Convert.ToInt32(reSplit[3]);
                                        tempTimingPoint.CustomSampleSet = Convert.ToInt32(reSplit[4]);
                                        tempTimingPoint.VolumePercentage = Convert.ToInt32(reSplit[5]);
                                        Info.TimingPoints.Add(tempTimingPoint);
                                        break;
                                    case 7:
                                        tempTimingPoint.Time = Convert.ToDouble(reSplit[0]);
                                        tempTimingPoint.BpmDelay = Convert.ToDouble(reSplit[1]);
                                        tempTimingPoint.TimeSignature = Convert.ToInt32(reSplit[2]);
                                        tempTimingPoint.SampleSet = Convert.ToInt32(reSplit[3]);
                                        tempTimingPoint.CustomSampleSet = Convert.ToInt32(reSplit[4]);
                                        tempTimingPoint.VolumePercentage = Convert.ToInt32(reSplit[5]);
                                        tempTimingPoint.InheritsBPM = !Convert.ToBoolean(Convert.ToInt32(reSplit[6]));
                                        Info.TimingPoints.Add(tempTimingPoint);
                                        break;
                                    case 8:
                                        tempTimingPoint.Time = Convert.ToDouble(reSplit[0]);
                                        tempTimingPoint.BpmDelay = Convert.ToDouble(reSplit[1]);
                                        tempTimingPoint.TimeSignature = Convert.ToInt32(reSplit[2]);
                                        tempTimingPoint.SampleSet = Convert.ToInt32(reSplit[3]);
                                        tempTimingPoint.CustomSampleSet = Convert.ToInt32(reSplit[4]);
                                        tempTimingPoint.VolumePercentage = Convert.ToInt32(reSplit[5]);
                                        tempTimingPoint.InheritsBPM = !Convert.ToBoolean(Convert.ToInt32(reSplit[6]));
                                        switch (reSplit[7])
                                        {
                                            case "1":
                                                tempTimingPoint.KiaiTime = true;
                                                break;
                                            case "8":
                                                tempTimingPoint.OmitFirstBarLine = true;
                                                break;
                                            case "9":
                                                tempTimingPoint.KiaiTime = true;
                                                tempTimingPoint.OmitFirstBarLine = true;
                                                break;
                                        }
                                        Info.TimingPoints.Add(tempTimingPoint);
                                        break;
                                }
                                break;
                            case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13:
                                tempTimingPoint.Time = Convert.ToDouble(reSplit[0]);
                                tempTimingPoint.BpmDelay = Convert.ToDouble(reSplit[1]);
                                tempTimingPoint.TimeSignature = Convert.ToInt32(reSplit[2]);
                                tempTimingPoint.SampleSet = Convert.ToInt32(reSplit[3]);
                                tempTimingPoint.CustomSampleSet = Convert.ToInt32(reSplit[4]);
                                tempTimingPoint.VolumePercentage = Convert.ToInt32(reSplit[5]);
                                tempTimingPoint.InheritsBPM = !Convert.ToBoolean(Convert.ToInt32(reSplit[6]));
                                switch (reSplit[7])
                                {
                                    case "1":
                                        tempTimingPoint.KiaiTime = true;
                                        break;
                                    case "8":
                                        tempTimingPoint.OmitFirstBarLine = true;
                                        break;
                                    case "9":
                                        tempTimingPoint.KiaiTime = true;
                                        tempTimingPoint.OmitFirstBarLine = true;
                                        break;
                                }
                                Info.TimingPoints.Add(tempTimingPoint);
                                break;
                        }
                    }

                    //Do work for [Colours] section
                    if (currentSection == "[Colours]")
                    {
                        switch (Info.Format)
                        {
                            case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13:
                                if (line.Substring(0, line.IndexOf(':', 1)).Trim() == "SliderBorder")
                                {
                                    string value = line.Substring(line.IndexOf(':', 1) + 1).Trim();
                                    string[] reSplit = value.Split(',');
                                    Info.SliderBorder = new ColourInfo();
                                    Info.SliderBorder.R = Convert.ToInt32(reSplit[0]);
                                    Info.SliderBorder.G = Convert.ToInt32(reSplit[1]);
                                    Info.SliderBorder.B = Convert.ToInt32(reSplit[2]);
                                }
                                else if (line.Substring(0, 5) == "Combo")
                                {
                                    string value = line.Substring(line.IndexOf(':', 1) + 1).Trim();
                                    string[] reSplit = value.Split(',');
                                    ComboInfo tempCombo = new ComboInfo();
                                    tempCombo.ComboNumber = Convert.ToInt32(line.Substring(5, 1));
                                    tempCombo.Colour.R = Convert.ToInt32(reSplit[0]);
                                    tempCombo.Colour.G = Convert.ToInt32(reSplit[1]);
                                    tempCombo.Colour.B = Convert.ToInt32(reSplit[2]);
                                    Info.ComboColours.Add(tempCombo);
                                }
                                break;
                        }
                    }

                    //Do work for [HitObjects] section
                    if (currentSection == "[HitObjects]")
                    {
                        string[] reSplit = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        switch (Info.Format)
                        {
                            case 3: case 4:
                                if (reSplit.Length == 5)
                                {
                                    //Circle
                                    BaseCircle tempCircle = new BaseCircle();
                                    tempCircle.Radius = 40 - 4 * (Info.CircleSize - 2);
                                    tempCircle.Location.X = Convert.ToInt32(reSplit[0]);
                                    tempCircle.Location.Y = Convert.ToInt32(reSplit[1]);
                                    tempCircle.StartTime = Convert.ToInt32(reSplit[2]);
                                    int tempNewCombo = Convert.ToInt32(reSplit[3]);
                                    if (tempNewCombo == 1)
                                    {
                                        tempCircle.NewCombo = false;
                                    }
                                    else
                                    {
                                        tempCircle.NewCombo = (tempNewCombo + 1) % 2 == 0;
                                    }
                                    tempCircle.Effect = (EffectType)Enum.Parse(typeof(EffectType), reSplit[4]);
                                    Info.HitObjects.Add(tempCircle);
                                }
                                else if (reSplit[5].Substring(0, 1) == "B" || reSplit[5].Substring(0, 1) == "C" || reSplit[5].Substring(0, 1) == "L" || reSplit[5].Substring(0, 1) == "P")
                                {
                                    //Slider
                                    SliderInfo tempSlider = new SliderInfo();
                                    tempSlider.Velocity = Info.SliderMultiplier;
                                    tempSlider.Radius = 40 - 4 * (Info.CircleSize - 2);
                                    tempSlider.Location.X = Convert.ToInt32(reSplit[0]);
                                    tempSlider.Location.Y = Convert.ToInt32(reSplit[1]);
                                    tempSlider.StartTime = Convert.ToInt32(reSplit[2]);
                                    int tempNewCombo = Convert.ToInt32(reSplit[3]);
                                    tempSlider.NewCombo = tempNewCombo != 2 && tempNewCombo % 2 == 0;
                                    tempSlider.Effect = (EffectType)Enum.Parse(typeof(EffectType), reSplit[4]);
                                    switch (reSplit[5].Substring(0, 1))
                                    {
                                        case "B":
                                            tempSlider.Type = SliderType.Bezier;
                                            break;
                                        case "C":
                                            tempSlider.Type = SliderType.CSpline;
                                            break;
                                        case "L":
                                            tempSlider.Type = SliderType.Linear;
                                            break;
                                        case "P":
                                            tempSlider.Type = SliderType.PSpline;
                                            break;
                                    }
                                    string[] pts = reSplit[5].Split(new[] { "|" }, StringSplitOptions.None);
                                    for (int i = 1; i <= pts.Length - 1; i++)
                                    {
                                        PointInfo p = new PointInfo(Convert.ToDouble(pts[i].Substring(0, pts[i].IndexOf(":", StringComparison.Ordinal))), Convert.ToDouble(pts[i].Substring(pts[i].IndexOf(":", StringComparison.Ordinal) + 1)));
                                        tempSlider.Points.Add(p);
                                    }
                                    tempSlider.RepeatCount = Convert.ToInt32(reSplit[6]);
                                    double tempDbl;
                                    if (double.TryParse(reSplit[7], out tempDbl))
                                    {
                                        tempSlider.MaxPoints = tempDbl;
                                    }
                                    Info.HitObjects.Add(tempSlider);
                                }
                                else
                                {
                                    //Spinner
                                    SpinnerInfo tempSpinner = new SpinnerInfo();
                                    tempSpinner.Location.X = Convert.ToInt32(reSplit[0]);
                                    tempSpinner.Location.Y = Convert.ToInt32(reSplit[1]);
                                    tempSpinner.StartTime = Convert.ToInt32(reSplit[2]);
                                    tempSpinner.Effect = (EffectType)Enum.Parse(typeof(EffectType), reSplit[4]);
                                    tempSpinner.EndTime = Convert.ToInt32(reSplit[5]);
                                    Info.HitObjects.Add(tempSpinner);
                                }
                                break;
                            //Note: Until I figure out how to represent the last few bytes, I will ignore them
                            case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13:
                                int circleCount = 5;
                                if (reSplit[reSplit.Length - 1].Contains(':'))
                                    circleCount = 6;
                                if (reSplit.Length == circleCount)
                                {
                                    //Circle
                                    BaseCircle tempCircle = new BaseCircle();
                                    tempCircle.Radius = 40 - 4 * (Info.CircleSize - 2);
                                    tempCircle.Location.X = Convert.ToInt32(reSplit[0]);
                                    tempCircle.Location.Y = Convert.ToInt32(reSplit[1]);
                                    tempCircle.StartTime = Convert.ToInt32(reSplit[2]);
                                    int tempNewCombo = Convert.ToInt32(reSplit[3]);
                                    if (tempNewCombo == 1)
                                    {
                                        tempCircle.NewCombo = false;
                                    }
                                    else
                                    {
                                        tempCircle.NewCombo = (tempNewCombo + 1) % 2 == 0;
                                    }

                                    tempCircle.Effect = (EffectType)Enum.Parse(typeof(EffectType), reSplit[4]);
                                    Info.HitObjects.Add(tempCircle);
                                }
                                else if ((reSplit[5].Substring(0, 1) == "B") || (reSplit[5].Substring(0, 1) == "C") || (reSplit[5].Substring(0, 1) == "L") || (reSplit[5].Substring(0, 1) == "P"))
                                {
                                    //Slider
                                    SliderInfo tempSlider = new SliderInfo();
                                    tempSlider.Velocity = Info.SliderMultiplier;
                                    tempSlider.Radius = 40 - 4 * (Info.CircleSize - 2);
                                    tempSlider.Location.X = Convert.ToInt32(reSplit[0]);
                                    tempSlider.Location.Y = Convert.ToInt32(reSplit[1]);
                                    tempSlider.StartTime = Convert.ToInt32(reSplit[2]);
                                    int tempNewCombo = Convert.ToInt32(reSplit[3]);
                                    tempSlider.NewCombo = tempNewCombo != 2 && tempNewCombo % 2 == 0;
                                    tempSlider.Effect = (EffectType)Enum.Parse(typeof(EffectType), reSplit[4]);
                                    switch (reSplit[5].Substring(0, 1))
                                    {
                                        case "B":
                                            tempSlider.Type = SliderType.Bezier;
                                            break;
                                        case "C":
                                            tempSlider.Type = SliderType.CSpline;
                                            break;
                                        case "L":
                                            tempSlider.Type = SliderType.Linear;
                                            break;
                                        case "P":
                                            tempSlider.Type = SliderType.PSpline;
                                            break;
                                    }
                                    string[] pts = reSplit[5].Split(new[] { "|" }, StringSplitOptions.None);
                                    tempSlider.Points.Add(tempSlider.Location);
                                    for (int i = 1; i <= pts.Length - 1; i++)
                                    {
                                        PointInfo p = new PointInfo(Convert.ToDouble(pts[i].Substring(0, pts[i].IndexOf(":", StringComparison.Ordinal))), Convert.ToDouble(pts[i].Substring(pts[i].IndexOf(":", StringComparison.Ordinal) + 1)));
                                        tempSlider.Points.Add(p);
                                    }
                                    tempSlider.RepeatCount = Convert.ToInt32(reSplit[6]);
                                    double tempDbl;
                                    if (double.TryParse(reSplit[7], out tempDbl))
                                    {
                                        tempSlider.MaxPoints = tempDbl;
                                    }
                                    Info.HitObjects.Add(tempSlider);
                                }
                                else
                                {
                                    //Spinner
                                    SpinnerInfo tempSpinner = new SpinnerInfo();
                                    tempSpinner.Location.X = Convert.ToInt32(reSplit[0]);
                                    tempSpinner.Location.Y = Convert.ToInt32(reSplit[1]);
                                    tempSpinner.StartTime = Convert.ToInt32(reSplit[2]);
                                    tempSpinner.Effect = (EffectType)Enum.Parse(typeof(EffectType), reSplit[4]);
                                    tempSpinner.EndTime = Convert.ToInt32(reSplit[5]);
                                    Info.HitObjects.Add(tempSpinner);
                                }
                                break;
                        }
                    }
                }
            }
            foreach (FieldInfo fi in Info.GetType().GetFields())
            {
                FieldInfo ff = GetType().GetField(fi.Name);
                ff.SetValue(this, fi.GetValue(Info));
            }
            foreach (PropertyInfo pi in Info.GetType().GetProperties())
            {
                PropertyInfo ff = GetType().GetProperty(pi.Name);
                ff.SetValue(this, pi.GetValue(Info, null), null);
            }
        }

        /// <summary>
        /// Saves the beatmap
        /// </summary>
        /// <param name="filename">The file to save the beatmap as</param>
        public void Save(string filename)
        {
            CultureInfo lastCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
            Save("", "osu file format v" + Info.Format);
            FieldInfo[] newFields = GetType().GetFields();
            FieldInfo[] oldFields = Info.GetType().GetFields();

            foreach (FieldInfo f1 in newFields)
            {
                foreach (FieldInfo f2 in oldFields.Where(f2 => f1.Name == f2.Name))
                {
                    switch (f1.Name)
                    {
                        case "EditorBookmarks":
                            {
                                List<int> temps = (List<int>)f1.GetValue(this);
                                if (temps.Count != 0)
                                {
                                    Save("General", "EditorBookmarks:" + string.Join(",", temps.Select(t => t.ToString(CultureInfo.InvariantCulture)).ToArray()));
                                }
                            }
                            break;
                        case "Bookmarks":
                            {
                                List<int> temps = (List<int>)f1.GetValue(this);
                                if (temps.Count != 0)
                                {
                                    Save("Editor", "Bookmarks:" + string.Join(",", temps.Select(t => t.ToString(CultureInfo.InvariantCulture)).ToArray()));
                                }
                            }
                            break;
                        case "Tags":
                            {
                                List<string> temps = (List<string>)f1.GetValue(this);
                                if (temps.Count != 0)
                                {
                                    Save("Metadata", "Tags:" + string.Join(" ", temps.ToArray()));
                                }
                            }
                            break;
                        case "Events":
                            if ((Info.Format >= 3) && (Info.Format <= 13))
                            {
                                foreach (BaseEvent o in (IEnumerable<BaseEvent>)f1.GetValue(this))
                                {
                                    if (o.GetType() == typeof(BackgroundInfo))
                                    {
                                        BackgroundInfo backgroundInfo = (BackgroundInfo)o;
                                        Save("Events", "0," + o.StartTime + ",\"" + backgroundInfo.Filename + "\"");
                                    }
                                    else if (o.GetType() == typeof(VideoInfo))
                                    {
                                        VideoInfo videoInfo = (VideoInfo)o;
                                        Save("Events", "1," + o.StartTime + ",\"" + videoInfo.Filename + "\"");
                                    }
                                    else if (o.GetType() == typeof(BreakInfo))
                                    {
                                        BreakInfo breakInfo = (BreakInfo)o;
                                        Save("Events", "2," + o.StartTime + "," + breakInfo.EndTime);
                                    }
                                    else if (o.GetType() == typeof(ColourInfo))
                                    {
                                        ColourInfo colourInfo = (ColourInfo)o;
                                        Save("Events", "3," + o.StartTime + "," + colourInfo.R + "," + colourInfo.G + "," + colourInfo.B);
                                    }
                                }
                            }
                            break;
                        case "TimingPoints":
                            switch (Info.Format)
                            {
                                case 3:
                                    foreach (TimingPointInfo o in (IEnumerable<TimingPointInfo>)f1.GetValue(this))
                                        Save("TimingPoints", o.Time + "," + o.BpmDelay);
                                    break;
                                case 4:
                                    foreach (TimingPointInfo o in (IEnumerable<TimingPointInfo>)f1.GetValue(this))
                                        Save("TimingPoints", o.Time + "," + o.BpmDelay + "," + o.TimeSignature + "," + o.SampleSet + "," + o.CustomSampleSet);
                                    break;
                                case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13:
                                    foreach (TimingPointInfo o in (IEnumerable<TimingPointInfo>)f1.GetValue(this))
                                    {
                                        int options = 0;
                                        if (o.KiaiTime)
                                            options += 1;
                                        if (o.OmitFirstBarLine)
                                            options += 8;
                                        Save("TimingPoints", o.Time + "," + o.BpmDelay + "," + o.TimeSignature + "," + o.SampleSet + "," + o.CustomSampleSet + "," + o.VolumePercentage + "," + Convert.ToInt32(!o.InheritsBPM) + "," + options);
                                    }
                                    break;
                            }
                            break;
                        case "ComboColours":
                            if ((Info.Format >= 5) && (Info.Format <= 13))
                            {
                                foreach (ComboInfo o in (IEnumerable<ComboInfo>)f1.GetValue(this))
                                    Save("Colours", "Combo" + o.ComboNumber + ':' + o.Colour.R + "," + o.Colour.G + "," + o.Colour.B);
                            }
                            break;
                        case "SliderBorder":
                            if ((Info.Format >= 5) && (Info.Format <= 13))
                            {
                                ColourInfo o = (ColourInfo)f1.GetValue(this);
                                Save("Colours", "SliderBorder: " + o.R + "," + o.G + "," + o.B);
                            }
                            break;
                        case "HitObjects":
                            switch (Info.Format)
                            {
                                case 3: case 4:
                                    foreach (BaseCircle obj in (IEnumerable<BaseCircle>)f1.GetValue(this))
                                    {
                                        int combo = 5;
                                        if (obj.NewCombo == false)
                                            combo = 1;
                                        if (obj.GetType() == typeof(BaseCircle))
                                        {
                                            Save("HitObjects", obj.Location.X + "," + obj.Location.Y + "," + obj.StartTime + "," + combo + "," + (int)obj.Effect + ",");
                                        }
                                        else if (obj.GetType() == typeof(SliderInfo))
                                        {
                                            SliderInfo sliderInfo = (SliderInfo)obj;
                                            string pointString = sliderInfo.Points.Aggregate("", (current, p) => current + ("|" + p.X + ':' + p.Y));
                                            Save("HitObjects", obj.Location.X + "," + obj.Location.Y + "," + obj.StartTime + "," + (combo + 1) + "," + (int)obj.Effect + "," + sliderInfo.Type.ToString().Substring(0, 1) + pointString + "," + sliderInfo.RepeatCount + "," + sliderInfo.MaxPoints + ",");
                                        }
                                        else if (obj.GetType() == typeof(SpinnerInfo))
                                        {
                                            SpinnerInfo spinnerInfo = (SpinnerInfo)obj;
                                            Save("HitObjects", obj.Location.X + "," + obj.Location.Y + "," + obj.StartTime + ",12," + (int)obj.Effect + "," + spinnerInfo.EndTime + ",");
                                        }
                                    }
                                    break;
                                case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13:
                                    foreach (BaseCircle obj in (IEnumerable<BaseCircle>)f1.GetValue(this))
                                    {
                                        int combo = 5;
                                        if (obj.NewCombo == false)
                                            combo = 1;
                                        if (obj.GetType() == typeof(BaseCircle))
                                        {
                                            Save("HitObjects", obj.Location.X + "," + obj.Location.Y + "," + obj.StartTime + "," + combo + "," + (int)obj.Effect);
                                        }
                                        else if (obj.GetType() == typeof(SliderInfo))
                                        {
                                            SliderInfo sliderInfo = (SliderInfo)obj;
                                            string pointString = sliderInfo.Points.Aggregate("", (current, p) => current + ("|" + p.X + ':' + p.Y));
                                            Save("HitObjects", obj.Location.X + "," + obj.Location.Y + "," + obj.StartTime + "," + (combo + 1) + "," + (int)obj.Effect + "," + sliderInfo.Type.ToString().Substring(0, 1) + pointString + "," + sliderInfo.RepeatCount + "," + sliderInfo.MaxPoints);
                                        }
                                        else if (obj.GetType() == typeof(SpinnerInfo))
                                        {
                                            SpinnerInfo spinnerInfo = (SpinnerInfo)obj;
                                            Save("HitObjects", obj.Location.X + "," + obj.Location.Y + "," + obj.StartTime + ",12," + (int)obj.Effect + "," + spinnerInfo.EndTime);
                                        }
                                    }
                                    break;
                            }
                            break;
                        default:
                            if (f1.Name != "Format" && f1.Name != "Filename" && f1.Name != "BeatmapHash")
                            {
                                if (f1.GetValue(this) != null)
                                {
                                    if (f2.GetValue(Info) != null)
                                    {
                                        if ((f1.GetValue(this).GetType() == typeof(GameMode)) || (f1.GetValue(this).GetType() == typeof(OverlayOptions)))
                                            Save(GetSection(f1.Name), f1.Name + ':' + (int)f1.GetValue(this));
                                        else
                                            Save(GetSection(f1.Name), f1.Name + ':' + f1.GetValue(this));
                                    }
                                    else
                                    {
                                        if ((f2.GetValue(Info).GetType() == typeof(GameMode)) || (f2.GetValue(Info).GetType() == typeof(OverlayOptions)))
                                            Save(GetSection(f2.Name), f2.Name + ':' + (int)f2.GetValue(Info));
                                        else
                                            Save(GetSection(f2.Name), f2.Name + ':' + f2.GetValue(Info));
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            FinishSave(filename);
            Thread.CurrentThread.CurrentCulture = lastCulture;
        }

        private void Save(string section, string contents)
        {
            if (section == "")
                WriteBuffer.Add(contents);
            else if (WriteBuffer.Contains("[" + section + "]") == false)
            {
                WriteBuffer.Add("");
                WriteBuffer.Add("[" + section + "]");
                WriteBuffer.Add(contents);
                SectionLength.Add(section, 1);
            }
            else
            {
                if (WriteBuffer.IndexOf("[" + section + "]") + SectionLength[section] == WriteBuffer.Count)
                {
                    WriteBuffer.Add(contents);
                    SectionLength[section] += 1;
                }
                else
                {
                    WriteBuffer.Insert(WriteBuffer.IndexOf("[" + section + "]") + SectionLength[section] + 1, contents);
                    SectionLength[section] += 1;
                }
            }
        }

        private void FinishSave(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                foreach (string l in WriteBuffer)
                {
                    sw.WriteLine(l);
                }
            }
        }

        private string GetSection(string name)
        {
            foreach (string k in BM_Sections.Keys.Where(k => k.Contains(name)))
            {
                return BM_Sections[k];
            }
            return "";
        }

        private string MD5FromFile(string fileName)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}
