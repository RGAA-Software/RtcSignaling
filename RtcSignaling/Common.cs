using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

namespace RtcSignaling;

public class Common
{
    public static string GetCurrentTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
    }

    public static long GetCurrentTimestamp()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    }
    
    public static string Md5String(string input)
    {
        using MD5 md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        var sb = new StringBuilder();
        foreach (var t in hashBytes)
        {
            sb.Append(t.ToString("x2"));
        }
        return sb.ToString();
    }

    public static string MakeJsonMessage(int code, string msg, Dictionary<string, object> value)
    {
        var resp = new Dictionary<string, object>
        {
            {"code", code},
            {"msg", msg},
            {"value", value}
        };
        return JsonConvert.SerializeObject(resp);
    }

    public static string MakeOkJsonMessage(Dictionary<string, object> value)
    {
        return MakeJsonMessage(200, "Ok", value);
    }

    public static string MakeOkJsonMessage()
    {
        return MakeJsonMessage(200, "Ok", new Dictionary<string, object>());
    }
    
    public static bool IsEmpty(string? value)
    {
        return value == null || value.Length <= 0;
    }
}