using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot1
{
    class Database
    {
        static public string connectionString = "Server=217.28.223.128;port=36700;User Id=admin;Password=admin;Database=Repetitors_bot";

        public static async Task AddStudent(Dictionary<long, StudentInfo> studentInfo, Message message)
        {
            string CommandText = "call add_student_procedure (@name_st, @class_st, @description_st, @telephone_number_st, @tg_name_st, @chat_id_st);";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(CommandText, connection))
                {
                    cmd.Parameters.AddWithValue("name_st", studentInfo[message.Chat.Id].Name + " " + studentInfo[message.Chat.Id].LastName);
                    cmd.Parameters.AddWithValue("class_st", studentInfo[message.Chat.Id].StudentClass);
                    cmd.Parameters.AddWithValue("description_st", studentInfo[message.Chat.Id].Description);
                    cmd.Parameters.AddWithValue("telephone_number_st", studentInfo[message.Chat.Id].PhoneNumber);
                    cmd.Parameters.AddWithValue("tg_name_st", "@" + studentInfo[message.Chat.Id].TgName);
                    cmd.Parameters.AddWithValue("chat_id_st", studentInfo[message.Chat.Id].ChatId);

                    cmd.ExecuteNonQuery();
                }
            }
            return;
        }

        public static async Task AddTeacher(Dictionary<long, TeacherInfo> teacherInfo, Message message)
        {
            string CommandText = "call add_teacher_procedure (@name_te, @subject_te, @description_te, @fix_time_te, @price_te, @tg_name_te, @chat_id_te);";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(CommandText, connection))
                {
                    cmd.Parameters.AddWithValue("name_te", teacherInfo[message.Chat.Id].Name + " " + teacherInfo[message.Chat.Id].LastName);
                    cmd.Parameters.AddWithValue("subject_te", teacherInfo[message.Chat.Id].Subject);
                    cmd.Parameters.AddWithValue("description_te", teacherInfo[message.Chat.Id].Description);
                    cmd.Parameters.AddWithValue("fix_time_te", TimeSpan.Parse(teacherInfo[message.Chat.Id].FixTime));
                    cmd.Parameters.AddWithValue("price_te", teacherInfo[message.Chat.Id].Price);
                    cmd.Parameters.AddWithValue("tg_name_te", "@" + teacherInfo[message.Chat.Id].TgName);
                    cmd.Parameters.AddWithValue("chat_id_te", teacherInfo[message.Chat.Id].ChatId);

                    cmd.ExecuteNonQuery();
                }
            }
            return;
        }

        public static string CheckUser(ITelegramBotClient botClient, Message message)
        {
            var chat_id = message.Chat.Id;
            string sql = @"SELECT 'students' AS students, chat_id FROM students WHERE chat_id = @chat_id
                           UNION ALL
                           SELECT 'teachers' AS teachers, chat_id FROM teachers WHERE chat_id = @chat_id";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("chat_id", chat_id);
                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    while (reader.Read())
                    {
                        string tableName = reader.GetString(0);
                        return tableName;
                    }
                    return null;
                }
            }
        }

        public static async Task DeleteUser(ITelegramBotClient botClient, Message message) // Удаляем профиль пользователя
        {
            var chat_id = message.Chat.Id;
            string sql = @"delete from students where chat_id = @chat_id;
                           delete from teachers where chat_id = @chat_id;
                           DELETE FROM students_teachers st 
                           WHERE st.id_t IN (SELECT te.id_t FROM teachers te WHERE te.chat_id = @chat_id);
                           DELETE FROM students_teachers st 
                           WHERE st.id_s IN (SELECT s.id_t FROM students s WHERE s.chat_id = @chat_id);
                           ";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("chat_id", chat_id);
                    cmd.ExecuteNonQuery();
                }
            }
            return;
        }

        public static Tuple<List<string>, List<string>, List<string>, List<TimeSpan>, List<int>, List<string>> ListTeachers(ITelegramBotClient botClient)
        {
            string sql = "SELECT tg_name_t, name_t, subject, fix_time, price, description_t FROM teachers";

            List<string> tg_name_t = new List<string>();
            List<string> name_t = new List<string>();
            List<string> subject = new List<string>();
            List<TimeSpan> fix_time = new List<TimeSpan>();
            List<int> price = new List<int>();
            List<string> description_t = new List<string>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            tg_name_t.Add(reader.GetString(0));
                            name_t.Add(reader.GetString(1));
                            subject.Add(reader.GetString(2));
                            fix_time.Add(reader.GetTimeSpan(3));
                            price.Add(reader.GetInt32(4));
                            description_t.Add(reader.GetString(5));
                        }
                    }
                }
            }
            return new Tuple<List<string>, List<string>, List<string>, List<TimeSpan>, List<int>, List<string>>(tg_name_t, name_t, subject, fix_time, price, description_t);
        }


        public static Tuple<List<string>, List<string>, List<string>, List<TimeSpan>, List<int>, List<string>> CheckTAccount(ITelegramBotClient botClient, Message message) // Просмотр аккаунта препродавателя
        {
            var chat_id = message.Chat.Id;
            string sql = @"select tg_name_t, name_t, subject, fix_time, price, description_t
                           from teachers
                           where chat_id = @chat_id";

            List<string> tg_name_t = new List<string>();
            List<string> name_t = new List<string>();
            List<string> subject = new List<string>();
            List<TimeSpan> fix_time = new List<TimeSpan>();
            List<int> price = new List<int>();
            List<string> description_t = new List<string>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("chat_id", chat_id);
                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            tg_name_t.Add(reader.GetString(0));
                            name_t.Add(reader.GetString(1));
                            subject.Add(reader.GetString(2));
                            fix_time.Add(reader.GetTimeSpan(3));
                            price.Add(reader.GetInt32(4));
                            description_t.Add(reader.GetString(5));
                        }
                    }
                }
            }
            return new Tuple<List<string>, List<string>, List<string>, List<TimeSpan>, List<int>, List<string>>(tg_name_t, name_t, subject, fix_time, price, description_t);
        }



        public static Tuple<List<string>, List<string>, List<string>, List<string>> CheckSAccount(ITelegramBotClient botClient, Message message) // Просмотр аккаунта ученика
        {
            var chat_id = message.Chat.Id;
            string sql = @"select name_s, description_s, telephone_number_s, tg_name_s
                           from students
                           where chat_id = @chat_id";

            List<string> tg_name_s = new List<string>();
            List<string> name_s = new List<string>();
            List<string> telephone_number_s = new List<string>();
            List<string> description_s = new List<string>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("chat_id", chat_id);
                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            name_s.Add(reader.GetString(0));
                            description_s.Add(reader.GetString(1));
                            telephone_number_s.Add(reader.GetString(2));
                            tg_name_s.Add(reader.GetString(3));
                        }
                    }
                }
            }
            return new Tuple<List<string>, List<string>, List<string>, List<string>>(tg_name_s, name_s, telephone_number_s, description_s);
        }

        public static long GetChatIdByUsernameTeachers(string username)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT chat_id FROM teachers WHERE tg_name_t = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt64(0);
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }

        public static long GetChatIdByUsernameStudents(string username)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT chat_id FROM students WHERE tg_name_s = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt64(0);
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }

        public static async Task StudentData(string username, Message message) // Просмотр аккаунта ученика
        {

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand("call connection_s_t (@tg_name_te, @tg_name_st);", connection))
                {
                    cmd.Parameters.AddWithValue("tg_name_te", username);
                    cmd.Parameters.AddWithValue("tg_name_st", "@" + message.Chat.Username);
                    cmd.ExecuteNonQuery();
                }
            }
            return;
        }

        public static Tuple<List<string>, List<string>, List<int>, List<string>> ListStudents(Message message)
        {
            string sql = @"select * from table_students(@tg_name_t);";

            List<string> tg_name_s = new List<string>();
            List<string> name_s = new List<string>();
            List<int> class_s = new List<int>();
            List<string> telephone_number_s = new List<string>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("tg_name_t", "@" + message.Chat.Username);
                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    if (reader.HasRows)
                    {

                        while (reader.Read())
                        {
                            tg_name_s.Add(reader.GetString(0));
                            name_s.Add(reader.GetString(1));
                            class_s.Add(reader.GetInt32(2));
                            telephone_number_s.Add(reader.GetString(3));
                        }
                    }
                }
            }
            return new Tuple<List<string>, List<string>, List<int>, List<string>>(tg_name_s, name_s, class_s, telephone_number_s);
        }
    }
}

            

                 
          
