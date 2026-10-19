using ResourceService.Models;

namespace ResourceService.Data;

// Демо-репозиторий книг в памяти. В реальном приложении — БД
public class BookRepository
{
    private static readonly List<Book> _books = new()
    {
        new Book { Id = 1, Title = "Мастер и Маргарита", Author = "М. Булгаков", Year = 1967, Price = 599m, BranchId = "B001" },
        new Book { Id = 2, Title = "Преступление и наказание", Author = "Ф. Достоевский", Year = 1866, Price = 449m, BranchId = "B001" },
        new Book { Id = 3, Title = "Война и мир", Author = "Л. Толстой", Year = 1869, Price = 1299m, BranchId = "B002" },
        new Book { Id = 4, Title = "Евгений Онегин", Author = "А. Пушкин", Year = 1833, Price = 399m, BranchId = "B002" },
        new Book { Id = 5, Title = "Мёртвые души", Author = "Н. Гоголь", Year = 1842, Price = 479m, BranchId = "HQ" }
    };

    public IReadOnlyList<Book> GetAll() => _books;

    public IReadOnlyList<Book> GetByBranchId(string branchId) =>
        _books.Where(b => b.BranchId == branchId).ToList();

    public Book? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

    public void Delete(int id) => _books.RemoveAll(b => b.Id == id);
}
