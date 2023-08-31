namespace DiscordVictorina.Configuration
{
	public class Victorina
	{
		public string Name { get; set; } = null!;

		public DateTimeOffset EndTime { get; set; }

		public List<Question> Questions { get; set; } = null!;

		public bool WithScreenshot { get; set; }

		public Question? ScreenshotQuestion { get; set; }

		public bool Active { get; set; }
	}
}
