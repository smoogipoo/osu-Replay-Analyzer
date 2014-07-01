using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InfosPlugin
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

            private Control Container { get; set; }

            /// <summary>
            /// Creates a new TexturedButton.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="texture">The texture of the button.</param>
            public TexturedButton(Control container, Texture2D texture)
            {
                Container = container;
                NormalTexture = texture;
                TriggeredTexture = texture;
                Texture = NormalTexture;
            }
            /// <summary>
            /// Creates a new TexturedButton.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="texture">The texture of the button.</param>
            /// <param name="scale">The button size scale.</param>
            public TexturedButton(Control container, Texture2D texture, float scale)
            {
                Container = container;
                NormalTexture = texture;
                TriggeredTexture = texture;
                Texture = NormalTexture;
                Scale = scale;
            }
            /// <summary>
            /// Creates a new TexturedButton.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="normalTexture">The texture of the button when normal.</param>
            /// <param name="triggerTexture">The texture of the button when clicked.</param>
            public TexturedButton(Control container, Texture2D normalTexture, Texture2D triggerTexture)
            {
                Container = container;
                NormalTexture = normalTexture;
                TriggeredTexture = triggerTexture;
                Texture = NormalTexture;
            }
            /// <summary>
            /// Creates a new TexturedButton.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="normalTexture">The texture of the button when normal.</param>
            /// <param name="triggerTexture">The texture of the button when clicked.</param>
            /// <param name="scale">The button size scale.</param>
            public TexturedButton(Control container, Texture2D normalTexture, Texture2D triggerTexture, float scale)
            {
                Container = container;
                NormalTexture = normalTexture;
                TriggeredTexture = triggerTexture;
                Texture = NormalTexture;
                Scale = scale;
            }

            public float Scale = 0.5f;

            /// <summary>
            /// Controls the maximum size of the button before scaling.
            /// </summary>
            public const float BUTTON_MAX_SCALE = 0.5f;
            /// <summary>
            /// Controls the minimum size of the button after scaling.
            /// </summary>
            public const float BUTTON_MIN_SCALE = 0.35f;

            /// <summary>
            /// Controls the time (in ms) to reach the minimum/maximum scale.
            /// </summary>
            private const int BUTTON_TRANSFORM_DELAY = 100;

            /// <summary>
            /// Controls the color when the button is entered.
            /// </summary>
            public Color HotColor = new Color(0, 120, 255);
            /// <summary>
            /// Controls the color when the button is not entered.
            /// </summary>
            public Color NormalColor = Color.White;

            /// <summary>
            /// The current color of the button.
            /// </summary>
            public Color Color = Color.White;

            /// <summary>
            /// The button's current texture.
            /// </summary>
            public Texture2D Texture { get; set; }

            /// <summary>
            /// The button's normal texture.
            /// </summary>
            private Texture2D NormalTexture { get; set; }
            /// <summary>
            /// The triggered texture of the button.
            /// </summary>
            private Texture2D TriggeredTexture { get; set; }
            /// <summary>
            /// Determines if the button is triggered.
            /// </summary>
            public bool Triggered = false;

            /// <summary>
            /// The screen position of the button.
            /// </summary>
            public Vector2 Position = Vector2.Zero;
            /// <summary>
            /// The origin of the button relative to the button's position.
            /// </summary>
            public Vector2 Origin
            {
                get
                {
                    if (Texture != null)
                        return new Vector2(Texture.Width / 2f, Texture.Height / 2f);
                    return Vector2.Zero;
                }
            }

            /// <summary>
            /// The button's rectangle after scaling.
            /// </summary>
            public Rectangle DisplayRectangle
            {
                get
                {
                    if (Texture != null)
                        return new Rectangle((int)(Position.X - Origin.X * Scale), (int)(Position.Y - Origin.Y * Scale), (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
                    return Rectangle.Empty;
                }
            }

            //Internal threads
            private Thread TriggerThread;

            /// <summary>
            /// Triggers the button for scaling.
            /// </summary>
            public void Trigger()
            {
                Triggered = !Triggered;
                if (TriggerThread != null && TriggerThread.ThreadState == System.Threading.ThreadState.Running)
                    TriggerThread.Abort();
                TriggerThread = new Thread(() =>
                {
                    float scaleAmount = (Scale - BUTTON_MIN_SCALE) / BUTTON_TRANSFORM_DELAY;
                    //Shrink animation
                    while (Scale.CompareTo(BUTTON_MIN_SCALE) > 0)
                    {
                        Scale -= scaleAmount;
                        Container.Invalidate();
                        NOP(10000); //1ms
                    }
                    //Reset scale
                    Scale = BUTTON_MAX_SCALE;

                    //Refresh the container
                    Container.Invalidate();

                    Texture = TriggeredTexture;
                    TriggeredTexture = NormalTexture;
                    NormalTexture = Texture;
                });

                TriggerThread.IsBackground = true;
                TriggerThread.Start();

            }

            /// <summary>
            /// Triggers the button's mouse enter event.
            /// </summary>
            public void Enter()
            {
                Color = HotColor;
            }
            /// <summary>
            /// Triggers the button's mouse leave event.
            /// </summary>
            public void Leave()
            {
                Color = NormalColor;
            }
        }
    }
}
