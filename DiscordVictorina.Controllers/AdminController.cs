using Discord;
using Discord.Interactions;
using DiscordVictorina.Configuration;
using OGA.AppSettings.Writeable.JSONConfig;

namespace DiscordVictorina.Controllers
{
	public class AdminController : InteractionModuleBase<SocketInteractionContext>
	{
		readonly IWritableOptions<BotConfiguration> config;
		
		public AdminController(IWritableOptions<BotConfiguration> config)
		{
			this.config = config;
		}

		private async Task<bool> AdminCheck()
		{
			if (!config.Value.Admins.Contains(Context.User.Id))
			{
				await RespondAsync("У Вас нет доступа к этой команде.");
				return false;
			}

			return true;
		}
	}
}
