namespace RtcSignaling.Settings;

public class Settings
{
    public int ListenPort { get; set; } 
    
    public string Certificate { get; set; } = "";

    public string Password { get; set; } = "";

    public string Dump()
    {
        return "Port: " + ListenPort + ", Certificate: " + Certificate + ", Password: " + Password;
    }

}