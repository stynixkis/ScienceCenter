using ScienceCenter.Models;
using ScienceCenter.Models.DataModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ScienceCenter.Pages
{
    /// <summary>
    /// Страница авторизации пользователя
    /// </summary>
    public partial class LoginPage : Page
    {
        private ScientificResearchInstituteContext _context = new ScientificResearchInstituteContext();

        /// <summary>
        /// Конструктор страницы авторизации
        /// </summary>
        public LoginPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Выполняет вход по логину и паролю
        /// </summary>
        private void LoginByClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //найти пользователя в базе по логину и паролю
                var users = _context.Users.Where(p => p.Login == loginBox.Text && p.Password == passwordBox.Password).FirstOrDefault();

                //проверить успешность авторизации
                if (users == null)
                {
                    //показать сообщения об ошибке
                    loginMes.Content = "Неверный логин!";
                    passwordMes.Content = "Неверный пароль!";
                    return;
                }

                //очистить сообщения об ошибке
                loginMes.Content = "";
                passwordMes.Content = "";

                //получить данные сотрудника
                var worker = _context.Workers.Where(p => p.IdWorker == users.IdWorker).FirstOrDefault();
                var role = _context.Posts.Where(p => p.IdPost == worker.IdPost).FirstOrDefault();

                //сохранить данные пользователя в статическом классе
                UserStatic.worker_id = users.IdWorker;
                UserStatic.role = role.TitlePost;
                UserStatic.name = $"{worker.LastName} {worker.Name} {worker.Patronymic}";

                //перейти на страницу списка оборудования
                NavigationService.Navigate(new ListEquipmentPage());
            }
            catch
            {
                //при ошибке подключения к БД войти как гость
                UserStatic.role = "гость";
                NavigationService.Navigate(new ListEquipmentPage());
            }
        }

        /// <summary>
        /// Выполняет вход как гость
        /// </summary>
        private void LoginByGuestClick(object sender, RoutedEventArgs e)
        {
            //установить роль гостя
            UserStatic.role = "гость";

            //перейти на страницу списка оборудования
            NavigationService.Navigate(new ListEquipmentPage());
        }
    }
}