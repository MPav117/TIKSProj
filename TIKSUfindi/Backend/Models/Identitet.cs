namespace Models;
public class Identitet
{
    [Key]
    public int ID { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Username { get; set; }
    public required string PasswordHash { get; set; } //zapravo ovo sacuvaj u bazi, a ne pravi pass
    public required string Tip { get; set; }
}