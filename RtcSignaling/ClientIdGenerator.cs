namespace RtcSignaling;

public class ClientIdGenerator
{
    public string Gen(string info)
    {
        var md5Str = Common.Md5String(info);
        var targetId = "";
        for (var i = 0; i < md5Str.Length; i++)
        {
            var c = md5Str[i];
            switch (i)
            {
                case 0:
                case 3:
                case 5:
                case 7:
                case 9:
                case 12:
                case 14:
                case 16:
                case 20:
                    targetId += c % 10;
                    break;
            }
        }
        return targetId;
    }
}