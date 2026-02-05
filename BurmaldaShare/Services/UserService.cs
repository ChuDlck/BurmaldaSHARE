using BurmaldaSHARE.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BurmaldaSHARE.Services
{
    public class UserService
    {
        private const string DataFolderName = "BurmaldaData";//папка для рабочих файлов софта
        private const string StorageFolderName = "BurmaldaStorage";//папка для директорий юзеров
        private const string DbFileName = "users.json";//бдшка SQL(json) :D

        public UserService()
        {
            if (!Directory.Exists(DataFolderName))
            {
                Directory.CreateDirectory(DataFolderName);
            }

            if (!Directory.Exists(StorageFolderName))
            {
                Directory.CreateDirectory(StorageFolderName);
            }
        }

        public User Register(string login, string password)
        {
            string pattern = @"^[a-zA-Z0-9]+$";

            if (!Regex.IsMatch(login, pattern))//проверка логина и пароля на то, что бы он был только из английским символов, чисел и _
            {
                throw new Exception("Логин может содержать только английские буквы, цифры и '_'");
            }

            var users = GetAllUsers();

            if (users.Any(u => u.Login == login))
            {
                throw new Exception("Пользователь уже существует!");
            }

            string userRootPath = Path.Combine(StorageFolderName, login);

            var newUser = new User
            {
                
                Login = login,
                Password = HashHelper.HashPassword(password),
                RootFolderPath = userRootPath
            };

            users.Add(newUser);
            SaveToFile(users);

            if (!Directory.Exists(newUser.RootFolderPath))
            {
                Directory.CreateDirectory(newUser.RootFolderPath);
            }
            return newUser;
        }

        private List<User> GetAllUsers()
        {
            string fullDbPath = Path.Combine(DataFolderName, DbFileName);

            if (!File.Exists(fullDbPath))
                return new List<User>();

            string json = File.ReadAllText(fullDbPath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveToFile(List<User> users)
        {
            string fullDbPath = Path.Combine(DataFolderName, DbFileName);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(users, options);

            File.WriteAllText(fullDbPath, json);
        }
        public User? Authenticate(string login, string password)
        {
            var users = GetAllUsers();
            var user = users.FirstOrDefault(u => u.Login == login);

            if (user == null) return null;
            if (HashHelper.VerifyPassword(password, user.Password))
            {
                return user;
            }
            return null;
        }
    }
}
