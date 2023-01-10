using System;
using System.Linq;
using System.Numerics;
using ImPlotNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

using static ImGuiNET.ImGuiNative;

namespace ImGuiNET
{
    class Program
    {
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiController _controller;
        // private static MemoryEditor _memoryEditor;

        // UI state
        private static float _f = 0.0f;
        private static int _counter = 0;
        private static int _dragInt = 0;
        private static Vector3 _clearColor = new Vector3(0.45f, 0.55f, 0.6f);
        private static bool _showImGuiDemoWindow = true;
        private static bool _showAnotherWindow = false;
        private static bool _showMemoryEditor = false;
        private static byte[] _memoryEditorData;
        private static uint s_tab_bar_flags = (uint)ImGuiTabBarFlags.Reorderable;
        static bool[] s_opened = { true, true, true, true }; // Persistent user state
        private static ImGui _imgui;

        static void SetThing(out float i, float val) { i = val; }

        static void Main(string[] args)
        {
            // Create window, GraphicsDevice, and all resources necessary for the demo.
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "_imgui.NET Sample Program"),
                new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
                out _window,
                out _gd);
            _window.Resized += () =>
            {
                _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller.WindowResized(_window.Width, _window.Height);
            };
            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
            // _memoryEditor = new MemoryEditor();
            Random random = new Random();
            _memoryEditorData = Enumerable.Range(0, 1024).Select(i => (byte)random.Next(255)).ToArray();

            // Main application loop
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to _imgui.

                SubmitUI();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
            _imgui = new ImGui();
        }

        private static unsafe void SubmitUI()
        {
            // Demo code adapted from the official Dear ImGui demo program:
            // https://github.com/ocornut/imgui/blob/master/examples/example_win32_directx11/main.cpp#L172

            // 1. Show a simple window.
            // Tip: if we don't call _imgui.BeginWindow()/_imgui.EndWindow() the widgets automatically appears in a window called "Debug".
            {
                _imgui.Text("Hello, world!");                                        // Display some text (you can use a format string too)
                _imgui.SliderFloat("float", ref _f, 0, 1, _f.ToString("0.000"));  // Edit 1 float using a slider from 0.0f to 1.0f    
                //_imgui.ColorEdit3("clear color", ref _clearColor);                   // Edit 3 floats representing a color

                _imgui.Text($"Mouse position: {_imgui.GetMousePos()}");

                _imgui.Checkbox("ImGui Demo Window", ref _showImGuiDemoWindow);                 // Edit bools storing our windows open/close state
                _imgui.Checkbox("Another Window", ref _showAnotherWindow);
                _imgui.Checkbox("Memory Editor", ref _showMemoryEditor);
                if (_imgui.Button("Button"))                                         // Buttons return true when clicked (NB: most widgets return true when edited/activated)
                    _counter++;
                _imgui.SameLine(0, -1);
                _imgui.Text($"counter = {_counter}");

                _imgui.DragInt("Draggable Int", ref _dragInt);

                float framerate = _imgui.GetIO().Framerate;
                _imgui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");
            }

            // 2. Show another simple window. In most cases you will use an explicit Begin/End pair to name your windows.
            if (_showAnotherWindow)
            {
                _imgui.Begin("Another Window", ref _showAnotherWindow);
                _imgui.Text("Hello from another window!");
                if (_imgui.Button("Close Me"))
                    _showAnotherWindow = false;
                _imgui.End();
            }

            // 3. Show the ImGui demo window. Most of the sample code is in _imgui.ShowDemoWindow(). Read its code to learn more about Dear ImGui!
            if (_showImGuiDemoWindow)
            {
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                _imgui.SetNextWindowPos(new Vector2(650, 20), ImGuiCond.FirstUseEver);
                _imgui.ShowDemoWindow(ref _showImGuiDemoWindow);
            }
            
            if (_imgui.TreeNode("Tabs"))
            {
                if (_imgui.TreeNode("Basic"))
                {
                    ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
                    if (_imgui.BeginTabBar("MyTabBar", tab_bar_flags))
                    {
                        if (_imgui.BeginTabItem("Avocado"))
                        {
                            _imgui.Text("This is the Avocado tab!\nblah blah blah blah blah");
                            _imgui.EndTabItem();
                        }
                        if (_imgui.BeginTabItem("Broccoli"))
                        {
                            _imgui.Text("This is the Broccoli tab!\nblah blah blah blah blah");
                            _imgui.EndTabItem();
                        }
                        if (_imgui.BeginTabItem("Cucumber"))
                        {
                            _imgui.Text("This is the Cucumber tab!\nblah blah blah blah blah");
                            _imgui.EndTabItem();
                        }
                        _imgui.EndTabBar();
                    }
                    _imgui.Separator();
                    _imgui.TreePop();
                }

                if (_imgui.TreeNode("Advanced & Close Button"))
                {
                    // Expose a couple of the available flags. In most cases you may just call BeginTabBar() with no flags (0).
                    _imgui.CheckboxFlags("ImGuiTabBarFlags_Reorderable", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.Reorderable);
                    _imgui.CheckboxFlags("ImGuiTabBarFlags_AutoSelectNewTabs", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.AutoSelectNewTabs);
                    _imgui.CheckboxFlags("ImGuiTabBarFlags_NoCloseWithMiddleMouseButton", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.NoCloseWithMiddleMouseButton);
                    if ((s_tab_bar_flags & (uint)ImGuiTabBarFlags.FittingPolicyMask) == 0)
                        s_tab_bar_flags |= (uint)ImGuiTabBarFlags.FittingPolicyDefault;
                    if (_imgui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyResizeDown", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.FittingPolicyResizeDown))
                s_tab_bar_flags &= ~((uint)ImGuiTabBarFlags.FittingPolicyMask ^ (uint)ImGuiTabBarFlags.FittingPolicyResizeDown);
                    if (_imgui.CheckboxFlags("ImGuiTabBarFlags_FittingPolicyScroll", ref s_tab_bar_flags, (uint)ImGuiTabBarFlags.FittingPolicyScroll))
                s_tab_bar_flags &= ~((uint)ImGuiTabBarFlags.FittingPolicyMask ^ (uint)ImGuiTabBarFlags.FittingPolicyScroll);

                    // Tab Bar
                    string[] names = { "Artichoke", "Beetroot", "Celery", "Daikon" };

                    for (int n = 0; n < s_opened.Length; n++)
                    {
                        if (n > 0) { _imgui.SameLine(); }
                        _imgui.Checkbox(names[n], ref s_opened[n]);
                    }

                    // Passing a bool* to BeginTabItem() is similar to passing one to Begin(): the underlying bool will be set to false when the tab is closed.
                    if (_imgui.BeginTabBar("MyTabBar", (ImGuiTabBarFlags)s_tab_bar_flags))
                    {
                        for (int n = 0; n < s_opened.Length; n++)
                            if (s_opened[n] && _imgui.BeginTabItem(names[n], ref s_opened[n]))
                            {
                                _imgui.Text($"This is the {names[n]} tab!");
                                if ((n & 1) != 0)
                                    _imgui.Text("I am an odd tab.");
                                _imgui.EndTabItem();
                            }
                        _imgui.EndTabBar();
                    }
                    _imgui.Separator();
                    _imgui.TreePop();
                }
                _imgui.TreePop();
            }

            ImGuiIOPtr io = _imgui.GetIO();
            SetThing(out io.DeltaTime, 2f);

            if (_showMemoryEditor)
            {
                _imgui.Text("Memory editor currently supported.");
                // _memoryEditor.Draw("Memory Editor", _memoryEditorData, _memoryEditorData.Length);
            }
        }
    }
}
