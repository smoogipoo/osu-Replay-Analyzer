using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MapPreview
{
    public static class GraphicsHelper
    {
        public static void NOP(long ticks)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedTicks < ticks) { }
        }

        public class TexturedButton
        {
            private float Scale = 1.0f;

            /// <summary>
            /// Controls the minimum size of the button after scale.
            /// </summary>
            public const float BUTTON_MIN_SCALE = 0.5f;

            /// <summary>
            /// Controls the time (in ms) to reach the minimum/maximum scale.
            /// </summary>
            private const int BUTTON_SCALE_TRANSFORM_DELAY = 100;
            /// <summary>
            /// Controls the time (in ms) to change colors.
            /// </summary>
            private const int BUTTON_COLOR_TRANSFORM_DELAY = 100;

            /// <summary>
            /// The color of the button when not moused over.
            /// </summary>
            private static readonly Color NormalColor = Color.White;
            /// <summary>
            /// The color of the button when moused over/clicked.
            /// </summary>
            private static readonly Color HotColor = new Color(0, 120, 255);
            /// <summary>
            /// The current color of the button.
            /// </summary>
            public Color Color = Color.White;

            public Texture2D Texture { get; set; }

            /// <summary>
            /// The screen position of the button.
            /// </summary>
            public Vector2 Position = Vector2.Zero;
            /// <summary>
            /// The origin of the button relative to the button's position.
            /// </summary>
            public Vector2 Origin = Vector2.Zero;
            /// <summary>
            /// The button's rectangle after scaling.
            /// </summary>
            public Rectangle DisplayRectangle
            {
                get
                {
                    return new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
                }
            }

            //Internal threads
            private Thread TriggerThread;
            private Thread TransformColourThread;

            /// <summary>
            /// Triggers the button for scaling.
            /// </summary>
            public void Trigger()
            {
                if (TriggerThread.ThreadState == System.Threading.ThreadState.Running)
                    TriggerThread.Abort();
                TriggerThread = new Thread(() =>
                {
                    float scaleAmount = (Scale - BUTTON_MIN_SCALE) / BUTTON_SCALE_TRANSFORM_DELAY;
                    while (Scale.CompareTo(BUTTON_MIN_SCALE) > 0)
                    {
                        Scale -= scaleAmount;
                        NOP(10000); //1ms
                    }
                });

                TriggerThread.IsBackground = true;
                TriggerThread.Start();
            }

            /// <summary>
            /// Triggers the button's mouse enter event.
            /// </summary>
            public void Enter()
            {
                PerformMouseEvent(true);
            }
            /// <summary>
            /// Triggers the button's mouse leave event.
            /// </summary>
            public void Leave()
            {
                PerformMouseEvent(false);
            }

            private void PerformMouseEvent(bool entered)
            {
                if (TransformColourThread.ThreadState == System.Threading.ThreadState.Running)
                    TransformColourThread.Abort();
                TransformColourThread = new Thread(() =>
                {
                    float rScaleAmount = (entered ? Color.R - HotColor.R : NormalColor.R) / (float)BUTTON_COLOR_TRANSFORM_DELAY;
                    float gScaleAmount = (entered ? Color.G - HotColor.G : NormalColor.G) / (float)BUTTON_COLOR_TRANSFORM_DELAY;
                    float bScaleAmount = (entered ? Color.B - HotColor.B : NormalColor.B) / (float)BUTTON_COLOR_TRANSFORM_DELAY;
                    while (Color != HotColor)
                    {
                        Color.R -= (byte)rScaleAmount;
                        Color.G -= (byte)gScaleAmount;
                        Color.B -= (byte)bScaleAmount;
                        NOP(10000); //1ms
                    }
                });

                TransformColourThread.IsBackground = true;
                TransformColourThread.Start();
            }
        }
    }
}
