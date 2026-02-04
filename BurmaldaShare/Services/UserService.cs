using BurmaldaSHARE.Models;
using System.Text.Json;

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

        public void Register(string login, string password)
        {
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
    }
}
