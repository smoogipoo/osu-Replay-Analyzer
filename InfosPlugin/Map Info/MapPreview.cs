using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using BMAPI;
using Microsoft.Xna.Framework.Graphics;
using o_RAResources;
using ReplayAPI;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace InfosPlugin
{
    public partial class MapPreview : UserControl
    {

        #region Load Events
        public MapPreview()
        {
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e) { }
        protected override void OnPaintBackground(PaintEventArgs e) { }
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
            Application.Idle += Application_Idle;
            oRA.Data.ReplayChanged += HandleReplayChanged;
        }
        private void MapPreview_Resize(object sender, EventArgs e)
        {
            //Resize our graphics device accordingly
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

        private GraphicsDevice device;
        private SpriteBatch sb;

        private Texture2D BeatmapBackground;
        private Beatmap CurrentBeatmap;

        private void HandleReplayChanged(Replay r, Beatmap b)
        {
            CurrentBeatmap = b;
            LoadContent();
        }


        /// <summary>
        /// One-time initializations.
        /// </summary>
        public void Initialize()
        {
            //Spritebatch initialization
            sb = new SpriteBatch(device);
        }

        /// <summary>
        /// Load sprites here.
        /// </summary>
        public void LoadContent()
        {
            if (CurrentBeatmap != null)
            {
                //Find background image
                foreach (BaseEvent ev in CurrentBeatmap.Events)
                {
                    if (ev.GetType() == typeof(BackgroundInfo))
                    {
                        string filePath = Path.Combine(CurrentBeatmap.Filename.Substring(0, CurrentBeatmap.Filename.LastIndexOf(@"\", StringComparison.InvariantCulture)), ((BackgroundInfo)ev).Filename);
                        if (File.Exists(filePath))
                        {
                            //Set the new texture
                            BeatmapBackground = Texture2D.FromStream(device, new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        }
                        else
                        {
                            //Release resources and null the object
                            //So that previous background is not drawn on next Draw()
                            if (BeatmapBackground != null)
                            {
                                BeatmapBackground.Dispose();
                                BeatmapBackground = null;                                
                            }
                        }
                         
                    }
                }
            }
        }
        
        /// <summary>
        /// Graphics updating thread (handle IO here).
        /// </summary>
        new public void Update()
        {

        }

        /// <summary>
        /// Main graphics thread (handle drawing here).
        /// </summary>
        public void Draw()
        {
            device.Clear(new Color(43, 43, 43));
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            /* */

            //Draw the beatmap background
            if (BeatmapBackground != null)
                sb.Draw(BeatmapBackground, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height), Color.White);



            /* */
            sb.End();
            if (device.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost || device.GraphicsDeviceStatus == GraphicsDeviceStatus.NotReset)
            {
                device.Reset();
            }
            device.Present();
        }
        
    }
}
