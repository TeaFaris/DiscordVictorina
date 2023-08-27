using Discord.Interactions;
using DiscordVictorina.Controllers;
using DiscordVictorina.Services;
using Microsoft.Extensions.Logging;

namespace DiscordVictorina.Handlers
{
	public class SlashCommandHandler : IInitializeService
	{
		readonly ILogger<SlashCommandHandler> logger;
		readonly InteractionService interactionService;
		readonly IServiceProvider serviceProvider;

		public SlashCommandHandler(
				InteractionService interactionService,
				ILogger<SlashCommandHandler> logger,
				IServiceProvider serviceProvider
			)
		{
			this.serviceProvider = serviceProvider;
			this.interactionService = interactionService;
			this.logger = logger;
		}

		public async Task InitializeAsync()
		{
			interactionService.InteractionExecuted += InteractionExecuted;

			await interactionService.AddModulesAsync(typeof(VictorinaController).Assembly, serviceProvider);
		}

		private Task InteractionExecuted(ICommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
		{
			if (!arg3.IsSuccess)
			{
				logger.LogError("Error occurred during interaction execution: {err}", arg3.ErrorReason);
			}

			return Task.CompletedTask;
		}
	}
}
