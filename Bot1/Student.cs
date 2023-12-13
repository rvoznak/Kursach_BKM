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
    class Student
    {
        public static Dictionary<long, StudentInfo> studentInfo = new Dictionary<long, StudentInfo>();

        async static public Task RegistrationStudent(ITelegramBotClient botClient, Update update, Dictionary<long, State> userState)
        {
            var message = update.Message;
            
            
            if (userState[message.Chat.Id] == State.WaitingButton && message.Text == "Ученик")
            {
                studentInfo[message.Chat.Id] = new StudentInfo();
                studentInfo[message.Chat.Id].ChatId = message.Chat.Id;
                studentInfo[message.Chat.Id].TgName = message.Chat.Username;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите ваше имя: ", replyMarkup: new ReplyKeyboardRemove());
                userState[message.Chat.Id] = State.WaitingNameStudent;
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingNameStudent) // Запрос имени пользователя
            {
                studentInfo[message.Chat.Id].Name = message.Text;
                userState[message.Chat.Id] = State.WaitingLastNameStudent;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите вашу фамилию: ");
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingLastNameStudent) // Запрос фамилии пользователя
            {
                studentInfo[message.Chat.Id].LastName = message.Text;
                userState[message.Chat.Id] = State.WaitingStudentClass;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите ваш класс: ");
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingStudentClass) // Запрос класса пользователя
            {
                studentInfo[message.Chat.Id].StudentClass = int.Parse(message.Text);
                userState[message.Chat.Id] = State.WaitingPhoneNumber;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите ваш номер телефона: ");
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingPhoneNumber) // Запрос номера телефона пользователя
            {
                studentInfo[message.Chat.Id].PhoneNumber = message.Text;
                userState[message.Chat.Id] = State.WaitingDescriptionStudent;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Расскажите немного о себе: ");
                return;
            }

            if (userState[message.Chat.Id] == State.WaitingDescriptionStudent) // Запрос информации о пользователи
            {
                studentInfo[message.Chat.Id].Description = message.Text;
                userState[message.Chat.Id] = State.WaitingDataBaseStudent;
                return;
            }
        }

        async static public Task SendInformationStudent(ITelegramBotClient botClient, Update update, Dictionary<long, State> userState)
        {
            var message = update.Message;

            if (userState[message.Chat.Id] == State.WaitingDataBaseStudent) // Отправка данных в базу данных и возвращение в начальное меню
            { 
                await Database.AddStudent(studentInfo, message);
                var replyKeyboard = new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton[] { "Найти преподавателя" },
                        new KeyboardButton[] { "Удалить аккаунт", "Поделиться ботом" },
                        new KeyboardButton[] { "Просмотр аккаунта" }
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

    class StudentInfo
    {
        public long ChatId;
        public string TgName;
        public string Name;
        public string LastName;
        public int StudentClass;
        public string PhoneNumber;
        public string Description;
    }
}