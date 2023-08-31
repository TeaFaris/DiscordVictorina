namespace DiscordVictorina.Configuration
{
	public class Question
	{
		public string Value { get; init; } = null!;

		public int MinLength { get; init; }

		public int MaxLength { get; init; }
	}
}