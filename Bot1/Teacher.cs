using Bot1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot1
{
    class Teacher
    {

        public static Dictionary<long, TeacherInfo> teacherInfo = new Dictionary<long, TeacherInfo>();

        async static public Task RegistrationTeacher(ITelegramBotClient botClient, Update update, Dictionary<long, State> userState)
        {
            var message = update.Message;

            if (userState[message.Chat.Id] == State.WaitingButton && message.Text == "Преподаватель")
            {
                teacherInfo[message.Chat.Id] = new TeacherInfo();
                teacherInfo[message.Chat.Id].ChatId = message.Chat.Id;
                teacherInfo[message.Chat.Id].TgName = message.Chat.Username;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите ваше имя: ", replyMarkup: new ReplyKeyboardRemove());
                userState[message.Chat.Id] = State.WaitingNameTeacher;
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingNameTeacher) // Запрос имени пользователя
            {
                teacherInfo[message.Chat.Id].Name = message.Text;
                userState[message.Chat.Id] = State.WaitingLastNameTeacher;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите вашу фамилию: ");
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingLastNameTeacher) // Запрос фамилии пользователя
            {
                teacherInfo[message.Chat.Id].LastName = message.Text;
                userState[message.Chat.Id] = State.WaitingSubject;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Какой предмет вы преподаете: ");
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingSubject) // Запрос предмета обучения
            {
                teacherInfo[message.Chat.Id].Subject = message.Text;
                userState[message.Chat.Id] = State.WaitingFixTime;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Сколько длится занятие (Формата 24:00:00): ");
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingFixTime) // Запрос длительности занятия
            {
                teacherInfo[message.Chat.Id].FixTime = message.Text;
                userState[message.Chat.Id] = State.WaitingPrice;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Стоимость занятия: ");
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingPrice) // Запрос стоимости занятия
            {
                teacherInfo[message.Chat.Id].Price = int.Parse(message.Text);
                userState[message.Chat.Id] = State.WaitingDescriptionTeacher;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Расскажите немного о себе: ");
                return;
            }
            if (userState[message.Chat.Id] == State.WaitingDescriptionTeacher) // Запрос информации о пользователи
            {
                teacherInfo[message.Chat.Id].Description = message.Text;
                userState[message.Chat.Id] = State.WaitingDataBaseTeacher;
                return;
            }
        }

        async static public Task SendInformationTeacher(ITelegramBotClient botClient, Update update, Dictionary<long, State> userState)
        {
            var message = update.Message;

            if (userState[message.Chat.Id] == State.WaitingDataBaseTeacher) // Отправка данных в базу данных и возвращение в начальное меню
            {
                await Database.AddTeacher(teacherInfo, message);
                var replyKeyboard = new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton[] {"Удалить аккаунт"},
                        new KeyboardButton[] {"Информация о проекте", "Поделиться ботом"},
                        new KeyboardButton[] { "Просмотр Вашего аккаунта" }
                    })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Регистрация прошла успешно!", replyMarkup: replyKeyboard);
                userState[update.Message.Chat.Id] = State.WaitingButton;
                return;
            }
        }
    }
    class TeacherInfo
    {
        public long ChatId;
        public string TgName;
        public string Name;
        public string LastName;
        public string Subject;
        public string Description;
        public string FixTime;
        public int Price;
    }
}
