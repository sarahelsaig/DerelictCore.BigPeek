using DerelictCore.BigPeek.Exceptions;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Vanara.PInvoke;

using static DerelictCore.BigPeek.Constants.HookTypes;

namespace DerelictCore.BigPeek.Services;

public sealed class PeekService : IDisposable
{
    private const string User32Dll = "user32.dll";
    private const string MagnificationDll = "magnification.dll";

    public Task<HWND> PickWindowAsync(params HWND[] ignoredWindowHandles)
    {
        var task = new TaskCompletionSource<HWND>();
        var mouseHook = new User32.HHOOK();

        // Register a global mouse event handler (hook) to look for the next left click that doesn't target our window.
        mouseHook = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL, (code, wParam, lParam) =>
        {
            try
            {
                if (code >= 0 && wParam == new IntPtr(LeftButtonDown))
                {
                    var clickLocation = Marshal.PtrToStructure<User32.MOUSEHOOKSTRUCT>(lParam).pt;
                    var target = User32.WindowFromPoint(clickLocation);

                    // Unregister the global mouse envent handler and try to return the result..
                    if (!ignoredWindowHandles.Contains(target))
                    {
                        User32.UnhookWindowsHookEx(mouseHook);
                        task.TrySetResult(target);
                    }
                }
            }
            catch (Exception exception)
            {
                if (!mouseHook.IsNull) User32.UnhookWindowsHookEx(mouseHook);
                task.SetException(exception);
            }

            return User32.CallNextHookEx(User32.HHOOK.NULL, code, wParam, lParam);
        });

        return task.Task;
    }

    public void MagnifyWindow(HWND target, float screenWidth, float screenHeight)
    {
        if (!User32.GetWindowRect(target, out var windowRect))
        {
            throw new ApiFailureException(User32Dll, "Unable to get window bounds!");
        }

        var windowBounds = GetBounds(windowRect);
        var magnificationFactor = Math.Min(
            screenWidth / windowBounds.Width,
            screenHeight / windowBounds.Height);

        if (magnificationFactor < 1f)
        {
            throw new InvalidOperationException(
                "The window must be smaller than the target screen! If you have multiple screens it must be smaller " +
                "than the screen where the Big Peek window is located.");
        }

        if (!Magnification.MagInitialize())
        {
            throw new ApiFailureException(MagnificationDll, "Unable to initialize the Magnifier API!");
        }

        if (!Magnification.MagSetFullscreenTransform(magnificationFactor, windowRect.X, windowRect.Y))
        {
            throw new ApiFailureException(MagnificationDll, "Unable to set full screen magnification!");
        }
    }

    private static (float Width, float Height) GetBounds(Rectangle rectangle) =>
        (rectangle.Width, rectangle.Height);

    public void Dispose()
    {
        Magnification.MagSetFullscreenTransform(1, 0, 0);
        Magnification.MagUninitialize();
    }
}