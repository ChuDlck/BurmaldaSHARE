using BurmaldaSHARE.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BurmaldaSHARE.Services
{
    public class UserService
    {
        private const string DataFolderName = "BurmaldaData";//папка для рабочих файлов софта
        private const string StorageFolderName = "BurmaldaStorage";//папка для директорий юзеров
        private const string DbFileName = "users.json";//бдшка SQL(json) :D
        private readonly string _dataFolderPath;
        private readonly string _storageFolderPath;

        public UserService(IWebHostEnvironment env, IConfiguration configuration) //при создании получает корневую папку приложения и значения из appsettings
        {
            string? dataPathSetting = configuration["Storage:DataPath"]; // берут пути из appsettings
            string? storagePathSetting = configuration["Storage:StoragePath"];

            _dataFolderPath = BuildPath(env.ContentRootPath, dataPathSetting, DataFolderName);
            _storageFolderPath = BuildPath(env.ContentRootPath, storagePathSetting, StorageFolderName);

            Directory.CreateDirectory(_dataFolderPath);
            Directory.CreateDirectory(_storageFolderPath);
        }
        /// <summary>
        /// Если передали пустую настройку - берет дефолтное название и создает где нибудь
        /// Если в appsettings есть абсолютный путь, то юзает его
        /// Если передан относительный путь, юзает его от корня
        /// </summary>
        /// <param name="contentRootPath">Полученная корневая папка</param>
        /// <param name="configuredPath">То, что в appsettings</param>
        /// <param name="defaultFolderName">Стандартное константное имя папки</param>
        /// <returns></returns>
        private static string BuildPath(string contentRootPath, string? configuredPath, string defaultFolderName)
        {
            string path = string.IsNullOrWhiteSpace(configuredPath) ? defaultFolderName : configuredPath;

            return Path.IsPathRooted(path)
                ? path
                : Path.Combine(contentRootPath, path);
        }
        public User Register(string login, string password)
        {
            string pattern = @"^[a-zA-Z0-9_]+$";

            if (!Regex.IsMatch(login, pattern))//проверка логина и пароля на то, что бы он был только из английским символов, чисел и _
            {
                throw new Exception("Логин может содержать только английские буквы, цифры и '_'");
            }

            var users = GetAllUsers();

            if (users.Any(u => u.Login == login))
            {
                throw new Exception("Пользователь уже существует!");
            }

            string userRootPath = Path.Combine(_storageFolderPath, login);

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
            string fullDbPath = Path.Combine(_dataFolderPath, DbFileName);

            if (!File.Exists(fullDbPath))
                return new List<User>();

            string json = File.ReadAllText(fullDbPath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveToFile(List<User> users)
        {
            string fullDbPath = Path.Combine(_dataFolderPath, DbFileName);

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
