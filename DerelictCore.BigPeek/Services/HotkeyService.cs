using DerelictCore.BigPeek.Constants;
using DerelictCore.BigPeek.Exceptions;
using System;
using System.Windows.Interop;
using Vanara.PInvoke;

namespace DerelictCore.BigPeek.Services;

public sealed class HotkeyService : IDisposable
{
    private readonly int _id;
    private readonly IntPtr _handle;
    private readonly HwndSource _source;
    private readonly User32.HotKeyModifiers _modifiers;
    private readonly uint _vk;
    private readonly Action<IntPtr> _callback;

    private HotkeyService(int id, IntPtr handle, HwndSource source, User32.HotKeyModifiers modifiers, uint vk, Action<IntPtr> callback)
    {
        _id = id;
        _handle = handle;
        _source = source;
        _modifiers = modifiers;
        _vk = vk;
        _callback = callback;
    }

    private HotkeyService RegisterHotKey()
    {
        _source.AddHook(HwndHook);

        if (!User32.RegisterHotKey(_handle, _id, _modifiers, _vk))
        {
            throw new ApiFailureException("user32.dll", "Failed to register hotkey: " + Win32Error.GetLastError());
        }

        return this;
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == HookParameterTypes.Hotkey && wParam.ToInt32() == _id)
        {
            try
            {
                _callback(lParam);
                handled = true;
            }
            catch (Exception)
            {
                handled = false;
            }
        }

        return IntPtr.Zero;
    }
    public void Dispose()
    {
        _source.RemoveHook(HwndHook);
        User32.UnregisterHotKey(_handle, _id);
    }

    public static HotkeyService Create(int id, IntPtr handle, HwndSource source, User32.HotKeyModifiers modifiers, uint vk, Action<IntPtr> callback) =>
        new HotkeyService(id, handle, source, modifiers, vk, callback).RegisterHotKey();
}