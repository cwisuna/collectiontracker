using collectiontracker.Data;
using collectiontracker.Models;
using collectiontracker.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using collectiontracker.Services;

namespace collectiontracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiguresController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private readonly EbayDataService ebayDataService;

        public FiguresController(AppDbContext appDbContext, EbayDataService ebayDataService)
        {
            this.appDbContext = appDbContext;
            this.ebayDataService = ebayDataService;
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "GetAllFigures")]
        public async Task<ActionResult<IEnumerable<ReadFiguresDto>>> GetAllFigures()
        {
            var figures = await appDbContext.Figures.ToListAsync();

            var figuresDto = figures.Select(fig => new ReadFiguresDto
            {
                Id = fig.Id,
                EbayItemId = fig.EbayItemId,
                Name = fig.Name,
                ImageUrl = fig.ImageUrl,
                EbayPrice = fig.EbayPrice,
                LastUpdated = fig.LastUpdated,
                SeriesId = fig.SeriesId
            }).ToList();

            return Ok(figuresDto);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(OperationId = "GetFigureById")]
        public async Task<ActionResult<ReadFiguresDto>> GetFigure(int id)
        {
            var figure = await appDbContext.Figures.FindAsync(id);

            if (figure == null)
            {
                return NotFound();
            }

            var figureDto = new ReadFiguresDto
            {
                Id = figure.Id,
                EbayItemId = figure.EbayItemId,
                Name = figure.Name,
                ImageUrl = figure.ImageUrl,
                EbayPrice = figure.EbayPrice,
                LastUpdated = figure.LastUpdated,
                SeriesId = figure.SeriesId
            };

            return Ok(figureDto);
        }

        [HttpGet("series/{seriesId}")]
        [SwaggerOperation(OperationId = "GetFiguresBySeries")]
        public async Task<ActionResult<IEnumerable<ReadFiguresDto>>> GetAllFiguresBySeries(int seriesId)
        {
            var figures = await appDbContext.Figures.Where(fig => fig.SeriesId == seriesId).ToListAsync();

            var figuresDto = figures.Select(fig => new ReadFiguresDto
            {
                Id = fig.Id,
                EbayItemId = fig.EbayItemId,
                Name = fig.Name,
                ImageUrl = fig.ImageUrl,
                EbayPrice = fig.EbayPrice,
                LastUpdated = fig.LastUpdated,
                SeriesId = fig.SeriesId
            }).ToList();

            if (!figuresDto.Any())
            {
                return NotFound();
            }

            return Ok(figuresDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "AddFigure")]
        public async Task<ActionResult<Figures>> AddFigure(CreateFigureDto figureDto)
        {
            if (figureDto == null)
            {
                return BadRequest();
            }

            if (figureDto.SeriesId != 1 && figureDto.SeriesId != 2)
            {
                return BadRequest("Invalid SeriesId. Only DBZ (1) or Naruto (2) are allowed.");
            }

            var newFigure = new Figures
            {
                Name = figureDto.Name,
                ImageUrl = figureDto.ImageUrl,
                EbayPrice = figureDto.EbayPrice,
                LastUpdated = DateTime.UtcNow,
                SeriesId = figureDto.SeriesId
            };

            appDbContext.Figures.Add(newFigure);

            await appDbContext.SaveChangesAsync();

            return Ok(newFigure);  
        }

        [HttpPost("from-ebay")]
        //Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "AddFigureFromEbay")]
        public async Task<IActionResult> AddFigureFromEbay([FromBody] AddFigureFromEbayDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.LegacyId))
            {
                return BadRequest("Invalid request. LegacyId and SeriesId are required.");
            }

            if (request.SeriesId != 1 && request.SeriesId != 2)
            {
                return BadRequest("Invalid SeriesId. Only DBZ (1) or Naruto (2) are allowed.");
            }

            var figureDto = await ebayDataService.GetEbayItemDetails(request.LegacyId);
            if (figureDto == null)
            {
                return NotFound($"No item found on eBay with LegacyId: {request.LegacyId}");
            }

            figureDto.SeriesId = request.SeriesId;

            var existingFigure = await appDbContext.Figures
                .FirstOrDefaultAsync(f => f.EbayItemId == figureDto.EbayItemId);
            if (existingFigure != null)
            {
                return Conflict("Figure already exists in the database.");
            }

            var newFigure = new Figures
            {
                EbayItemId = figureDto.EbayItemId,
                Name = figureDto.Name,
                ImageUrl = figureDto.ImageUrl,
                EbayPrice = figureDto.EbayPrice,
                LastUpdated = DateTime.UtcNow,
                SeriesId = figureDto.SeriesId
            };

            appDbContext.Figures.Add(newFigure);
            await appDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFigure), new { id = newFigure.Id }, newFigure);
        }

        /*[HttpPost("add-from-ebay")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "AddFiguresFromEbay")]
        public async Task<ActionResult> AddFiguresFromEbay([FromBody] List<string> legacyIds)
        {
            if (legacyIds == null || legacyIds.Count == 0)
            {
                return BadRequest("No legacy IDs provided.");
            }

            await ebayDataService.AddEbayItemsToDatabase(legacyIds);

            return Ok("Figures added from eBay successfully.");
        }*/
    }
}
