namespace TTMC.Tuya
{
    public class Commands
    {
        public List<Command>? commands { get; set; }
    }
    public class Command
    {
        public string? code { get; set; }
        public object? value { get; set; }
    }
    public class Base
    {
        public ushort? code { get; set; }
        public string? msg { get; set; }
        public object? result { get; set; }
        public bool success { get; set; }
        public long t { get; set; }
        public string? tid { get; set; }
    }
    public class HSV
    {
        public ushort h { get; set; }
        public ushort s { get; set; }
        public ushort v { get; set; }
    }
    public class GetTokenObj
    {
        public string? access_token { get; set; }
        public ushort expire_time { get; set; }
        public string? refresh_token { get; set; }
        public string? uid { get; set; }
    }
}