namespace SqlInjectionPresenter.Api.Models;

// Tablica koja simulira "sigurno" pohranjene podatke koji će se naknadno koristiti nesigurno.
// Ključna za demonstraciju second-order SQL injection napada.
public class StoredProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
