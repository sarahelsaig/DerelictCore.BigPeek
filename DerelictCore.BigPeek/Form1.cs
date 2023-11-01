using System.Runtime.InteropServices;
using System.Text.Json;
using Vanara.PInvoke;

namespace DerelictCore.BigPeek;

public partial class Form1 : Form
{
    // https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondown
    private const int WM_LBUTTONDOWN = 0x0201;

    private User32.SafeHHOOK _mouseHook = new(IntPtr.Zero);

    public Form1() => InitializeComponent();

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        Magnification.MagSetFullscreenTransform(1, 0, 0);
        Magnification.MagUninitialize();
    }

    private void PickWindowButton_Click(object sender, EventArgs e)
    {
        PickWindowButton.Enabled = false;

        // Register a global mouse event handler (hook) to look for the next left click that doesn't target our window.
        _mouseHook = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL, (code, wParam, lParam) =>
        {
            if (code >= 0 && wParam == new IntPtr(WM_LBUTTONDOWN))
            {
                var mouseHook = Marshal.PtrToStructure<User32.MOUSEHOOKSTRUCT>(lParam);
                var target = User32.WindowFromPoint(mouseHook.pt);

                // Unregister the global mouse envent handler and start projection.
                if (target != Handle)
                {
                    User32.UnhookWindowsHookEx(_mouseHook);
                    PickWindowButton.Enabled = true;

                    RegisterWindow(target);
                }
            }

            return User32.CallNextHookEx(User32.HHOOK.NULL, code, wParam, lParam);
        });
    }

    private void RegisterWindow(HWND target)
    {
        if (!User32.GetWindowRect(target, out var windowRect))
        {
            Error("Unable to get window bounds!");
            return;
        }

        var windowBounds = GetBounds(windowRect);
        var screenBounds = GetBounds(Screen.FromControl(this).Bounds);
        var magnificationFactor = Math.Min(
            screenBounds.Width / windowBounds.Width,
            screenBounds.Height / windowBounds.Height);

        if (magnificationFactor < 1f)
        {
            Error("The window must be smaller than the target screen! If you have multiple screens it must be " +
                  "smaller than the screen where the Big Peek window is located.");
        }

        if (!Magnification.MagInitialize())
        {
            Error("Unable to initialize the Magnifier API!");
            return;
        }

        if (!Magnification.MagSetFullscreenTransform(magnificationFactor, windowRect.X, windowRect.Y))
        {
            Error("Unable to set full screen magnification!");
            return;
        }
    }

    // Intentionally fire-and-forget so the error message isn't blocking stuff like a global hook.
    private static void Error(object errorObject) =>
        Task.Run(() => MessageBox.Show(
            errorObject as string ?? PrettyJson(errorObject),
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error));

    private static string PrettyJson(object data) =>
        JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

    private static (float Width, float Height) GetBounds(Rectangle rectangle) =>
        (rectangle.Width, rectangle.Height);
}