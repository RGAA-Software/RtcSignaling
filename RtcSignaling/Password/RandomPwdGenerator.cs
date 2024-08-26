using System.Text;

namespace RtcSignaling.Password;

public class RandomPwdGenerator
{
    private const string RandomCharSink = "1234567890abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ";

    public static string GenRandomPassword()
    {
        var random = new Random();
        var sb = new StringBuilder();
        for (var i = 0; i < 8; i++)
        {
            sb.Append(RandomCharSink[random.Next(0, RandomCharSink.Length)]);
        }
        return sb.ToString();
    }

}