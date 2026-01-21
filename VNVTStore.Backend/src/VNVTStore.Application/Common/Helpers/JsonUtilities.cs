using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VNVTStore.Application.Common.Helpers;

public static class JsonUtilities
{
    public static bool CheckJsonArray(object? value)
    {
        if (value == null) return false;
        if (value is JArray) return true;
        
        var str = value.ToString();
        if (string.IsNullOrWhiteSpace(str)) return false;

        str = str.Trim();
        return (str.StartsWith("[") && str.EndsWith("]"));
    }
}
