namespace collectiontracker.Models
{
    public class Figures
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal EbayPrice { get; set; }
        public DateTime LastUpdated { get; set; }
        public int SeriesId { get; set; }

    }
}
