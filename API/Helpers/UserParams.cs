namespace API.Helpers
{
    public class UserParams : PaginationParams
    {
        public string CurrentUsername { get; set; }
        public int CurrentUserId { get; set; }
        public string Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 100;
        public string OrderBy { get; set; } = "lastActive";
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }


        private const double MinDistance = 0.0;
        private const double MaxDistance = 100.0;
        private double _distance = 100.0;
        public double Distance // Max Distance to search in km
        {
            get => _distance;
            set
            {
                if (value < MinDistance)
                {
                    _distance = MinDistance;
                }
                else if (value > MaxDistance)
                {
                    _distance = MaxDistance;
                }
                else
                {
                    _distance = value;
                }
            }
        }

        private const int MinSimilarity = 0; // Giới hạn dưới
        private const int MaxSimilarity = 10; // Giới hạn trên
        private int _similarity = 2;
        public int Similarity
        {
            get => _similarity;
            set
            {
                if (value < MinSimilarity)
                {
                    _similarity = MinSimilarity;
                }
                else if (value > MaxSimilarity)
                {
                    _similarity = MaxSimilarity;
                }
                else
                {
                    _similarity = value;
                }
            }
        }

    }
}