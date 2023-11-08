# Tuya API
With this library, you can control your light easily in C#.

## Login to Tuya account
```cs
Profile me = new("clietID", "clientSecret");
```
## Get specific lamp from your profile
```cs
Device lamp = me.GetDevice("deviceID");
```
## Turn your light on or off
```cs
// true for on and false for off
lamp.SwitchLED(true);
```