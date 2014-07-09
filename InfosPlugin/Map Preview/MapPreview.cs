using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using BMAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using o_RAResources;
using ReplayAPI;

namespace InfosPlugin
{
    public partial class MapPreview : System.Windows.Forms.UserControl
    {

        #region Load Events
        public MapPreview()
        {
            InitializeComponent();
        }
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) { }
        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e) { }
        private void MapPreview_Load(object sender, EventArgs e)
        {
            //Load our graphics device
            PresentationParameters pp = new PresentationParameters();
            pp.BackBufferHeight = Height;
            pp.BackBufferWidth = Width;
            pp.IsFullScreen = false;
            pp.DeviceWindowHandle = Handle;
            device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, pp);

            Initialize();
            LoadContent();
            System.Windows.Forms.Application.Idle += Application_Idle;
            oRA.Data.ReplayChanged += HandleReplayChanged;
        }
        private void MapPreview_Resize(object sender, EventArgs e)
        {
            //Resize our graphics device accordingly
            if (ClientSize.Height <= 0 || ClientSize.Width <= 0)
                return;
            PresentationParameters pp = device.PresentationParameters;
            pp.BackBufferHeight = ClientSize.Height;
            pp.BackBufferWidth = ClientSize.Width;

            device.Reset(pp);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Update();
            Draw();
        }
        #endregion


        private MouseState LastMouseState;
        private GraphicsDevice device;
        private SpriteBatch sb;

        const float ButtonScale = 0.5f;
        const float ButtonSize = 16; //px
        const int ButtonSpacing = 4;

        private Beatmap CurrentBeatmap;

        private Texture2D BackgroundTexture;
        private Texture2D FollowPointTexture;
        private Texture2D HitCircleTexture;
        private Texture2D HitCircleOverlayTexture;

        private GraphicsHelper.TexturedButton PlayerPlayPauseButton;
        private GraphicsHelper.TexturedButton PlayerGotoStartButton;
        private GraphicsHelper.TexturedButton PlayerGotoEndButton;

        private bool Playing;

        private int PlayerPosition;
        private int TotalBeatmapTime;
        private int BeatmapApproachRate;

        private void HandleReplayChanged(Replay r, Beatmap b)
        {
            CurrentBeatmap = b;
            LoadContent();
        }

        /// <summary>
        /// One-time initializations.
        /// </summary>
        private void Initialize()
        {
            //Spritebatch initialization
            sb = new SpriteBatch(device);


            PlayerPlayPauseButton = new GraphicsHelper.TexturedButton(this, Texture2D.FromStream(device, ResourceHelper.GetResourceStream("Player_Play.png")), Texture2D.FromStream(device, ResourceHelper.GetResourceStream("Player_Pause.png")));
            PlayerGotoStartButton = new GraphicsHelper.TexturedButton(this, Texture2D.FromStream(device, ResourceHelper.GetResourceStream("Player_Start.png")));
            PlayerGotoEndButton = new GraphicsHelper.TexturedButton(this, Texture2D.FromStream(device, ResourceHelper.GetResourceStream("Player_End.png")));
        }

        /// <summary>
        /// Load sprites here.
        /// </summary>
        private void LoadContent()
        {
            if (CurrentBeatmap != null)
            {
                PlayerPosition = 0;
                TotalBeatmapTime = 0;
                Playing = false;

                //Load the background image
                string beatmapLoc = CurrentBeatmap.Filename.Substring(0, CurrentBeatmap.Filename.LastIndexOf(@"\", StringComparison.InvariantCulture));
                foreach (Event_Base ev in CurrentBeatmap.Events)
                {
                    if (ev.GetType() == typeof(Event_Background))
                    {
                        string filePath = Path.Combine(beatmapLoc, ((Event_Background)ev).Filename);
                        if (File.Exists(filePath))
                        {
                            //Set the new texture
                            BackgroundTexture = Texture2D.FromStream(device, new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        }
                        else
                        {
                            //Release resources and null the object
                            //So that previous background is not drawn on next Draw()
                            if (BackgroundTexture != null)
                            {
                                BackgroundTexture.Dispose();
                                BackgroundTexture = null;                                
                            }
                        }                        
                    }
                }

                //Load the skin sprites
                //(followpoints, hitcircle, etc)
                string followPointFile = Path.Combine(beatmapLoc, "followpoint.png");
                string hitCircleFile = Path.Combine(beatmapLoc, "hitcircle.png");
                string hitCircleOverlayFile = Path.Combine(beatmapLoc, "hitcircleoverlay.png");
                //We're not using the default skin hence
                //We want to use the skin textures inside o!RAResources 
                //if the following occurs
                if (File.Exists(followPointFile))
                    FollowPointTexture = Texture2D.FromStream(device, new FileStream(followPointFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                else
                    FollowPointTexture = Texture2D.FromStream(device, ResourceHelper.GetResourceStream("default_followpoint.png"));

                if (File.Exists(hitCircleFile))
                    HitCircleTexture = Texture2D.FromStream(device, new FileStream(hitCircleFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                else
                    HitCircleTexture = Texture2D.FromStream(device, ResourceHelper.GetResourceStream("default_hitcircle.png"));

                if (File.Exists(hitCircleOverlayFile))
                    HitCircleOverlayTexture = Texture2D.FromStream(device, new FileStream(hitCircleOverlayFile, FileMode.Open, FileAccess.Read, FileShare.Read));
                else
                    HitCircleOverlayTexture = Texture2D.FromStream(device, ResourceHelper.GetResourceStream("default_hitcircleoverlay.png"));

                //Load the beatmap
                
                //Linearly interpolate approachrate timings
                //AR0: 1800ms
                //AR5: 1200ms
                //AR10: 450ms
                BeatmapApproachRate = CurrentBeatmap.ApproachRate < 5 ? (int)(1800 + CurrentBeatmap.ApproachRate * (1200 - 1800) / 5) : (int)(1200 + (CurrentBeatmap.ApproachRate - 5) * (450 - 1200) / 5);

                HitObject_Circle endObject = CurrentBeatmap.HitObjects[CurrentBeatmap.HitObjects.Count - 1];
                if (endObject.GetType() == typeof(HitObject_Spinner))
                    TotalBeatmapTime = ((HitObject_Spinner)endObject).EndTime + BeatmapApproachRate;
                else
                    TotalBeatmapTime = endObject.StartTime + BeatmapApproachRate;
            }
        }
        
        /// <summary>
        /// Graphics updating thread (handle IO here).
        /// </summary>
        new private void Update()
        {
            MouseState state = Mouse.GetState();
            Point position = new Point(PointToClient(MousePosition).X, PointToClient(MousePosition).Y);

            #region Update Buttons
            //Update Play/Pause button
            if (PlayerPlayPauseButton.DisplayRectangle.Contains(position))
            {
                if (state.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released)
                {
                    if (PlayerPlayPauseButton.DisplayRectangle.Contains(position))
                    {
                        PlayerPlayPauseButton.Trigger();
                        Playing = !Playing;
                        Thread t = new Thread(IncrementPlayer);
                        t.IsBackground = true;
                        t.Start();
                    }
                }
                PlayerPlayPauseButton.Enter();
            }
            else
            {
                PlayerPlayPauseButton.Leave();
            }

            //Update GotoStart button
            if (PlayerGotoStartButton.DisplayRectangle.Contains(position))
            {
                if (state.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released)
                {
                    if (PlayerGotoStartButton.DisplayRectangle.Contains(position))
                    {
                        PlayerGotoStartButton.Trigger();
                        if (Playing)
                            PlayerPlayPauseButton.Trigger();
                        Playing = false;
                        PlayerPosition = 0;
                    }
                }
                PlayerGotoStartButton.Enter();
            }
            else
            {
                PlayerGotoStartButton.Leave();
            }

            //Update GotoEnd button
            if (PlayerGotoEndButton.DisplayRectangle.Contains(position))
            {
                if (state.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released)
                {
                    if (PlayerGotoEndButton.DisplayRectangle.Contains(position))
                    {
                        PlayerGotoEndButton.Trigger();
                        if (Playing)
                            PlayerPlayPauseButton.Trigger();
                        Playing = false;
                        PlayerPosition = TotalBeatmapTime;
                    }
                }
                PlayerGotoEndButton.Enter();
            }
            else
            {
                PlayerGotoEndButton.Leave();
            }
            #endregion
            
            LastMouseState = state;
        }

        private void IncrementPlayer()
        {
            while (Playing)
            {
                if (PlayerPosition < TotalBeatmapTime)
                    PlayerPosition += 1;
                else
                {
                    PlayerPlayPauseButton.Trigger();
                    Playing = false;
                    return;
                }
                Invalidate();
                GraphicsHelper.NOP(10000);
            }
        }

        /// <summary>
        /// Main graphics thread (handle drawing here).
        /// </summary>
        private void Draw()
        {
            device.Clear(new Color(43, 43, 43));

            //Arbitrary spacing at the bottom
            //Todo: Move these to Resize()
            float objectScaling = (ClientSize.Height - ButtonSize) / 768f;

            sb.Begin();
            /* */

            //Draw the beatmap background
            if (BackgroundTexture != null)
            {
                sb.Draw(BackgroundTexture, new Vector2(ClientSize.Width / 2f, 0), null, Color.White * 0.25f, 0, new Vector2(BackgroundTexture.Width / 2f, 0), (768f / BackgroundTexture.Height) * objectScaling, SpriteEffects.None, 0);             
            }

            //Draw play/pause/start/end buttons
            PlayerPlayPauseButton.Position = new Vector2(ClientSize.Width / 2f, ClientSize.Height - ButtonSize / 2);
            PlayerGotoStartButton.Position = new Vector2(ClientSize.Width / 2f - ButtonSpacing - ButtonSize, ClientSize.Height - ButtonSize / 2);
            PlayerGotoEndButton.Position = new Vector2(ClientSize.Width / 2f + ButtonSpacing + ButtonSize, ClientSize.Height - ButtonSize / 2);

            if (PlayerGotoStartButton.Texture != null)
                sb.Draw(PlayerGotoStartButton.Texture, PlayerGotoStartButton.Position, null, PlayerGotoStartButton.Color, 0, PlayerGotoStartButton.Origin, PlayerGotoStartButton.Scale, SpriteEffects.None, 1);
            if (PlayerPlayPauseButton.Texture != null)
                sb.Draw(PlayerPlayPauseButton.Texture, PlayerPlayPauseButton.Position, null, PlayerPlayPauseButton.Color, 0, PlayerPlayPauseButton.Origin, PlayerPlayPauseButton.Scale, SpriteEffects.None, 1);
            if (PlayerGotoEndButton.Texture != null)
                sb.Draw(PlayerGotoEndButton.Texture, PlayerGotoEndButton.Position, null, PlayerGotoEndButton.Color, 0, PlayerGotoEndButton.Origin, PlayerGotoEndButton.Scale, SpriteEffects.None, 1);


            if (CurrentBeatmap != null && CurrentBeatmap.HitObjects.Count != 0)
            {
                //Figure out the real play area size
                //(so that all hitobjects appear within backgrounded area)
                Rectangle playArea = new Rectangle((int)(ClientSize.Width / 2f - 512 * objectScaling + CurrentBeatmap.HitObjects[0].Radius * objectScaling), (int)(CurrentBeatmap.HitObjects[0].Radius * objectScaling),
                                                   (int)(1024 * objectScaling - 2 * CurrentBeatmap.HitObjects[0].Radius * objectScaling), (int)(768 * objectScaling - 2 * CurrentBeatmap.HitObjects[0].Radius * objectScaling));

                //Draw hitobjects
                foreach (HitObject_Circle obj in CurrentBeatmap.HitObjects)
                {
                    //Position transforming from osu! coords to real x-y
                    float xTransform = (playArea.Width - (float)obj.Radius) / 512f;
                    float yTransform = (playArea.Height - (float)obj.Radius) / 384f;
                    if (obj.GetType() == typeof(HitObject_Slider))
                    {

                    }
                    else if (obj.GetType() == typeof(HitObject_Spinner))
                    {

                    }
                    else
                    {
                        //200 ms delay after approach circle has hit
                        if (obj.StartTime - PlayerPosition < BeatmapApproachRate && PlayerPosition - obj.StartTime < 200)
                        {
                            //Origin is centre of hitcircle
                            sb.Draw(HitCircleTexture, new Rectangle((int)(playArea.X + obj.Location.X * xTransform), (int)(playArea.Y + obj.Location.Y * yTransform), (int)(2 * obj.Radius * objectScaling), (int)(2 * obj.Radius * objectScaling)), null, Color.Red, 0, new Vector2((float)obj.Radius / 2, (float)obj.Radius / 2), SpriteEffects.None, 0);
                            sb.Draw(HitCircleOverlayTexture, new Rectangle((int)(playArea.X + obj.Location.X * xTransform), (int)(playArea.Y + obj.Location.Y * yTransform), (int)(2 * obj.Radius * objectScaling), (int)(2 * obj.Radius * objectScaling)), null, Color.White, 0, new Vector2((float)obj.Radius / 2, (float)obj.Radius / 2), SpriteEffects.None, 0);
                        }
                    }
                }
            }
            

            /* */
            sb.End();
            if (device.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost || device.GraphicsDeviceStatus == GraphicsDeviceStatus.NotReset)
                device.Reset();
            device.Present();
        }
        
    }
}
