using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktos.DatabaseContext;
using Praktos.Model;
using Praktos.Requests;

namespace Praktos.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : Controller
    {
        readonly TestApiDB _context;
        public GenresController(TestApiDB context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("getAllGenres")]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _context.Genres.ToListAsync();
            var genresDto = genres.Select(g => new GetAllGenres
            {
                Id_Genres = g.Id_Genres,
                Title = g.Title,
            });
            return Ok(genresDto);
        }

        [HttpPost]
        [Route("CreateNewGenres")]
        public async Task<IActionResult> CreateNewGenres([FromQuery] CreateNewGenres newGenres)
        {
            var genres = new Genres()
            {
                Title = newGenres.Title,
            };
            await _context.Genres.AddAsync(genres);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenres(int id, [FromQuery] CreateNewGenres updateGenre)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления книги.");
            }
            try
            {
            var genres = await _context.Genres.FindAsync(id);
            if (genres == null)
            {
                return NotFound("Книга с указанным идентификатором не найдена.");
            }

            genres.Title = updateGenre.Title;

            _context.Genres.Update(genres);
            await _context.SaveChangesAsync();

            return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenres(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления книги.");
            }
            try
            {
                var genres = await _context.Genres.FindAsync(id);
                if (genres == null)
                {
                    return NotFound("Книга с указанным идентификатором не найдена.");
                }

                _context.Genres.Remove(genres);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}
