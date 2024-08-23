using Serilog;

namespace RtcSignaling;

public class ClientIdGenerator
{
    public string Gen(string info)
    {
        var md5Str = Common.Md5String(info);
        var targetId = "";
        var indices = GenIndices();
        foreach (var idx in indices)
        {
            var c = md5Str[idx];
            targetId += c % 10;
        }
        return targetId;
    }

    private List<int> GenIndices()
    {
        var values = new List<int>();
        for (var i = 0; i < 9; i++)
        {
            while (true)
            {
                var newValue = new Random().Next(0, 32);
                var exists = values.Any(val => val == newValue);
                if (exists) continue;
                
                values.Add(newValue);
                break;
            }
        }
        return values;
    }
}