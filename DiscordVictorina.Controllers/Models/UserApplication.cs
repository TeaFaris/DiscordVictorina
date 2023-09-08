using Discord;

namespace DiscordVictorina.Controllers.Models
{
	public class UserApplication
	{
		public static readonly List<UserApplication> Applications = new();

		public ulong UserId { get; init; }

		public List<QuestionAnswer> Answers { get; init; } = null!;

		public IAttachment? Screenshot { get; set; }

		public DateTime PublishDate { get; set; }

		public ulong? PostedMessageId { get; set; }
	}
}
