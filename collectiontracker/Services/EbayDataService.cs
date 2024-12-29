using collectiontracker.Data;
using collectiontracker.DTOs;
using collectiontracker.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

namespace collectiontracker.Services
{
    public class EbayDataService
    {
        private readonly AppDbContext appDbContext;
        private readonly EbayTokenService tokenService;

        public EbayDataService(AppDbContext appDbContext, EbayTokenService tokenService)
        {
            this.appDbContext = appDbContext;
            this.tokenService = tokenService;
        }

        public async Task<ReadFiguresDto> GetEbayItemDetails(string legacyId)
        {
            var token = await tokenService.GetAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://api.ebay.com/buy/browse/v1/item/get_item_by_legacy_id?legacy_item_id={legacyId}");

            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadAsStringAsync();
                Console.WriteLine(item);
                var figure = JsonConvert.DeserializeObject<Figures>(item);

                if (figure == null)
                {
                    return null;
                }

                int seriesId = AssignSeriesId(figure.Name);

                var figureDto = new ReadFiguresDto
                {
                    EbayItemId = figure.EbayItemId,
                    Name = figure.Name,
                    ImageUrl = figure.ImageUrl,
                    EbayPrice = figure.EbayPrice,
                    LastUpdated = figure.LastUpdated,
                    SeriesId = seriesId
                };

                return figureDto;
            }

            return null;
        }

        // only starting with DBZ and Naruto figures for now
        private int AssignSeriesId(string name)
        {
            if (name.Contains("Dragon Ball", StringComparison.OrdinalIgnoreCase))
            {
                return 1; // DBZ
            }
            else if (name.Contains("Naruto", StringComparison.OrdinalIgnoreCase))
            {
                return 2; // Naruto
            }
            return 1;
        }
        public async Task AddEbayItemsToDatabase(List<string> legacyIds)
        {
            var token = await tokenService.GetAccessTokenAsync();
            foreach (var legacyId in legacyIds)
            {
                var figureDto = await GetEbayItemDetails(legacyId);
                if (figureDto != null)
                {
                    var existingFigure = await appDbContext.Figures.FirstOrDefaultAsync(f => f.EbayItemId == figureDto.EbayItemId);

                    if (existingFigure == null)
                    {
                        var newFigure = new Figures
                        {
                            EbayItemId = figureDto.EbayItemId,
                            Name = figureDto.Name,
                            ImageUrl = figureDto.ImageUrl,
                            EbayPrice = figureDto.EbayPrice,
                            LastUpdated = figureDto.LastUpdated,
                            SeriesId = figureDto.SeriesId
                        };

                        appDbContext.Figures.Add(newFigure);
                        await appDbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}

