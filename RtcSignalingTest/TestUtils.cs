
using RtcSignaling;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestGenClientId()
    {
        Console.WriteLine("ID: " + new ClientIdGenerator().Gen("xxxxxx", true));
        Console.WriteLine("ID: " + new ClientIdGenerator().Gen("xxxxxx", false));
    }
    
    [Test]
    public void TestRespJsonMessage()
    {
        Console.WriteLine("" + Common.MakeJsonMessage(200, "OKOK", new Dictionary<string, object>
        {
            {"Name", "Jack Sparrow"},
            {"Age", "I don't known"}
        }));
    }
}