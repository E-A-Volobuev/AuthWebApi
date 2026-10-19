namespace ResourceService.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string BranchId { get; set; } = string.Empty; // Отдел, которому принадлежит книга
}
