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

    public class ReadersController : Controller
    {
        readonly TestApiDB _context;
        public ReadersController(TestApiDB context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("getAllReaders")]
        public async Task<IActionResult> GetReaders()
        {
            var readers = await _context.Readers.ToListAsync();
            var readersDto = readers.Select(b => new GetAllReaders
            {
                Id_Readers = b.Id_Readers,
                FName = b.FName,
                LName = b.LName,
                DateOfBirth = b.DateOfBirth,
                ContactDetails = b.ContactDetails
            });
            return Ok(readersDto);
        }

        [HttpPost]
        [Route("CreateNewReaders")]
        public async Task<IActionResult> CreateNewReaders([FromQuery] CreateNewReaders newReaders)
        {
            try
            {
                var readers = new Readers()
            {
                FName = newReaders.FName,
                LName = newReaders.FName,
                DateOfBirth = newReaders.DateOfBirth,
                ContactDetails = newReaders.ContactDetails
            };
            await _context.Readers.AddAsync(readers);
            await _context.SaveChangesAsync();
            return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReaders(int id, [FromQuery] CreateNewReaders updateReaders)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления читателя.");
            }
            try
            {
                var readers = await _context.Readers.FindAsync(id);
                if (readers == null)
                {
                    return NotFound("Читатель с указанным идентификатором не найдена.");
                }

                readers.FName = updateReaders.FName;
                readers.LName = updateReaders.LName;

                _context.Readers.Update(readers);
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
                return BadRequest("Некорректные данные для обновления читателя.");
            }
            try
            {
                var readers = await _context.Readers.FindAsync(id);
                if (readers == null)
                {
                    return NotFound("Читатель с указанным идентификатором не найдена.");
                }

                _context.Readers.Remove(readers);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Строка обязательна для поиска.");
            }
            try
            {
                var readers = await _context.Readers
                    .Where(b => b.FName.Contains(query) || b.LName.Contains(query))
                    .ToListAsync();
                var readersDto = readers.Select(b => new GetAllReaders
                {
                    Id_Readers = b.Id_Readers,
                    FName = b.FName,
                    LName = b.LName,
                    DateOfBirth = b.DateOfBirth,
                    ContactDetails = b.ContactDetails
                });
                if (readers == null)
                {
                    return NotFound("Читатель с указанным идентификатором не найден.");
                }
                return Ok(readersDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}
