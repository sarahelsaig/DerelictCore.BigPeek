using DerelictCore.BigPeek.Exceptions;
using DerelictCore.BigPeek.Models;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

using static DerelictCore.BigPeek.Constants.HookParameterTypes;

namespace DerelictCore.BigPeek.Services;

public sealed class PeekService : IDisposable
{
    private const string User32Dll = "user32.dll";
    private const string MagnificationDll = "magnification.dll";

    private bool _isInitialized;

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

    public MagnificationInfo MagnifyWindow(
        HWND target,
        float screenWidth,
        float screenHeight)
    {
        var windowRect = GetWindowBounds(target);
        var magnificationFactor = Math.Min(
            screenWidth / windowRect.Width,
            screenHeight / windowRect.Height);

        if (magnificationFactor < 1f)
        {
            throw new InvalidOperationException(
                "The window must be smaller than the target screen! If you have multiple screens it must be smaller " +
                "than the screen where the Big Peek window is located.");
        }

        _isInitialized = _isInitialized || Magnification.MagInitialize();
        if (!_isInitialized) throw new ApiFailureException(MagnificationDll, "Unable to initialize the Magnifier API!");

        if (!Magnification.MagSetFullscreenTransform(magnificationFactor, windowRect.X, windowRect.Y))
        {
            throw new ApiFailureException(MagnificationDll, "Unable to set full screen magnification!");
        }

        return windowRect with { MagnificationFactor = magnificationFactor };
    }

    public string GetWindowTitle(HWND windowHandle)
    {
        var length = User32.GetWindowTextLength(windowHandle) + 1;
        var builder = new StringBuilder(capacity: length);
        User32.GetWindowText(windowHandle, builder, length);

        return builder.ToString();
    }

    public (float Width, float Height) GetScreenSize(HWND windowHandle)
    {
        var monitorHandle = User32.MonitorFromWindow(windowHandle, User32.MonitorFlags.MONITOR_DEFAULTTONULL);
        if (monitorHandle.IsNull)
        {
            throw new ApiFailureException(User32Dll, "Failed to find the window's display monitor.");
        }

        User32.MONITORINFO monitorinfo = new User32.MONITORINFO();

        unsafe
        {
            monitorinfo.cbSize = (uint)sizeof(User32.MONITORINFO);
        }

        if (!User32.GetMonitorInfo(monitorHandle, ref monitorinfo))
        {
            throw new ApiFailureException(User32Dll, $"Failed to get monitor info for monitor {monitorHandle}.");
        }

        return (monitorinfo.rcMonitor.Width, monitorinfo.rcMonitor.Height);
    }

    public void ZoomOut() => Magnification.MagSetFullscreenTransform(1, 0, 0);

    private static MagnificationInfo GetWindowBounds(HWND windowHandle) =>
        User32.GetWindowRect(windowHandle, out var windowRect)
            ? new MagnificationInfo(windowRect.Width, windowRect.Height, windowRect.X, windowRect.Height)
            : throw new ApiFailureException(User32Dll, "Unable to get window bounds!");

    public void Dispose()
    {
        ZoomOut();
        Magnification.MagUninitialize();
    }
}