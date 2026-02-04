using ScienceCenter.Models;
using ScienceCenter.Models.DataModels;
using ScienceCenter.Windows;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public void LoadData()
        {
            DataContext = this;
            if (UserStatic.role == "гость")
            {
                var listAboutGostLong = _context.Equipment.Where(p => p.IdWorker == null && p.IdOffices == null && (p.IdAudience == null || p.IdAudience == (_context.Audiences.Where(x =>  x.NumberAudience == "склад").Select(x => x.IdAudience).FirstOrDefault()))).ToList();
                
                equipmentList.ItemsSource = (System.Collections.IEnumerable)LoadListEquipment(listAboutGostLong);
                fio.Content = string.Empty;
            }
            if (UserStatic.role == "администратор бд" || UserStatic.role == "инженер")
            {
                var listAboutGostLong = _context.Equipment.ToList();
                equipmentList.ItemsSource = (System.Collections.IEnumerable)LoadListEquipment(listAboutGostLong);
                fio.Content = UserStatic.name;
            }
            if (UserStatic.role == "лаборант" || UserStatic.role == "техник" || UserStatic.role == "заведующий лабораторией")
            {
                var usersLab = _context.Workers.Where(p => p.IdWorker == UserStatic.worker_id).Select(p => p.IdOffices).FirstOrDefault();
                var workerIdsInOffice = _context.Workers
                        .Where(p => p.IdOffices == usersLab)
                        .Select(p => p.IdWorker)
                        .ToList();

                var listAboutGostLong = _context.Equipment
                    .Where(p => workerIdsInOffice.Contains((int)p.IdWorker) || p.IdOffices == usersLab)
                    .ToList();
                equipmentList.ItemsSource = (System.Collections.IEnumerable)LoadListEquipment(listAboutGostLong);
                fio.Content = UserStatic.name;
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
            AddEquipmentWindow window = new AddEquipmentWindow(this);
            window.Show();
        }

        private void EditEquipment(object sender, RoutedEventArgs e)
        {
            LoadData();
            search.Text = string.Empty;
            MessageBox.Show("Данные обновлены успешно!");
        }
        private void SbrosEquipment(object sender, RoutedEventArgs e)
        {
            LoadData();
            search.Text = string.Empty;
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

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (search.Text.Trim().Length == 0)
            {
                LoadData();
                return;
            }
            LoadData();
            var list = new List<Equipment>((IEnumerable<Equipment>)equipmentList.ItemsSource);
            equipmentList.ItemsSource = list.Where(p => p.TitleEquipment.ToLower().Contains(search.Text.ToLower()) 
            || p.InventoryNumber.ToLower().Contains(search.Text.ToLower()) 
            || p.Description.ToLower().Contains(search.Text.ToLower())).ToList();
        }
        private System.Windows.Media.Imaging.BitmapImage LoadStubImage()
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();

                string stubPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "stub.jpg");
                if (File.Exists(stubPath))
                {
                    bitmap.UriSource = new Uri(stubPath, UriKind.Absolute);
                }
                else
                {
                    bitmap.UriSource = new Uri("pack://application:,,,/Resources/stub.jpg");
                }

                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return  null;
            }
        }

        private List<BrieflyAboutEquipment> LoadListEquipment(List<Equipment> listAboutGostLong)
        {
            List<BrieflyAboutEquipment> listAboutGostBriefly = new List<BrieflyAboutEquipment>();

            foreach (var eq in listAboutGostLong)
            {
                var briefItem = new BrieflyAboutEquipment(eq);

                if (eq.Photo != null)
                {
                    string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                    string resourcesPath = Path.Combine(projectRoot, "Resources");
                    string photoPath = Path.Combine(resourcesPath, eq.Photo);

                    if (File.Exists(photoPath))
                    {
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(photoPath, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            briefItem.BitmapImage = bitmap;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                            briefItem.BitmapImage = LoadStubImage();
                        }
                    }
                    else
                    {
                        briefItem.BitmapImage = LoadStubImage();
                    }
                }
                else
                {
                    briefItem.BitmapImage = LoadStubImage();
                }

                var audience = _context.Audiences.FirstOrDefault(a => a.IdAudience == eq.IdAudience);
                if (audience != null)
                {
                    briefItem.AuditorNumber = audience.NumberAudience;
                }
                else
                {
                    briefItem.AuditorNumber = "Не указана";
                }

                var officeAudience = _context.OfficesAudiences.FirstOrDefault(oa => oa.IdAudience == eq.IdAudience);
                if (officeAudience != null)
                {
                    var office = _context.Offices.FirstOrDefault(o => o.IdOffice == officeAudience.IdOffice);
                    if (office != null)
                    {
                        briefItem.OfficeTitle = office.FullTitle;
                    }
                    else
                    {
                        briefItem.OfficeTitle = "Не указано";
                    }
                }
                else
                {
                    briefItem.OfficeTitle = "Не указано";
                }

                var date = (eq.DateTransferToCompanyBalance.ToDateTime(TimeOnly.MinValue)).AddYears(eq.StandardServiceLife);

                if (date.Year == DateTime.Now.Year)
                {
                    briefItem.StatusColor = (Brush)new BrushConverter().ConvertFrom("#FFA500");
                    briefItem.StatusText = "СРОК СЛУЖБЫ ИСТЕКАЕТ В ТЕКУЩЕМ ГОДУ";
                }
                else if (date < DateTime.Now)
                {
                    briefItem.StatusColor = (Brush)new BrushConverter().ConvertFrom("#E32636");
                    briefItem.StatusText = "НА СПИСАНИЕ";
                }
                else
                {
                    briefItem.StatusColor = base.Background;
                    briefItem.StatusText = $"СРОК СЛУЖБЫ ДО: {date.ToString("dd. MM. yyyy г.")}";
                }

                listAboutGostBriefly.Add(briefItem);
            }

            return listAboutGostBriefly;
        }

    }
}
