using Models;

[NotMapped]
public class IdentitetDTO   //mislim da sam napravila dosta ovih i da treba ipak da se nekako spoje u 1 DTO??
{
    public required string Username { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
}

