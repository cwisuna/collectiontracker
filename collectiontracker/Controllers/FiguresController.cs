using collectiontracker.Data;
using collectiontracker.Models;
using collectiontracker.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace collectiontracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiguresController : ControllerBase
    {
        private readonly AppDbContext appDbContext;

        public FiguresController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "GetAllFigures")]
        public async Task<ActionResult<IEnumerable<ReadFiguresDto>>> GetAllFigures()
        {
            var figures = await appDbContext.Figures.ToListAsync();

            var figuresDto = figures.Select(fig => new ReadFiguresDto
            {
                Id = fig.Id,
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
        [SwaggerOperation(OperationId = "AddFigure")]
        public async Task<ActionResult<Figures>> AddFigure(CreateFigureDto figureDto)
        {
            if (figureDto == null)
            {
                return BadRequest();
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
    }
}
