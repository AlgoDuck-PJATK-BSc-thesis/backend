namespace AlgoDuck.Modules.User.Shared.DTOs;

public sealed class Reminder
{
    public string Day { get; set; } = "Mon";
    public bool Enabled { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
}
