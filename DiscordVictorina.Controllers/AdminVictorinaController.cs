using Discord;
using Discord.Interactions;
using DiscordVictorina.Configuration;
using OGA.AppSettings.Writeable.JSONConfig;

namespace DiscordVictorina.Controllers
{
	public class AdminInteractionsController : InteractionModuleBase<SocketInteractionContext>
	{
		readonly IWritableOptions<BotConfiguration> config;

		public AdminInteractionsController(IWritableOptions<BotConfiguration> config)
		{
			this.config = config;
		}

		[ComponentInteraction(nameof(SelectedQuestionToRemove), ignoreGroupNames: true)]
		public async Task SelectedQuestionToRemove(string[] selectedQuestionsIndexes)
		{
			if (!await AdminCheck())
			{
				return;
			}

			var selectedQuestionIndex = int.Parse(selectedQuestionsIndexes[0]);

			config.Update(x =>
			{
				x.Victorina.Questions[selectedQuestionIndex] = new Question
				{
					Value = string.Empty,
					MaxLength = 0,
					MinLength = 0
				};
			});

			await RespondAsync("Успешно удалили вопрос.");
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

	[Group("викторина", "Настройка викторины.")]
	public class AdminVictorinaController : InteractionModuleBase<SocketInteractionContext>
	{
		readonly IWritableOptions<BotConfiguration> config;

		public AdminVictorinaController(IWritableOptions<BotConfiguration> config)
		{
			this.config = config;
		}

		[SlashCommand("канал", "Меняет канал где будет проводиться викторина.")]
		public async Task ChangeChannel([Summary("Канал")] ITextChannel channel)
		{
			if (!await AdminCheck())
			{
				return;
			}

			config.Update(x => x.ChannelId = channel.Id);

			await RespondAsync($"Успешно изменено на канал {channel.Mention}.");
		}

		[SlashCommand("название", "Меняет название викторины.")]
		public async Task ChangeVictorinaName([MaxLength(100)][Summary("Название")] string name)
		{
			if (!await AdminCheck())
			{
				return;
			}

			config.Update(x => x.Victorina.Name = name);

			await RespondAsync($"Успешно изменено название на '{name}'.");
		}

		[SlashCommand("дата-окончания", "Меняет дату окончания")]
		public async Task ChangeEndTime([Summary("Дата", "Дата в UTC, формата `ММ.ДД.ГГГГ чч:мм`")] DateTime date)
		{
			if (!await AdminCheck())
			{
				return;
			}

			config.Update(x => x.Victorina.Active = false);
			config.Update(x => x.Victorina.EndTime = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0, 0, TimeSpan.Zero));

			await RespondAsync($"Успешно изменено дату окончания на {date:dd.MM.yyyy HH:mm}.");
		}

		[Group("вопрос", "Настроить вопросы.")]
		public class QuestionsSubController : InteractionModuleBase<SocketInteractionContext>
		{
			readonly IWritableOptions<BotConfiguration> config;

			public QuestionsSubController(IWritableOptions<BotConfiguration> config)
			{
				this.config = config;
			}

			[SlashCommand("добавить", "Добавляет вопрос.")]
			public async Task Add(
					[Summary("Вопрос")] string question,
					[Summary("Макс-длина-ответа")] int maxLength = 500,
					[Summary("Мин-длина-ответа")] int minLength = 1
				)
			{
				if (!await AdminCheck())
				{
					return;
				}

				config.Update(x =>
				{
					x.Victorina.Questions ??= new List<Question>();

					x.Victorina.Questions.Add(new Question
					{
						Value = question,
						MaxLength = maxLength,
						MinLength = minLength,
					});
				});

				await RespondAsync("Успешно добавили вопрос.");
			}

			[SlashCommand("удалить", "Удаляет вопрос.")]
			public async Task Remove()
			{
				if (!await AdminCheck())
				{
					return;
				}

				var questions = config
					.Value
					.Victorina
					.Questions;

				if (!questions.Exists(x => !string.IsNullOrEmpty(x.Value)))
				{
					await RespondAsync("Нет вопросов которые можно было бы удалить.");
					return;
				}

				var menuBuilder = new SelectMenuBuilder()
					.WithPlaceholder("Выберите какой вопрос удалить.")
					.WithCustomId(nameof(AdminInteractionsController.SelectedQuestionToRemove))
					.WithMinValues(1)
					.WithMaxValues(1);

				for (int i = 0, k = 0; i < questions.Count; i++)
				{
					var question = questions[i];

					if (string.IsNullOrEmpty(question.Value))
					{
						continue;
					}

					menuBuilder.AddOption((k + 1) + " вопрос", i.ToString(), question.Value.Length > 100 ? question.Value[..97] + "..." : question.Value);

					k++;
				}

				var componentBuilder = new ComponentBuilder()
					.WithSelectMenu(menuBuilder);

				await RespondAsync(components: componentBuilder.Build());
			}

			[SlashCommand("со-скрином-вкл", "Добавляет вопрос со скрином, как последний вопрос викторины.")]
			public async Task ScreenshotQuestionOn([Summary("Вопрос")] string question)
			{
				if (!await AdminCheck())
				{
					return;
				}

				config.Update(x => x.Victorina.WithScreenshot = true);
				config.Update(x => x.Victorina.ScreenshotQuestion = new Question
				{
					Value = question,
					MaxLength = 1,
					MinLength = 1
				});

				await RespondAsync("Успешно включили вопрос со скрином.");
			}

			[SlashCommand("со-скрином-выкл", "Удаляет вопрос со скрином.")]
			public async Task ScreenshotQuestionOff()
			{
				if (!await AdminCheck())
				{
					return;
				}

				config.Update(x => x.Victorina.WithScreenshot = false);

				await RespondAsync("Успешно выключили вопрос со скрином.");
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

		[SlashCommand("запустить", "Запускает викторину.")]
		public async Task Start()
		{
			if (!await AdminCheck())
			{
				return;
			}

			if (config.Value.Victorina.EndTime <= DateTimeOffset.UtcNow)
			{
				await RespondAsync("Не можем запустить викторину, т.к. дата окончания викторины уже прошла. Измените её, а потом запустите.");
			}

			if (!config.Value.Victorina.Questions.Any() && !config.Value.Victorina.WithScreenshot)
			{
				await RespondAsync("Не можем запустить викторину, т.к. нет вопросов.");
			}

			config.Update(x => x.Victorina.Active = true);

			await RespondAsync("Успешно запустили викторину.");
		}

		[SlashCommand("остановить", "Принудительно останавливает викторину.")]
		public async Task Stop()
		{
			if (!await AdminCheck())
			{
				return;
			}

			if (!config.Value.Victorina.Active || config.Value.Victorina.EndTime >= DateTimeOffset.UtcNow)
			{
				await RespondAsync("Не можем остановить викторину, т.к. она ещё не запущена.");
			}

			config.Update(x => x.Victorina.Active = true);

			await RespondAsync("Успешно остановили викторину.");
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
