namespace API.Helpers
{
    public class UserParams : PaginationParams
    {
        public string CurrentUsername { get; set; }
        public string Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 100;
        public string OrderBy { get; set; } = "lastActive";
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        private const double MaxDistance = 100;
        private double _distance = 100;
        public double Distance //Max Distance to search in km
        {
            get => _distance;
            set => _distance = (value > MaxDistance) ? MaxDistance : value;
        }
    }
}