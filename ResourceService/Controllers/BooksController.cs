using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceService.Data;

namespace ResourceService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Все действия требуют валидный JWT-токен в заголовке Authorization: Bearer <token>
public class BooksController : ControllerBase
{
    private readonly BookRepository _bookRepository;

    public BooksController(BookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    // GET /api/books — список книг
    [HttpGet]
    public IActionResult GetBooks()
    {
        var books = _bookRepository.GetAll();
        return Ok(books);
    }

    // GET /api/books/1 — книга по id
    [HttpGet("{id:int}")]
    public IActionResult GetBookById(int id)
    {
        var book = _bookRepository.GetById(id);
        if (book is null)
        {
            return NotFound(new { message = $"Книга с id={id} не найдена" });
        }

        return Ok(book);
    }

    // DELETE /api/books/1 — удаление книги, только для роли Admin (посредством политики)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    //[Authorize(Policy = "CanDeleteBooks")]
    public IActionResult DeleteBook(int id)
    {
        var book = _bookRepository.GetById(id);
        if (book is null)
        {
            return NotFound(new { message = $"Книга с id={id} не найдена" });
        }

        _bookRepository.Delete(id);
        return NoContent();
    }

    // GET /api/books/branch/B001 — книги отдела.
    // Уровень 2: политика на claim'ы — доступна только пользователю с BranchId = "B001" (alice)
    [HttpGet("branch/B001")]
    [Authorize(Policy = "CanViewBranchB001")]
    public IActionResult GetBranchB001Books()
    {
        return Ok(_bookRepository.GetByBranchId("B001"));
    }
}
