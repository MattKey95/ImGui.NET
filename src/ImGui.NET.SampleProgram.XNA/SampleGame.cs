using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Num = System.Numerics;

namespace ImGuiNET.SampleProgram.XNA
{
    /// <summary>
    /// Simple FNA + ImGui example
    /// </summary>
    public class SampleGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer _imGuiRenderer;

        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;

        public SampleGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.PreferMultiSampling = true;

            IsMouseVisible = true;
            _imgui = new ImGui();
        }

        protected override void Initialize()
        {
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Texture loading example

			// First, load the texture as a Texture2D (can also be done using the XNA/FNA content pipeline)
			_xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
			{
				var red = (pixel % 300) / 2;
				return new Color(red, 1, 1);
			});

			// Then, bind it to an ImGui-friendly pointer, that we can use during regular _imgui.** calls (see below)
			_imGuiTexture = _imGuiRenderer.BindTexture(_xnaTexture);

            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(clear_color.X, clear_color.Y, clear_color.Z));

            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout();

            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        // Direct port of the example at https://github.com/ocornut/imgui/blob/master/examples/sdl_opengl2_example/main.cpp
        private float f = 0.0f;

        private bool show_test_window = false;
        private bool show_another_window = false;
        private Num.Vector3 clear_color = new Num.Vector3(114f / 255f, 144f / 255f, 154f / 255f);
        private byte[] _textBuffer = new byte[100];
        private ImGui _imgui;

        protected virtual void ImGuiLayout()
        {
            // 1. Show a simple window
            // Tip: if we don't call _imgui.Begin()/_imgui.End() the widgets appears in a window automatically called "Debug"
            {
                _imgui.Text("Hello, world!");
                _imgui.SliderFloat("float", ref f, 0.0f, 1.0f, string.Empty);
                _imgui.ColorEdit3("clear color", ref clear_color);
                if (_imgui.Button("Test Window")) show_test_window = !show_test_window;
                if (_imgui.Button("Another Window")) show_another_window = !show_another_window;
                _imgui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / _imgui.GetIO().Framerate, _imgui.GetIO().Framerate));

                _imgui.InputText("Text input", _textBuffer, 100);

                _imgui.Text("Texture sample");
                _imgui.Image(_imGuiTexture, new Num.Vector2(300, 150), Num.Vector2.Zero, Num.Vector2.One, Num.Vector4.One, Num.Vector4.One); // Here, the previously loaded texture is used
            }

            // 2. Show another simple window, this time using an explicit Begin/End pair
            if (show_another_window)
            {
                _imgui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
                _imgui.Begin("Another Window", ref show_another_window);
                _imgui.Text("Hello");
                _imgui.End();
            }

            // 3. Show the ImGui test window. Most of the sample code is in _imgui.ShowTestWindow()
            if (show_test_window)
            {
                _imgui.SetNextWindowPos(new Num.Vector2(650, 20), ImGuiCond.FirstUseEver);
                _imgui.ShowDemoWindow(ref show_test_window);
            }
        }

		public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
		{
			//initialize a texture
			var texture = new Texture2D(device, width, height);

			//the array holds the color for each pixel in the texture
			Color[] data = new Color[width * height];
			for(var pixel = 0; pixel < data.Length; pixel++)
			{
				//the function applies the color according to the specified pixel
				data[pixel] = paint( pixel );
			}

			//set the color
			texture.SetData( data );

			return texture;
		}
	}
}