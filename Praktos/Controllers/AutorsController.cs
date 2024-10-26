using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktos.DatabaseContext;
using Praktos.Model;
using Praktos.Requests;
using static System.Reflection.Metadata.BlobBuilder;

namespace Praktos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutorsController : Controller
    {
        readonly TestApiDB _context;
        public AutorsController(TestApiDB context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("getAllAutors")]
        public async Task<IActionResult> GetAutors()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления книги.");
            }
            try
            {
                var autors = await _context.Autors.ToListAsync();
                var autorsDto = autors.Select(b => new GetAllAutors
                {
                    Id_Autors = b.Id_Autors,
                    FName = b.FName,
                    LName = b.LName
                });
                return Ok(autorsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("CreateNewAutors")]
        public async Task<IActionResult> CreateNewAutors([FromQuery] CreateNewAutors newAutors)
        {
            var autors = new Autors()
            {
                FName = newAutors.FirstName,
                LName = newAutors.LastName,
            };
            await _context.Autors.AddAsync(autors);
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAutors(int id, [FromQuery] CreateNewAutors updateAutors)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления автора.");
            }
            try
            {
            var autors = await _context.Autors.FindAsync(id);
            if (autors == null)
            {
                return NotFound("Автор с указанным идентификатором не найдена.");
            }

            autors.FName = updateAutors.FirstName;
            autors.LName = updateAutors.LastName;

            _context.Autors.Update(autors);
            await _context.SaveChangesAsync();

            return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAutors(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления автора.");
            }
            try
            {
                var autors = await _context.Autors.FindAsync(id);
            if (autors == null)
            {
                return NotFound("Автор с указанным идентификатором не найдена.");
            }

            _context.Autors.Remove(autors);
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
