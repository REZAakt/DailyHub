using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Errors;

public static class ErrorCodes
{
    public const string Unknown = "ERR_UNKNOWN";
    public const string InvalidParameters = "ERR_INVALID_PARAMETERS";
    public const string UpstreamUnavailable = "ERR_UPSTREAM_UNAVAILABLE";
}
