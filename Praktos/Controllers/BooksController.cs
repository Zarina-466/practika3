using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktos.DatabaseContext;
using Praktos.Model;
using Praktos.Requests;
using System.ComponentModel.DataAnnotations;
using static System.Reflection.Metadata.BlobBuilder;

namespace Praktos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : Controller
    {
        readonly TestApiDB _context;
       public BooksController(TestApiDB context) 
        {
            _context = context;
        }
        [HttpGet]
        [Route("getAllBooks")]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _context.Books.Include(b=>b.Autors).Include(b=>b.Genres).ToListAsync();
            var booksDto = books.Select(b => new GetAllBooksName
            {
                Id_Books = b.Id_Books,
                Title = b.Title,
                Autor = b.Autors.FName+" "+b.Autors.LName,
                Genre = b.Genres.Title,
                AvailableCopies = b.AvailableCopies,
                YearOfPublication = b.YearOfPublication,
                Description = b.Description
            });
            return Ok(booksDto);
        }

        [HttpPost]
        [Route("CreateNewBooks")]
        public async Task<IActionResult> CreateNewBooks([FromQuery] CreateNewBooks newBooks)
        {
            try
            {
                var books = new Books()
                {
                    Title = newBooks.Title,
                    Id_Autors = newBooks.Id_Autors,
                    Id_Genre = newBooks.Id_Genre,
                    AvailableCopies = newBooks.AvailableCopies,
                    YearOfPublication = newBooks.YearOfPublication,
                    Description = newBooks.Description,
                };
                await _context.Books.AddAsync(books);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления книги.");
            }
            try
            {
                var books = await _context.Books.FindAsync(id);
                if (books == null)
                {
                    return NotFound("Книга с указанным идентификатором не найдена.");
                }
                var booksDto = new GetAllBooksId
                {
                    Id_Books = books.Id_Books,
                    Title = books.Title,
                    Id_Autors = books.Id_Autors,
                    Id_Genre = books.Id_Genre,
                    AvailableCopies = books.AvailableCopies,
                    YearOfPublication = books.YearOfPublication,
                    Description = books.Description
                };
                return Ok(booksDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromQuery] CreateNewBooks updateBooks)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления книги.");
            }
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound("Книга с указанным идентификатором не найдена.");
                }

                book.Title = updateBooks.Title;
                book.YearOfPublication = updateBooks.YearOfPublication;
                book.Description = updateBooks.Description;
                book.AvailableCopies = updateBooks.AvailableCopies;
                book.Id_Genre = updateBooks.Id_Genre;
                book.Id_Autors = updateBooks.Id_Autors;

                _context.Books.Update(book);
                await _context.SaveChangesAsync();

            return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления книги.");
            }
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return NotFound("Книга с указанным идентификатором не найдена.");
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("byGenre/{genreId}")]
        public async Task<IActionResult> GetBooksByGenre(int genreId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для обновления книги.");
            }
            try
            {
                var books = await _context.Books
                .Where(b => b.Id_Genre == genreId)
                .ToListAsync();
                if (books == null)
                {
                    return NotFound("Книга с указанным идентификатором не найдена.");
                }
                var booksDto = books.Select(b => new GetAllBooksId
            {
                Id_Books = b.Id_Books,
                Title = b.Title,
                Id_Autors = b.Id_Autors,
                Id_Genre = b.Id_Genre,
                AvailableCopies = b.AvailableCopies,
                YearOfPublication = b.YearOfPublication,
                Description = b.Description
            });

            return Ok(booksDto);
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
                var books = await _context.Books.Include(b => b.Autors).Include(b => b.Genres)
                    .Where(b => b.Title.Contains(query) || b.Autors.FName.Contains(query) || b.Autors.LName.Contains(query) || b.Genres.Title.Contains(query))
                    .ToListAsync();
                var booksDto = books.Select(b => new GetAllBooksName
                {
                    Id_Books = b.Id_Books,
                    Title = b.Title,
                    Autor = b.Autors.FName + " " + b.Autors.LName,
                    Genre = b.Genres.Title,
                    AvailableCopies = b.AvailableCopies,
                    YearOfPublication = b.YearOfPublication,
                    Description = b.Description
                });
                if (books == null)
                {
                    return NotFound("Книга с указанным идентификатором не найдена.");
                }
                return Ok(booksDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("{title}/available-copies")]
        public async Task<IActionResult> GetAvailableCopies([Required][FromRoute] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Название книги обязательно для поиска.");
            }
            try
            {
                var books = await _context.Books
                .Where(b => b.Title.Contains(title))
                .Select(b => new BookAvailableCopies
                {
                    Id_Books = b.Id_Books,
                    Title = b.Title,
                    AvailableCopies = b.AvailableCopies
                })
                .ToListAsync();

                if (books == null || books.Count == 0)
                {
                    return NotFound("Книги с указанным названием не найдены.");
                }

                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}
