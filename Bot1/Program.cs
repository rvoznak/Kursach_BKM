using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;

namespace Bot1
{
    internal class Bot1
    {
        private static Dictionary<long, State> userState;

        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient("6635411143:AAGgN4ZPdhcjcd0Mw27_XCjSVQxaHAtwyw8");
            userState = new Dictionary<long, State>();

            botClient.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;

            if (message.Text != null)
            {
                if (!userState.ContainsKey(message.Chat.Id))
                {
                    userState[message.Chat.Id] = State.WaitingStart;
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Введите команду /start");
                    return;
                }

                await HandleStart(botClient, update, message);
                await StudentTeacher(botClient, update, message);
                await StudentTeacherDelete(botClient, update, message);
                await Student.RegistrationStudent(botClient, update, userState);
                await Student.SendInformationStudent(botClient, update, userState);
                await Teacher.RegistrationTeacher(botClient, update, userState);
                await Teacher.SendInformationTeacher(botClient, update, userState);
                await BotOnMessage(botClient, update, message);
                await SendMessageStudent(botClient, update, message);
                await FindTeacher(botClient, update, message);
                await CheckAccountT(botClient, update, message);
                await CheckAccountS(botClient, update, message);
                

                if (message.Text == "Информация о проекте" && userState[message.Chat.Id] == State.WaitingButton) // Информация о проекте
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Бот \"Репетиторы здесь!\" был создан для поиска репетитора по всей стране.\n Очное и дистанционное образование уверенно использется в нашей жизни, поэтому Мы собираем преподавателей из разных сфер в одном месте.\n\n 🔥 Удобный поиск репетиторов с различными форматами обучения. \n 🔥 Помощь в написании контрольных и самостоятельных работ. \n 🔥 Проверенные репетиторы со всей страны.");
                    return;
                }

                if (message.Text == "Поделиться ботом" && userState[message.Chat.Id] == State.WaitingButton) // Поделиться ботом
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Мы будем рады, если Вы поделитесь нашим ботом с друзьями!\n\nСсылка на Бота: @Repetitors_here_bot");
                    return;
                }

            }

        }

        async static Task HandleStart(ITelegramBotClient botClient, Update update, Message message) // Кнопки при вводе команды /start
        {
            if (message.Text == "/start" && userState[message.Chat.Id] == State.WaitingStart)
            {
                var tableName = Database.CheckUser(botClient, message);

                if (tableName != null)
                {
                    if (tableName == "teachers")
                    {
                        var replyKeyboard = new ReplyKeyboardMarkup(
                            new[]
                            {
                                new KeyboardButton[] { "Написать ученику", "Удалить аккаунт"},
                                new KeyboardButton[] { "Информация о проекте", "Поделиться ботом" },
                                new KeyboardButton[] { "Просмотр Вашего аккаунта" }
                            })
                        {
                            ResizeKeyboard = true
                        };
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Выберите соответствующую кнопку:", replyMarkup: replyKeyboard);
                        userState[update.Message.Chat.Id] = State.WaitingButton;
                        return;
                    }

                    if (tableName == "students")
                    {
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
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Выберите соответствующую кнопку:", replyMarkup: replyKeyboard);
                        userState[update.Message.Chat.Id] = State.WaitingButton;
                        return;
                    }
                }
                else
                {
                    var replyKeyboard = new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton[] {"Регистрация", "Информация о проекте" }
                    })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Здравствуйте, {message.Chat.Username}! Выберите соответствующую кнопку:", replyMarkup: replyKeyboard);
                    userState[update.Message.Chat.Id] = State.WaitingButton;
                    return;
                }     
            }
        }

        async static Task StudentTeacher(ITelegramBotClient botClient, Update update, Message message) // Кнопки при нажатии Регистрация
        {
            if (message.Text != "Регистрация" || userState[message.Chat.Id] != State.WaitingButton)
            {
                return;
            }

            if (message.Text == "Регистрация" && userState[message.Chat.Id] == State.WaitingButton)
            {
                var replyKeyboard = new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton[] {"Ученик", "Преподаватель" },
                    })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите соответствующую кнопку:", replyMarkup: replyKeyboard);
                userState[update.Message.Chat.Id] = State.WaitingButton;
                return;
            }

        }

        async static Task StudentTeacherDelete(ITelegramBotClient botClient, Update update, Message message)
        {
            
            if (message.Text != "Удалить аккаунт" || userState[message.Chat.Id] != State.WaitingButton)
            {
                return;
            }

            var tableName = Database.CheckUser(botClient, message);

            if (tableName != null)
            {
                var tableNameDel = Database.DeleteUser(botClient, message);
                if (tableNameDel != null)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Вы успешно удалили аккаунт");

                    var replyKeyboard = new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton[] {"Регистрация", "Информация о проекте" }
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Выберите соответствующую кнопку:", replyMarkup: replyKeyboard);
                    userState[update.Message.Chat.Id] = State.WaitingButton;
                    return;
                }
            }

        }

        async static Task FindTeacher(ITelegramBotClient botClient, Update update, Message message) // Поиск преподавателей
        {
            if (message.Text == "Найти преподавателя")
            {
                var tupleOfLists = Database.ListTeachers(botClient);
                List<string> tg_name_t = tupleOfLists.Item1;
                List<string> name_t = tupleOfLists.Item2;
                List<string> subject = tupleOfLists.Item3;
                List<TimeSpan> fix_time = tupleOfLists.Item4;
                List<int> price = tupleOfLists.Item5;
                List<string> description_t = tupleOfLists.Item6;

                string allMessages = "";

                for (int i=0; i < tg_name_t.Count; i++)
                {
                    allMessages += $"Тelegram Name преподавателя: {tg_name_t[i]} \nИмя и фамилия преподавателя: {name_t[i]} \nПредмет: {subject[i]} \nВремя занятия (Часы:минуты:секунды): {fix_time[i]} \nЦена: {price[i]} \nОписание: {description_t[i]}" + "\n\n";

                }

                await botClient.SendTextMessageAsync(message.Chat.Id, "Список преподавателей:");
                await botClient.SendTextMessageAsync(message.Chat.Id, allMessages);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Напишите @Имя_пользователя *сообщение преподавателю* для связи с преподавателем. Для возвращения в главное меню напишите /exit", replyMarkup: new ReplyKeyboardRemove());
                userState[update.Message.Chat.Id] = State.WaitingMessageStudent;
                return;
            }
        }

        async static Task SendMessageStudent(ITelegramBotClient botClient, Update update, Message message) // Написать ученику
        {
            if (message.Text == "Написать ученику")
            {
                var AList = Database.ListStudents(message);
                List<string> tg_name_s = AList.Item1;
                List<string> name_s = AList.Item2;
                List<int> class_s = AList.Item3;
                List<string> telephone_number_s = AList.Item4;

                string allMessages = "";

                for (int i = 0; i < tg_name_s.Count; i++)
                {
                    allMessages += $"Тelegram Name: {tg_name_s[i]} \nИмя и фамилия: {name_s[i]} \nКласс: {class_s[i]}\nНомер телефона: {telephone_number_s[i]}";

                }
                await botClient.SendTextMessageAsync(message.Chat.Id, "Список учеников:");
                await botClient.SendTextMessageAsync(message.Chat.Id, allMessages);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Напишите @Имя_пользователя *сообщение ученику* для связи с учеником. Для возвращения в главное меню напишите /exit", replyMarkup: new ReplyKeyboardRemove());
                userState[update.Message.Chat.Id] = State.WaitingMessageTeacher;
                return;
            }

            if (userState[update.Message.Chat.Id] == State.WaitingMessageTeacher)
            {
                if (message.Text == "/exit")
                {
                    var replyKeyboard = new ReplyKeyboardMarkup(
                            new[]
                            {
                                new KeyboardButton[] { "Написать ученику", "Удалить аккаунт"},
                                new KeyboardButton[] { "Информация о проекте", "Поделиться ботом" },
                                new KeyboardButton[] { "Просмотр Вашего аккаунта" }
                            })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Выберите соответствующую кнопку:", replyMarkup: replyKeyboard);
                    userState[update.Message.Chat.Id] = State.WaitingButton;
                    return;
                }

                string[] messageParts = message.Text.Split(' ');


                if (messageParts.Length >= 2)
                {
                    string username = messageParts[0];
                    string message2 = String.Join(" ", messageParts.Skip(1).ToArray());

                    long chatId = Database.GetChatIdByUsernameStudents(username);
                    if (chatId != 0)
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Message from {message.From.Username}: {message2}");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "User not found");
                    }
                }
                userState[update.Message.Chat.Id] = State.WaitingMessageTeacher;
            }
        }

        async static Task CheckAccountT(ITelegramBotClient botClient, Update update, Message message) // Просмотр аккаунта (Преподавателя)
        {
            if (message.Text == "Просмотр Вашего аккаунта")
            {
                var tupleOfLists = Database.CheckTAccount(botClient, message);
                List<string> tg_name_t = tupleOfLists.Item1;
                List<string> name_t = tupleOfLists.Item2;
                List<string> subject = tupleOfLists.Item3;
                List<TimeSpan> fix_time = tupleOfLists.Item4;
                List<int> price = tupleOfLists.Item5;
                List<string> description_t = tupleOfLists.Item6;

                string allMessages = "";

                for (int i = 0; i < tg_name_t.Count; i++)
                {
                    allMessages += $"Тelegram Name: {tg_name_t[i]} \nИмя и фамилия: {name_t[i]} \nПредмет: {subject[i]} \nВремя занятия (Часы:минуты:секунды): {fix_time[i]} \nЦена: {price[i]} \nОписание: {description_t[i]}";

                }

                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш аккаунт");
                await botClient.SendTextMessageAsync(message.Chat.Id, allMessages);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Если хотите что-то изменить, то нажмите кнопку \n|Удалить аккаунт|. \nДалее заново нажмите кнопку \n|Регистарция|");
                userState[update.Message.Chat.Id] = State.WaitingButton;
                return;
            }
        }



        async static Task CheckAccountS(ITelegramBotClient botClient, Update update, Message message) // Просмотр аккаунта (Ученика)
        {
            if (message.Text == "Просмотр аккаунта")
            {
                var tupleOfLists = Database.CheckSAccount(botClient, message);
                List<string> tg_name_s = tupleOfLists.Item1;
                List<string> name_s = tupleOfLists.Item2;
                List<string> telephone_number_s = tupleOfLists.Item3;
                List<string> description_s = tupleOfLists.Item4;

                string allMessages = "";

                for (int i = 0; i < tg_name_s.Count; i++)
                {
                    allMessages += $"Тelegram Name: {tg_name_s[i]} \nИмя и фамилия: {name_s[i]} \nНомер телефона: {telephone_number_s[i]}\nОписание: {description_s[i]}";

                }

                await botClient.SendTextMessageAsync(message.Chat.Id, "Ваш аккаунт");
                await botClient.SendTextMessageAsync(message.Chat.Id, allMessages);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Если хотите что-то изменить, то нажмите кнопку \n|Удалить аккаунт| \nДалее заново нажмите кнопку \n|Регистарция|");
                userState[update.Message.Chat.Id] = State.WaitingButton;
                return;
            }
        }

        async static Task BotOnMessage(ITelegramBotClient botClient, Update update, Message message)
        {

            if (userState[update.Message.Chat.Id] == State.WaitingMessageStudent)
            {
                if (message.Text == "/exit")
                {
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
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Выберите соответствующую кнопку:", replyMarkup: replyKeyboard);
                    userState[update.Message.Chat.Id] = State.WaitingButton;
                    return;
                }

                string[] messageParts = message.Text.Split(' ');
                
                
                if (messageParts.Length >= 2)
                {
                    string username = messageParts[0];
                    string message2 = String.Join(" ", messageParts.Skip(1).ToArray());

                    long chatId =  Database.GetChatIdByUsernameTeachers(username);
                    Database.StudentData(username, message);

                    if (chatId != 0)
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Message from {message.From.Username}: {message2}");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "User not found");
                    }
                }
                userState[update.Message.Chat.Id] = State.WaitingMessageStudent;
            }
            
        }




        async static Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.Message);
            
        }
    }

    public enum State
    {
        WaitingStart,
        WaitingButton,
        WaitingNameStudent,
        WaitingNameTeacher,
        WaitingLastNameStudent,
        WaitingLastNameTeacher,
        WaitingStudentClass,
        WaitingPhoneNumber,
        WaitingDescriptionStudent,
        WaitingDescriptionTeacher,
        WaitingDataBaseStudent,
        WaitingDataBaseTeacher,
        WaitingSubject,
        WaitingFixTime,
        WaitingPrice,
        WaitingMessageStudent,
        WaitingMessageTeacher
    }
}