using Microsoft.AspNetCore.Mvc;

namespace PEPScanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SarController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(new List<object>());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            return NotFound();
        }

        [HttpPost]
        public IActionResult Create([FromBody] object request)
        {
            return Ok(new { id = Guid.NewGuid(), message = "SAR created" });
        }
    }
}