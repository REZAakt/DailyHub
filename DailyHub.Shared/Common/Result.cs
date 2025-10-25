using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Common;

public sealed class Result<T>
{
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Value { get; init; }

    public static Result<T> Ok(T value) => new() { Success = true, Value = value };
    public static Result<T> Fail(string code, string? message = null) => new() { Success = false, ErrorCode = code, ErrorMessage = message };
}
