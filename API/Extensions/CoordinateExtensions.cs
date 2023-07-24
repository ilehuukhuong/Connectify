namespace API.Extensions
{
    public static class CoordinateExtensions
    {
        private const double EarthRadiusKm = 6371.0; // Radius of the Earth in kilometers

        public static double ToRadians(this double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double CalculateDistance(this (double latitude, double longitude) coordinate1, (double latitude, double longitude) coordinate2)
        {
            double lat1Rad = coordinate1.latitude.ToRadians();
            double lon1Rad = coordinate1.longitude.ToRadians();
            double lat2Rad = coordinate2.latitude.ToRadians();
            double lon2Rad = coordinate2.longitude.ToRadians();

            double dLat = lat2Rad - lat1Rad;
            double dLon = lon2Rad - lon1Rad;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = EarthRadiusKm * c;
            return distance;
        }
    }
}