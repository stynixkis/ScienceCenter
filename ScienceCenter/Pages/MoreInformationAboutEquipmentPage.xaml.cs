using ScienceCenter.Models;
using ScienceCenter.Models.DataModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ScienceCenter.Pages
{
    /// <summary>
    /// Логика взаимодействия для MoreInformationAboutEquipmentPage.xaml
    /// </summary>
    public partial class MoreInformationAboutEquipmentPage : Page
    {
        private ScientificResearchInstituteContext _context = new ScientificResearchInstituteContext();
        private Equipment itemSelect { get; set; }
        public MoreInformationAboutEquipmentPage(Equipment item)
        {
            InitializeComponent();
            DataContext = this;

            itemSelect = item;

            if (item.Photo != null)
            {
                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string resourcesPath = System.IO.Path.Combine(projectRoot, "Resources");
                string photoPath = System.IO.Path.Combine(resourcesPath, item.Photo);

                if (File.Exists(photoPath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();

                        bitmap.UriSource = new Uri(photoPath, UriKind.Absolute);

                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        Image.Source = bitmap;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                        LoadStubImage();
                    }
                }
                else
                {
                    LoadStubImage();
                }
            }
            else
            {
                LoadStubImage();
            }

            if (UserStatic.role == "техник" || UserStatic.role == "инженер")
                PrintInformationShort();

            if (UserStatic.role == "заведующий лабораторией" || UserStatic.role == "администратор бд")
                PrintInformationLong();

        }

        private void LoadStubImage()
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
                Image.Source = bitmap;
            }
            catch
            {
                Image.Source = null;
            }
        }

        private void PrintInformationShort()
        {
            fromShortUser.Visibility = Visibility.Visible;
            fromLongUser.Visibility = Visibility.Collapsed;

            NameShort.Content = itemSelect.TitleEquipment;
            DescriptionShort.Content = "Описание: " + itemSelect.Description;

            if (itemSelect.IdAudience != null)
            {
                PlaceShort.Content = "Аудитория: " + itemSelect.IdAudience;
            }

            if (itemSelect.IdWorker != null)
            {
                var audit = _context.Workers.Where(p => p.IdWorker == itemSelect.IdWorker).Select(p => p.IdOffices).FirstOrDefault();
                OfficeShort.Content = "Подразделение: " + audit;
            }

            if (itemSelect.IdOffices != null)
            {
                OfficeShort.Content = "Подразделение: " + itemSelect.IdOffices;
            }
        }

        private void PrintInformationLong()
        {
            fromShortUser.Visibility = Visibility.Collapsed;
            fromLongUser.Visibility = Visibility.Visible;

            NameLong.Text = itemSelect.TitleEquipment;
            DescriptionLong.Text = itemSelect.Description;

            var numbers = _context.Audiences.Select(p => p.NumberAudience).OrderBy(p => p).ToList();
            numbers.Add(null);
            PlaceLong.ItemsSource = numbers;

            if (itemSelect.IdAudience == null)
            {
                PlaceLong.SelectedIndex = PlaceLong.Items.Count - 1;
            }
            else
            {
                int count = 0;
                foreach (var v in PlaceLong.ItemsSource)
                {
                    if (_context.Audiences.FirstOrDefault(p => p.IdAudience == itemSelect.IdAudience && p.NumberAudience == v) != null)
                    {
                        PlaceLong.SelectedIndex = count;
                        continue;
                    }
                    count++;
                }
            }

            var title = _context.Offices.Select(p => p.FullTitle).OrderBy(p => p).ToList();
            title.Add(null);
            OfficeLong.ItemsSource = title;

            if (itemSelect.IdWorker == null && itemSelect.IdOffices == null)
            {
                OfficeLong.SelectedIndex = OfficeLong.Items.Count - 1;
            }
            else if (itemSelect.IdOffices != null)
            {
                for (int i = 0; i < OfficeLong.Items.Count; i++)
                {
                    if (_context.Offices.FirstOrDefault(p => p.IdOffice == itemSelect.IdOffices && p.FullTitle == OfficeLong.Items[i]) != null)
                    {
                        OfficeLong.SelectedIndex = i;
                        continue;
                    }
                }
            }
            else if (itemSelect.IdWorker != null)
            {
                var audit = _context.Workers.Where(p => p.IdWorker == itemSelect.IdWorker).Select(p => p.IdOffices).FirstOrDefault();

                for (int i = 0; i < OfficeLong.Items.Count; i++)
                {
                    if (_context.Offices.FirstOrDefault(p => p.IdOffice == audit && p.FullTitle == OfficeLong.Items[i]) != null)
                    {
                        OfficeLong.SelectedIndex = i;
                        continue;
                    }
                }
            }

            var date = (itemSelect.DateTransferToCompanyBalance.ToDateTime(TimeOnly.MinValue)).AddYears(itemSelect.StandardServiceLife);

            if (date.Year == DateTime.Now.Year)
            {
                statusLong.Background = (Brush)new BrushConverter().ConvertFrom("#FFA500");
                statusLong.Content = "СРОК СЛУЖБЫ ИСТЕКАЕТ В ТЕКУЩЕМ ГОДУ";
            }
            else if (date < DateTime.Now)
            {
                statusLong.Background = (Brush)new BrushConverter().ConvertFrom("#E32636");
                statusLong.Content = "НА СПИСАНИЕ";
            }
            else if (date > DateTime.Now)
            {
                statusLong.Background = base.Background;
                statusLong.Content = $"СРОК СЛУЖБЫ ДО: {date.ToString("dd. MM. yyyy г.")}";
            }

        }

        private void BackByClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ListEquipmentPage());
        }

        private void EditSave(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите сохранить изменения?", "СОХРАНЕНИЕ", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (NameLong.Text.Trim() != null)
                    {
                        itemSelect.TitleEquipment = NameLong.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Названия оборудования!");
                        return;
                    }

                    if (DescriptionLong.Text.Trim() != null)
                    {
                        itemSelect.Description = DescriptionLong.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Описания оборудования!");
                        return;
                    }

                    MessageBox.Show(itemSelect.IdAudience + " --- " + itemSelect.IdOffices + "\\" + itemSelect.IdWorker);

                    var existingItem = _context.Equipment.Find(itemSelect.IdEquipment);
                    if (existingItem != null)
                    {
                        existingItem.TitleEquipment = itemSelect.TitleEquipment;
                        existingItem.Description = itemSelect.Description;
                        existingItem.IdWorker = itemSelect.IdWorker;
                        existingItem.IdAudience = itemSelect.IdAudience;
                        existingItem.IdOffices = itemSelect.IdOffices;
                        _context.SaveChanges();
                        MessageBox.Show("Редактирование успешно!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex}");
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void PlaceLong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaceLong.SelectedItem is string value)
            {
                itemSelect.IdAudience = _context.Audiences
                    .Where(p => p.NumberAudience == value)
                    .Select(p => p.IdAudience)
                    .FirstOrDefault();
            }
        }

        private void OfficeLong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OfficeLong.SelectedItem is string value)
            {
                if (itemSelect.IdOffices != null)
                {
                    itemSelect.IdOffices = _context.Offices
                        .Where(p => p.FullTitle == value)
                        .Select(p => p.IdOffice)
                        .FirstOrDefault();
                    return;
                }
                if (itemSelect.IdWorker != null)
                {
                    var office = _context.Offices.Where(p => p.FullTitle == value).Select(p => p.IdOffice).FirstOrDefault();
                    itemSelect.IdWorker = _context.Workers.Where(p => p.IdOffices == office && p.IdPost == 1).Select(p => p.IdWorker).FirstOrDefault();
                    return;
                }
            }
        }

        private void DeleteSave(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить оборудование?", "УДАЛЕНИЕ", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var existingItem = _context.Equipment.Find(itemSelect.IdEquipment);
                    if (existingItem != null)
                    {
                        _context.Equipment.Remove(existingItem);
                        _context.SaveChanges();
                        MessageBox.Show("Удаление успешно!");
                        NavigationService.Navigate(new ListEquipmentPage());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex}");
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }
}
