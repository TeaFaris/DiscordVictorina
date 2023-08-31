using DiscordVictorina.Configuration;

namespace DiscordVictorina.Controllers.Models
{
    public class QuestionAnswer
    {
        public Question Question { get; init; } = null!;

        public string Value { get; init; } = null!;
    }
}
