using ScienceCenter.Models;
using ScienceCenter.Models.DataModels;
using ScienceCenter.Windows;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ScienceCenter.Pages
{
    /// <summary>
    /// Логика взаимодействия для ListEquipmentPage.xaml
    /// </summary>
    public partial class ListEquipmentPage : Page
    {
        private ScientificResearchInstituteContext _context = new ScientificResearchInstituteContext();
        public ListEquipmentPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            DataContext = this;
            if (UserStatic.role == "гость")
            {
                equipmentList.ItemsSource = _context.Equipment.Where(p => p.IdWorker == null && p.IdOffices == null).ToList();
            }
            if (UserStatic.role == "администратор бд" || UserStatic.role == "инженер")
            {
                equipmentList.ItemsSource = _context.Equipment.ToList();
            }
            if (UserStatic.role == "лаборант" || UserStatic.role == "техник" || UserStatic.role == "заведующий лабораторией")
            {
                var usersLab = _context.Workers.Where(p => p.IdWorker == UserStatic.worker_id).Select(p => p.IdOffices).FirstOrDefault();
                var workerIdsInOffice = _context.Workers
                        .Where(p => p.IdOffices == usersLab)
                        .Select(p => p.IdWorker)
                        .ToList();

                equipmentList.ItemsSource = _context.Equipment
                    .Where(p => workerIdsInOffice.Contains((int)p.IdWorker) || p.IdOffices == usersLab)
                    .ToList();
            }

            if (UserStatic.role == "администратор бд" || UserStatic.role == "заведующий лабораторией")
                addEq.Visibility = Visibility.Visible;
            else
                addEq.Visibility = Visibility.Collapsed;

            if (UserStatic.role == "администратор бд" || UserStatic.role == "инженер")
                filter.Visibility = Visibility.Visible;
            else
                filter.Visibility = Visibility.Collapsed;
        }

        private void SelectElem(object sender, SelectionChangedEventArgs e)
        {
            if (UserStatic.role == "гость" || UserStatic.role == "лаборант")
            {
                MessageBox.Show("ДОСТУП ЗАПРЕЩЕН");
                return;
            }
            if (UserStatic.role == "инженер" || UserStatic.role == "техник" || UserStatic.role == "заведующий лабораторией" || UserStatic.role == "администратор бд")
            {
                Equipment item = (Equipment)equipmentList.SelectedItem;
                NavigationService.Navigate(new MoreInformationAboutEquipmentPage(item));
            }
        }

        private void ExitByClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите выйти?", "ВЫХОД", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                UserStatic.worker_id = null;
                UserStatic.role = null;
                NavigationService.Navigate(new LoginPage());
            }
            else
            {
                return;
            }
        }

        private void AddEquipment(object sender, RoutedEventArgs e)
        {
            AddEquipmentWindow window = new AddEquipmentWindow();
            window.Show();
        }

        private void editEquipment(object sender, RoutedEventArgs e)
        {
            LoadData();
            search.Text = string.Empty;
            MessageBox.Show("Данные обновлены успешно!");
        }
        private void sbrosEquipment(object sender, RoutedEventArgs e)
        {
            LoadData();
            search.Text = string.Empty;
        }

        private void SearchList(object sender, RoutedEventArgs e)
        {
            if (search.Text.Trim().Length == 0)
            {
                LoadData();
                return;
            }
            LoadData();
            var list = new List<Equipment>((IEnumerable<Equipment>)equipmentList.ItemsSource);
            equipmentList.ItemsSource = list.Where(p => p.TitleEquipment.ToLower().Contains(search.Text.ToLower())).ToList();
        }

        private void SortABC(object sender, RoutedEventArgs e)
        {
            var list = new List<Equipment>((IEnumerable<Equipment>)equipmentList.ItemsSource);
            equipmentList.ItemsSource = list.OrderBy(p => p.TitleEquipment).ToList();
        }

        private void SortDCB(object sender, RoutedEventArgs e)
        {
            var list = new List<Equipment>((IEnumerable<Equipment>)equipmentList.ItemsSource);
            equipmentList.ItemsSource = list.OrderByDescending(p => p.TitleEquipment).ToList();
        }
    }
}
