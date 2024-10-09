using Serilog;

namespace RtcSignaling;

public class ClientIdGenerator
{
    public string Gen(string info, bool fixedIndices)
    {
        var md5Str = Common.Md5String(info);
        var targetId = "";
        var indices = fixedIndices ? GetFixedIndices() : GenIndices();

        foreach (var idx in indices)
        {
            var c = md5Str[idx];
            targetId += c % 10;
        }
        return targetId;
    }

    private static List<int> GetFixedIndices()
    {
        return [2, 7, 13, 15, 19, 23, 26, 29, 31];
    }
    
    private static List<int> GenIndices()
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