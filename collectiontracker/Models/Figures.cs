using Newtonsoft.Json;

namespace collectiontracker.Models
{
    public class Figures
    {
        public int Id { get; set; }
        [JsonProperty("itemId")]
        public string EbayItemId { get; set; }
        [JsonProperty("title")]
        public string Name { get; set; }
        [JsonProperty("image.imageUrl")]
        public string ImageUrl { get; set; }
        [JsonProperty("price.value")]
        public decimal EbayPrice { get; set; }
        public DateTime LastUpdated { get; set; }
        public int SeriesId { get; set; }
    }
}
