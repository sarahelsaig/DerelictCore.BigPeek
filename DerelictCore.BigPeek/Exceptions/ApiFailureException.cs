using System;
using System.Runtime.CompilerServices;

namespace DerelictCore.BigPeek.Exceptions;

public class ApiFailureException : Exception
{
    public string ApiName { get; set; }
    public string Caller { get; set; }

    public ApiFailureException(string api, string message, [CallerMemberName] string? caller = null)
        : base(message)
    {
        ApiName = api;
        Caller = caller ?? "[unknown caller]";
    }
}