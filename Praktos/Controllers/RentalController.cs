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
    public class BookRentalController : ControllerBase
    {
        private readonly TestApiDB _context;

        public BookRentalController(TestApiDB context)
        {
            _context = context;
        }

        [HttpPost("rent")]
        public async Task<IActionResult> RentBook([FromQuery] CreateNewRental request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var book = await _context.Books.FindAsync(request.id_book);
                if (book == null)
                {
                    return NotFound("Книга не найдена.");
                }

                if (book.AvailableCopies <= 0)
                {
                    return BadRequest("Нет доступных копий для аренды.");
                }
                var reader = await _context.Readers
                .FirstOrDefaultAsync(g => g.Id_Readers == request.id_readers);
                var rental = new BookRentalHistory
                {
                    Id_Books = request.id_book,
                    Id_Readers = reader.Id_Readers,
                    RentDate = DateTime.UtcNow,
                    DueDate = request.DueDate
                };

                book.AvailableCopies--;

                _context.BookRentalsHistory.Add(rental);
                await _context.SaveChangesAsync();

                return Ok("Книга арендована.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromQuery] CreateNewReturn request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var rental = await _context.BookRentalsHistory.FindAsync(request.id_Rental);
                if (rental == null)
                {
                    return NotFound("Аренда не найдена.");
                }

                var book = await _context.Books.FindAsync(rental.Id_Books);
                if (book == null)
                {
                    return NotFound("Книга не найдена.");
                }

                rental.ReturnDate = DateTime.UtcNow;
                book.AvailableCopies++;

                await _context.SaveChangesAsync();

                return Ok("Книга возвращена.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("user/{id_readers}/history")]
        public async Task<IActionResult> GetRentalHistoryByUser(int id_readers)
        {
            try
            {
                var rentals = await _context.BookRentalsHistory
                    .Where(r => r.Id_Readers == id_readers)
                    .Include(r => r.Books)
                    .Include(r => r.Readers)
                    .Select(r => new
                    {
                        Title = r.Books.Title,
                        Reader = r.Readers.FName + " " + r.Readers.LName,
                        RentalDate = r.RentDate,
                        DueDate = r.DueDate,
                        ReturnDate = r.ReturnDate
                    })
                    .ToListAsync();

                if (rentals == null || rentals.Count == 0)
                {
                    return NotFound("История аренды не найдена.");
                }

                return Ok(rentals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentRentals()
        {
            try
            {
                var rentals = await _context.BookRentalsHistory
                    .Where(r => r.ReturnDate == null)
                    .Include(r => r.Books)
                    .Include(r => r.Readers)
                    .Select(r => new
                    {
                        Title = r.Books.Title,
                        Reader = r.Readers.FName + " " + r.Readers.LName,
                        RentalDate = r.RentDate,
                        DueDate = r.DueDate
                    })
                    .ToListAsync();

                return Ok(rentals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("book/{id_book}/history")]
        public async Task<IActionResult> GetRentalHistoryByBook(int id_book)
        {
            try
            {
                var rentals = await _context.BookRentalsHistory
                    .Where(r => r.Id_Books == id_book)
                    .Include(r => r.Books)
                    .Include(r => r.Readers)
                    .Select(r => new
                    {
                        Title = r.Books.Title,
                        Reader = r.Readers.FName + " " + r.Readers.LName,
                        RentalDate = r.RentDate,
                        DueDate = r.DueDate,
                        Return = r.ReturnDate
                    })
                    .ToListAsync();

                if (rentals == null || rentals.Count == 0)
                {
                    return NotFound("История аренды для книги не найдена.");
                }

                return Ok(rentals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }
    }
}
