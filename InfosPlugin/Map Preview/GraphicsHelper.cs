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

        public class TexturedControl
        {
            protected Control Container { get; set; }
            public float Scale = 0.5f;
            public Color Color = Color.White;
            public Texture2D Texture { get; set; }
            public Vector2 Position = Vector2.Zero;
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
            /// Creates a new textured control with the default scale.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="texture">The texture of the button.</param>
            public TexturedControl(Control container, Texture2D texture)
            {
                Container = container;
                Texture = texture;
            }
            /// <summary>
            /// Creates a new textured control with the specified scale.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="texture">The texture of the button.</param>
            /// <param name="scale">The button size scale.</param>
            public TexturedControl(Control container, Texture2D texture, float scale)
            {
                Container = container;
                Texture = texture;
                Scale = scale;
            }

            /// <summary>
            /// The control's rectangle after scaling.
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

            /// <summary>
            /// The control's MouseClick event.
            /// </summary>
            public virtual void Trigger() { }
            /// <summary>
            /// The control's MouseEnter event.
            /// </summary>
            public virtual void Enter() { }
            /// <summary>
            /// The control's MouseLeave event.
            /// </summary>
            public virtual void Leave() { }
        }

        public class TexturedButton : TexturedControl
        {
            ///<summary>
            /// Creates a new textured button control.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="texture">The texture of the button.</param>
            public TexturedButton(Control container, Texture2D texture) : base(container, texture)
            {
                NormalTexture = texture;
                TriggeredTexture = texture;
            }
            ///<summary>
            /// Creates a new textured button control with the specified scale.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="texture">The texture of the button.</param>
            /// <param name="scale">The button size scale.</param>
            public TexturedButton(Control container, Texture2D texture, float scale) : base(container, texture, scale)
            {
                NormalTexture = texture;
                TriggeredTexture = texture;
            }
            ///<summary>
            /// Creates a new textured button control which changes texture when triggered.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="normalTexture">The texture of the button when normal.</param>
            /// <param name="triggerTexture">The texture of the button when clicked.</param>
            public TexturedButton(Control container, Texture2D normalTexture, Texture2D triggerTexture) : base(container, normalTexture)
            {
                NormalTexture = normalTexture;
                TriggeredTexture = triggerTexture;
            }
            ///<summary>
            /// Creates a new textured button control with the specified scale which changes texture when triggered.
            /// </summary>
            /// <param name="container">The object containing the button.</param>
            /// <param name="normalTexture">The texture of the button when normal.</param>
            /// <param name="triggerTexture">The texture of the button when clicked.</param>
            /// <param name="scale">The button size scale.</param>
            public TexturedButton(Control container, Texture2D normalTexture, Texture2D triggerTexture, float scale) : base(container, normalTexture, scale)
            {
                NormalTexture = normalTexture;
                TriggeredTexture = triggerTexture;
            }

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

            //Internal scale thread
            private Thread TriggerThread;

            public override void Trigger()
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

                        //Force graphics redraw on the container
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

            public override void Enter()
            {
                Color = HotColor;
            }

            public override void Leave()
            {
                Color = NormalColor;
            }
        }
    }
}
