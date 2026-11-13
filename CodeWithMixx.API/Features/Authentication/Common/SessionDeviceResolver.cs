using UAParser;

namespace CodeWithMixx.API.Features.Authentication.Common;

public static class SessionDeviceResolver
{
    public static SessionDevice Resolve(ClientInfo client)
    {
        var userAgent = client.String;
        var device = client.Device.Family;
        var os = client.OS.Family;

        if (device.Contains("iPad", StringComparison.OrdinalIgnoreCase)
            || device.Contains("Tablet", StringComparison.OrdinalIgnoreCase)
            || device.Contains("Kindle", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("Silk", StringComparison.OrdinalIgnoreCase))
            return SessionDevice.Tablet;

        if (os.Equals("Android", StringComparison.OrdinalIgnoreCase))
            return userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase)
                ? SessionDevice.Mobile
                : SessionDevice.Tablet;

        if (os.Equals("iOS", StringComparison.OrdinalIgnoreCase)
            || os.Contains("Windows Phone", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("Mobi", StringComparison.OrdinalIgnoreCase))
            return SessionDevice.Mobile;

        return SessionDevice.Desktop;
    }
}
