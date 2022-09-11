using System.Drawing;
using System.Text;
using System.Text.Json;

namespace TTMC.Tuya
{
    public class Device
    {
        private Profile? brain;
        private string? device = null;
        private HttpClient client;
        public Device(Profile profile, string deviceID)
        {
            device = deviceID;
            brain = profile;
            client = brain.client;
        }
        public Base? SwitchLED(bool mode)
        {
            return SendCommand("switch_led", mode);
        }
        public Base? ChangeColor(Color color)
        {
            return SendCommand("colour_data", new HSV() { h = (ushort)color.GetHue(), s = (ushort)(color.GetSaturation() * 255), v = (ushort)(color.GetBrightness() * 255) });
        }
        public Base? ChangeColor(HSV color)
        {
            return SendCommand("colour_data", color);
        }
        public Base? WorkMode(string color)
        {
            if (color == "white" && color == "colour")
            {
                return SendCommand("work_mode", color);
            }
            return null;
        }
        public Base? TempValue(ushort value)
        {
            if (value >= 0 && value <= 255)
            {
                return SendCommand("temp_value", value);
            }
            return null;
        }
        public Base? BrightValue(ushort value)
        {
            if (value >= 25 && value <= 255)
            {
                return SendCommand("bright_value", value);
            }
            return null;
        }
        private Base? SendCommand(string command, object value)
        {
            if (brain != null && brain.accessToken != null && !string.IsNullOrEmpty(brain.secret))
            {
                Command cmd = new() { code = command, value = value };
                Commands cmds = new() { commands = new() { cmd } };
                string json = JsonSerializer.Serialize(cmds);
                string method = "POST";
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                string signUrl = "/v1.0/devices/" + device + "/commands";
                string contentHash = Engine.GetHash(json);
                string stringToSign = string.Join("\n", method, contentHash, string.Empty, signUrl);
                string signStr = brain.ID + brain.accessToken + timestamp + stringToSign;
                client.DefaultRequestHeaders.Remove("t");
                client.DefaultRequestHeaders.TryAddWithoutValidation("t", timestamp.ToString());
                client.DefaultRequestHeaders.Remove("sign");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sign", Engine.Encrypt(signStr, brain.secret));
                client.DefaultRequestHeaders.Remove("access_token");
                client.DefaultRequestHeaders.TryAddWithoutValidation("access_token", brain.accessToken);
                HttpResponseMessage resp = client.PostAsync(signUrl, new StringContent(json, Encoding.UTF8, "application/json")).Result;
                return JsonSerializer.Deserialize<Base>(resp.Content.ReadAsStringAsync().Result);
            }
            return null;
        }
    }
    public class Profile
    {
        public string? secret = null;
        public string ID = "";
        public string? accessToken = null;
        public string? refreshToken = null;
        public HttpClient client = new();
        public Profile(string clientID, string clientSecret, string server = "https://openapi.tuyaeu.com")
        {
            secret = clientSecret;
            ID = clientID;
            client.BaseAddress = new Uri(server);
            client.DefaultRequestHeaders.TryAddWithoutValidation("lang", "en");
            client.DefaultRequestHeaders.TryAddWithoutValidation("sign_method", "HMAC-SHA256");
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_id", clientID);
            GetToken();
        }
        public Device GetDevice(string deviceID)
        {
            return new Device(this, deviceID);
        }
        private void GetToken()
        {
            if (secret != null)
            {
                string method = "GET";
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                string signUrl = "/v1.0/token?grant_type=1";
                string contentHash = Engine.GetHash(string.Empty);
                string stringToSign = string.Join("\n", method, contentHash, string.Empty, signUrl);
                string signStr = ID + timestamp + stringToSign;
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, signUrl);
                req.Headers.Add("t", timestamp);
                req.Headers.Add("sign", Engine.Encrypt(signStr, secret));
                HttpResponseMessage resp = client.SendAsync(req).Result;
                string json = resp.Content.ReadAsStringAsync().Result;
                Base? getToken = JsonSerializer.Deserialize<Base>(json);
                if (getToken != null && getToken.success && getToken.result != null)
                {
                    JsonElement element = (JsonElement)getToken.result;
                    GetTokenObj? nzx = JsonSerializer.Deserialize<GetTokenObj>(element.GetRawText());
                    if (nzx != null)
                    {
                        if (nzx.access_token != null && nzx.refresh_token != null)
                        {
                            accessToken = nzx.access_token;
                            refreshToken = nzx.refresh_token;
                        }
                    }
                }
            }
        }
    }
}