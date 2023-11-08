using System.Drawing;
using System.Net.Http.Json;
using System.Text.Json;

namespace TTMC.Tuya
{
	public class Device
	{
		private Profile profile;
		private string deviceID;
		public Device(Profile profile, string deviceID)
		{
			this.deviceID = deviceID;
			this.profile = profile;
		}
		public Base SwitchLED(bool mode)
		{
			return SendCommand("switch_led", mode);
		}
		public Base ChangeColor(Color color)
		{
			return SendCommand("colour_data", new HSV() { h = (ushort)color.GetHue(), s = (ushort)(color.GetSaturation() * 255), v = (ushort)(color.GetBrightness() * 255) });
		}
		public Base ChangeColor(HSV color)
		{
			return SendCommand("colour_data", color);
		}
		public Base WorkMode(string color)
		{
			return SendCommand("work_mode", color);
		}
		public Base TempValue(ushort value)
		{
			return SendCommand("temp_value", value);
		}
		public Base BrightValue(ushort value)
		{
			return SendCommand("bright_value", value);
		}
		private Base SendCommand(string command, object value)
		{
			Command cmd = new() { code = command, value = value };
			Commands cmds = new() { commands = new() { cmd } };
			string json = JsonSerializer.Serialize(cmds, Engine.jsonSerializerOptions);
			string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
			string signUrl = $"/v1.0/devices/{deviceID}/commands";
			string stringToSign = string.Join("\n", "POST", Engine.GetHash(json), string.Empty, signUrl);
			string signStr = profile.clientID + profile.accessToken + timestamp + stringToSign;
			JsonContent jsonContent = JsonContent.Create(cmds, options: Engine.jsonSerializerOptions);
			jsonContent.Headers.Add("t", timestamp.ToString());
			jsonContent.Headers.Add("sign", Engine.Encrypt(signStr, profile.clientSecret));
			jsonContent.Headers.Add("access_token", profile.accessToken);
			HttpResponseMessage resp = profile.client.PostAsync(signUrl, jsonContent).Result;
			string response = resp.Content.ReadAsStringAsync().Result;
			return Engine.ToBase(response);
		}
	}
	public class Profile
	{
		public string clientSecret;
		public string clientID;
		public string accessToken;
		public string refreshToken;
		public HttpClient client;
		public Profile(string clientID, string clientSecret, string server = "https://openapi.tuyaeu.com")
		{
			this.clientSecret = clientSecret;
			this.clientID = clientID;
			client = new()
			{
				BaseAddress = new Uri(server)
			};
			client.DefaultRequestHeaders.TryAddWithoutValidation("lang", "en");
			client.DefaultRequestHeaders.TryAddWithoutValidation("sign_method", "HMAC-SHA256");
			client.DefaultRequestHeaders.TryAddWithoutValidation("client_id", clientID);
			string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
			string signUrl = "/v1.0/token?grant_type=1";
			string stringToSign = string.Join("\n", "GET", Engine.GetHash(string.Empty), string.Empty, signUrl);
			string signStr = clientID + timestamp + stringToSign;
			HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, signUrl);
			req.Headers.Add("t", timestamp);
			req.Headers.Add("sign", Engine.Encrypt(signStr, clientSecret));
			HttpResponseMessage resp = client.SendAsync(req).Result;
			string response = resp.Content.ReadAsStringAsync().Result;
			Base getToken = Engine.ToBase(response);
			if (getToken.result == null)
			{
				throw new(response);
			}
			JsonElement element = (JsonElement)getToken.result;
			GetTokenObj? nzx = JsonSerializer.Deserialize<GetTokenObj>(element.GetRawText());
			if (nzx == null || string.IsNullOrEmpty(nzx.access_token) || string.IsNullOrEmpty(nzx.refresh_token))
			{
				throw new(response);
			}
			accessToken = nzx.access_token;
			refreshToken = nzx.refresh_token;
		}
		public Device GetDevice(string deviceID)
		{
			return new Device(this, deviceID);
		}
	}
}