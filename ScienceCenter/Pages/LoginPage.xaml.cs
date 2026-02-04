using ScienceCenter.Models;
using ScienceCenter.Models.DataModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ScienceCenter.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private ScientificResearchInstituteContext _context = new ScientificResearchInstituteContext();
        public LoginPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoginByClick(object sender, RoutedEventArgs e)
        {
            var users = _context.Users.Where(p => p.Login == loginBox.Text && p.Password == passwordBox.Password).FirstOrDefault();

            if (users == null)
            {
                loginMes.Content = "Неверный логин!";
                passwordMes.Content = "Неверный пароль!";
                return;
            }

            loginMes.Content = "";
            passwordMes.Content = "";

            var worker = _context.Workers.Where(p => p.IdWorker == users.IdWorker).FirstOrDefault();
            var role = _context.Posts.Where(p => p.IdPost == worker.IdPost).FirstOrDefault();

            UserStatic.worker_id = users.IdWorker;
            UserStatic.role = role.TitlePost;
            UserStatic.name = $"{worker.LastName} {worker.Name} {worker.Patronymic}";

            NavigationService.Navigate(new ListEquipmentPage());
        }

        private void LoginByGuestClick(object sender, RoutedEventArgs e)
        {
            UserStatic.role = "гость";
            NavigationService.Navigate(new ListEquipmentPage());
        }
    }
}
