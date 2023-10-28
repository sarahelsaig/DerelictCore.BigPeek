using System.Runtime.InteropServices;
using System.Text.Json;
using Vanara.PInvoke;

namespace DerelictCore.BigPeek;

public partial class Form1 : Form
{
    // https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondown
    private const int WM_LBUTTONDOWN = 0x0201;

    private HTHUMBNAIL _thumbnail;
    private User32.SafeHHOOK _mouseHook = new(IntPtr.Zero);

    public Form1() => InitializeComponent();

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
        var result = DwmApi.DwmRegisterThumbnail(Handle, target, out _thumbnail);

        if (!result.Succeeded)
        {
            MessageBox.Show(
                JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }),
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }
    }
}