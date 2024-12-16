using collectiontracker.Data;
using collectiontracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<Figures>>> GetAllFigures()
        {
            var figures = await appDbContext.Figures.ToListAsync();
            return Ok(figures);
        }
    }
}
