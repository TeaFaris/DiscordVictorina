using Discord.Interactions;
using Discord.WebSocket;
using DiscordVictorina.Services;

namespace DiscordVictorina.Handlers
{
	public class InteractionHandler : IInitializeService
	{
		readonly DiscordSocketClient discordClient;
		readonly InteractionService interactionService;
		readonly IServiceProvider serviceProvider;
		public InteractionHandler(
				DiscordSocketClient discordClient,
				InteractionService interactionService,
				IServiceProvider serviceProvider
			)
		{
			this.serviceProvider = serviceProvider;
			this.interactionService = interactionService;
			this.discordClient = discordClient;
		}

		public Task InitializeAsync()
		{
			discordClient.InteractionCreated += InteractionCreated;

			return Task.CompletedTask;
		}

		private async Task InteractionCreated(SocketInteraction arg)
		{
			var ctx = new SocketInteractionContext(discordClient, arg);

			await interactionService.ExecuteCommandAsync(ctx, serviceProvider);
		}
	}
}