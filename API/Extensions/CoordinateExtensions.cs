namespace API.Extensions
{
    public static class CoordinateExtensions
    {
        private const double EarthRadiusKm = 6371.0; // Radius of the Earth in kilometers

        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = EarthRadiusKm * c;
            return distance;
        }
    }
}