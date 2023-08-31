using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace DiscordVictorina.Services.SlashCommandsRegisterServices
{
	public class SlashCommandsRegisterService : IInitializeService
	{
		readonly InteractionService interactionService;
		readonly ILogger<SlashCommandsRegisterService> logger;
		readonly DiscordSocketClient discordClient;
		public SlashCommandsRegisterService(
				DiscordSocketClient discordClient,
				InteractionService interactionService,
				ILogger<SlashCommandsRegisterService> logger
			)
		{
			this.discordClient = discordClient;
			this.logger = logger;
			this.interactionService = interactionService;
		}

		public Task InitializeAsync()
		{
			discordClient.Ready += ClientReady;
			return Task.CompletedTask;
		}

		private async Task ClientReady()
		{
			try
			{
				await interactionService.RegisterCommandsGloballyAsync(true);

				logger.LogInformation("Slash commands was successfully registered.");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error occurred during register slash commands.");
			}
		}
	}
}
