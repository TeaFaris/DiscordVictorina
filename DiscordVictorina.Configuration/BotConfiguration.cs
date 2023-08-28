namespace DiscordVictorina.Configuration
{
	public class BotConfiguration
	{
		public string Token { get; init; } = null!;

		public ulong[] Admins { get; init; } = null!;

		public ulong GuildId { get; init; }

		public ulong ChannelId { get; init; }

		public Victorina Victorina { get; set; } = null!;
	}
}