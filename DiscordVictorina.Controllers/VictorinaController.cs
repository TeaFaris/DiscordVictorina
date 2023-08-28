using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordVictorina.Configuration;
using DiscordVictorina.Controllers.Models;
using Microsoft.Extensions.Logging;
using OGA.AppSettings.Writeable.JSONConfig;
using System;
using System.Text;

namespace DiscordVictorina.Controllers
{
	public class VictorinaController : InteractionModuleBase<SocketInteractionContext>
	{
		readonly IWritableOptions<BotConfiguration> config;
		readonly DiscordSocketClient discordClient;
		readonly ILogger<VictorinaController> logger;

		public VictorinaController(
				IWritableOptions<BotConfiguration> config,
				DiscordSocketClient discordClient,
				ILogger<VictorinaController> logger
			)
		{
			this.logger = logger;
			this.discordClient = discordClient;
			this.config = config;
			
			discordClient.ModalSubmitted += AnsweredVictorina;
		}


		[SlashCommand("начать", "Начать викторину.")]
		public async Task StartVictorina()
		{
			if (!config.Value.Victorina.Active || config.Value.Victorina.EndTime <= DateTime.UtcNow)
			{
				await RespondAsync("В данный момент никакая викторина не проходит.");
				return;
			}

			if(UserApplication.Applications.Exists(x => x.UserId == Context.User.Id))
			{
				await RespondAsync("Вы уже отправили свои ответы.");
				return;
			}

			var modalBuilder = new ModalBuilder()
				.WithTitle(config.Value.Victorina.Name)
				.WithCustomId(nameof(AnsweredVictorina));

			for (int i = 0; i < config.Value.Victorina.Questions.Count; i++)
			{
				Question question = config.Value.Victorina.Questions[i];

				bool fitInHeader = question.Value.Length <= 45;

				var questionHeader = fitInHeader
					? question.Value
					: question.Value[..42] + "...";

				var questionPlaceholder = fitInHeader
					? "Ваш ответ..."
					: "..." + question.Value[42..];

				modalBuilder.AddTextInput(questionHeader, i.ToString(), style: TextInputStyle.Paragraph, placeholder: questionPlaceholder, minLength: question.MinLength, maxLength: question.MaxLength);
			}

			await RespondWithModalAsync(modalBuilder.Build());
		}

		private async Task AnsweredVictorina(SocketModal arg)
		{
			if(arg.Data.CustomId != nameof(AnsweredVictorina) || arg.HasResponded)
			{
				return;
			}

			var questions = config.Value.Victorina.Questions;
			var answersString = arg.Data.Components.ToArray();

			var answers = new List<QuestionAnswer>();

			for (int i = 0; i < questions.Count; i++)
			{
				var question = questions[i];
				var answerString = answersString[i];

				var answer = new QuestionAnswer
				{
					Question = question,
					Value = answerString.Value
				};

				answers.Add(answer);
			}

			UserApplication newUserApplication = new()
			{
				UserId = arg.User.Id,
				Answers = answers
			};
			UserApplication.Applications.Add(newUserApplication);

			if (config.Value.Victorina.WithScreenshot)
			{
				var embedBuilder = new EmbedBuilder()
					.WithColor(Color.Blue)
					.WithDescription($"""
									 Последний вопрос:
									 {config.Value.Victorina.ScreenshotQuestion!.Value}
									 
									 > Отправьте скриншот командой '/отправить-скрин'.
									 """);

				await arg.RespondAsync(embed: embedBuilder.Build());
				return;
			}

			await SendToChannel(newUserApplication, arg);
		}

		[SlashCommand("отправить-скрин", "Отправить скриншот к ответу.")]
		public async Task SendScreenshot([Summary("Скриншот")] IAttachment screenshot)
		{
			if (!config.Value.Victorina.WithScreenshot)
			{
				await RespondAsync("Для этой викторины не требуется прикрипление скриншота.");
				return;
			}

			var userApplication = UserApplication.Applications.Find(x => x.UserId == Context.User.Id);

			if (userApplication is null)
			{
				await RespondAsync("Вы не отправили ответы. Напишите '/начать', чтобы начать викторину.");
				return;
			}

			if(screenshot.ContentType is not "image/png" or not "image/jpeg")
			{
				await RespondAsync("Отправьте картинку формата .png, .jpg или .jpeg");
				return;
			}

			userApplication.Screenshot = screenshot;

			await SendToChannel(userApplication);
		}

		private async Task SendToChannel(UserApplication application, IDiscordInteraction? socketModal = null)
		{
			var user = await discordClient.GetUserAsync(application.UserId);

			var discriminator = user.Discriminator != "0000"
				? $"#{user.Discriminator}"
				: string.Empty;

			var questionsStringBuilder = new StringBuilder();

			for (int i = 0; i < application.Answers.Count; i++)
			{
				QuestionAnswer answer = application.Answers[i];

				questionsStringBuilder
					.Append(i + 1)
					.Append(". ")
					.AppendLine(answer.Question.Value);

				questionsStringBuilder
					.Append("> ")
					.AppendLine(answer.Value);
			}

			var embedBuilder = new EmbedBuilder()
				.WithColor(Color.Blue)
				.WithThumbnailUrl(user.GetAvatarUrl());

			if(application.Screenshot is not null)
			{
				questionsStringBuilder
					.Append(application.Answers.Count + 1)
					.Append(". ")
					.AppendLine(config.Value.Victorina.ScreenshotQuestion!.Value);
				embedBuilder.WithImageUrl(application.Screenshot.Url);
			}

			embedBuilder.WithDescription($"""
										 `Участник [тэгом]`
										 {user.Mention}
										 `Участник [текстом]`
										 {user.Username}{discriminator}
										 
										 Ответы:
										 {questionsStringBuilder}
										 """);

			var guild = discordClient.GetGuild(config.Value.GuildId);
			var textChannel = guild.GetTextChannel(config.Value.ChannelId);

			await textChannel.SendMessageAsync(embed: embedBuilder.Build());

			if (socketModal is not null)
			{
				await socketModal.RespondAsync($"Ваши ответы успешно опубликованы в ветку: <#{config.Value.ChannelId}>.");
				return;
			}

			await RespondAsync($"Ваши ответы успешно опубликованы в ветку: <#{config.Value.ChannelId}>.");
		}
	}
}