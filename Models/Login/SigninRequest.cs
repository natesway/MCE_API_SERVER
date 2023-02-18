using System;

namespace MCE_API_SERVER.Models.Login
{
    public class Coordinate
    {
        public double latitude { get; set; }
        public double longitude { get; set; }

        public static Coordinate operator -(Coordinate a, Coordinate b)
            => new Coordinate() { latitude = a.latitude - b.latitude, longitude = a.longitude - b.longitude };
        public static Coordinate operator /(Coordinate a, Coordinate b)
            => new Coordinate() { latitude = a.latitude / b.latitude, longitude = a.longitude / b.longitude };
        public static Coordinate operator /(Coordinate a, double b)
            => new Coordinate() { latitude = a.latitude / b, longitude = a.longitude / b };

        public double Lenght()
            => Math.Sqrt(latitude * latitude + longitude * longitude);
    }

    public class SigninRequest
    {
        public string advertisingId { get; set; }
        public string appsFlyerId { get; set; }
        public string buildNumber { get; set; }
        public string clientVersion { get; set; }
        public Coordinate coordinate { get; set; }
        public string deviceId { get; set; }
        public string deviceOS { get; set; }
        public string deviceToken { get; set; }
        public string language { get; set; }
        public string sessionTicket { get; set; }
        public object streams { get; set; }
    }
}
