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
    /// Страница со списком оборудования
    /// </summary>
    public partial class ListEquipmentPage : Page
    {
        private ScientificResearchInstituteContext _context = new ScientificResearchInstituteContext();

        /// <summary>
        /// Конструктор страницы списка оборудования
        /// </summary>
        public ListEquipmentPage()
        {
            InitializeComponent();
            //загрузить данные при создании страницы
            LoadData();
        }

        /// <summary>
        /// Загружает данные оборудования в зависимости от роли пользователя
        /// </summary>
        public void LoadData()
        {
            DataContext = this;

            //загрузить данные для гостя
            if (UserStatic.role == "гость")
            {
                try
                {
                    //показать только оборудование без ответственных и на складе
                    var listAboutGostLong = _context.Equipment.Where(p => p.IdWorker == null && p.IdOffices == null && (p.IdAudience == null || p.IdAudience == (_context.Audiences.Where(x => x.NumberAudience == "склад").Select(x => x.IdAudience).FirstOrDefault()))).ToList();

                    equipmentList.ItemsSource = LoadListEquipment(listAboutGostLong);
                    fio.Content = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка подключения к базе данных");
                    return;
                }
            }

            //загрузить данные для администратора и инженера
            if (UserStatic.role == "администратор бд" || UserStatic.role == "инженер")
            {
                //показать все оборудование
                var listAboutGostLong = _context.Equipment.ToList();
                equipmentList.ItemsSource = LoadListEquipment(listAboutGostLong);

                //показать фильтр по подразделениям
                filter.Visibility = Visibility.Visible;
                var office = _context.Offices.Select(p => p.FullTitle).OrderBy(p => p).ToList();
                office.Add("Все подразделения");
                OfficeBox.ItemsSource = office;
                OfficeBox.SelectedIndex = OfficeBox.Items.Count - 1;
                fio.Content = UserStatic.name;
            }
            else
                filter.Visibility = Visibility.Collapsed;

            //загрузить данные для лаборанта, техника, заведующего
            if (UserStatic.role == "лаборант" || UserStatic.role == "техник" || UserStatic.role == "заведующий лабораторией")
            {
                //найти подразделение пользователя
                var usersLab = _context.Workers.Where(p => p.IdWorker == UserStatic.worker_id).Select(p => p.IdOffices).FirstOrDefault();
                var workerIdsInOffice = _context.Workers
                        .Where(p => p.IdOffices == usersLab)
                        .Select(p => p.IdWorker)
                        .ToList();

                //показать оборудование в подразделении пользователя
                var listAboutGostLong = _context.Equipment
                    .Where(p => workerIdsInOffice.Contains((int)p.IdWorker) || p.IdOffices == usersLab)
                    .ToList();
                equipmentList.ItemsSource = LoadListEquipment(listAboutGostLong);
                fio.Content = UserStatic.name;
            }

            //показать кнопку добавления для администратора и заведующего
            if (UserStatic.role == "администратор бд" || UserStatic.role == "заведующий лабораторией")
                addEq.Visibility = Visibility.Visible;
            else
                addEq.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Обрабатывает выбор элемента в списке
        /// </summary>
        private void SelectElem(object sender, SelectionChangedEventArgs e)
        {
            //запретить доступ гостью и лаборанту
            if (UserStatic.role == "гость" || UserStatic.role == "лаборант")
            {
                MessageBox.Show("ДОСТУП ЗАПРЕЩЕН");
                return;
            }

            //открыть страницу с подробной информацией для остальных ролей
            if (UserStatic.role == "инженер" || UserStatic.role == "техник" || UserStatic.role == "заведующий лабораторией" || UserStatic.role == "администратор бд")
            {
                Equipment item = (Equipment)equipmentList.SelectedItem;
                NavigationService.Navigate(new MoreInformationAboutEquipmentPage(item));
            }
        }

        /// <summary>
        /// Выполняет выход из системы
        /// </summary>
        private void ExitByClick(object sender, RoutedEventArgs e)
        {
            //подтвердить выход
            MessageBoxResult result = MessageBox.Show("Вы точно хотите выйти?", "ВЫХОД", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                //очистить данные пользователя
                UserStatic.worker_id = null;
                UserStatic.role = null;

                //вернуться на страницу входа
                NavigationService.Navigate(new LoginPage());
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Открывает окно добавления оборудования
        /// </summary>
        private void AddEquipment(object sender, RoutedEventArgs e)
        {
            AddEquipmentWindow window = new AddEquipmentWindow(this);
            window.Show();
        }

        /// <summary>
        /// Обновляет данные на странице
        /// </summary>
        private void EditEquipment(object sender, RoutedEventArgs e)
        {
            LoadData();
            search.Text = string.Empty;
            MessageBox.Show("Данные обновлены успешно!");
        }

        /// <summary>
        /// Сбрасывает фильтры и обновляет данные
        /// </summary>
        private void SbrosEquipment(object sender, RoutedEventArgs e)
        {
            LoadData();
            search.Text = string.Empty;
        }

        /// <summary>
        /// Сортирует список по возрастанию веса
        /// </summary>
        private void SortABC(object sender, RoutedEventArgs e)
        {
            var list = new List<Equipment>((IEnumerable<Equipment>)equipmentList.ItemsSource);
            equipmentList.ItemsSource = list.OrderBy(p => p.WeightInKg).ToList();
        }

        /// <summary>
        /// Сортирует список по убыванию веса
        /// </summary>
        private void SortDCB(object sender, RoutedEventArgs e)
        {
            var list = new List<Equipment>((IEnumerable<Equipment>)equipmentList.ItemsSource);
            equipmentList.ItemsSource = list.OrderByDescending(p => p.WeightInKg).ToList();
        }

        /// <summary>
        /// Выполняет поиск по названию, инвентарному номеру и описанию
        /// </summary>
        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            //сбросить поиск при пустой строке
            if (search.Text.Trim().Length == 0)
            {
                LoadData();
                return;
            }

            LoadData();

            //отфильтровать список по тексту поиска
            var list = new List<Equipment>((IEnumerable<Equipment>)equipmentList.ItemsSource);
            equipmentList.ItemsSource = list.Where(p => p.TitleEquipment.ToLower().Contains(search.Text.ToLower())
            || p.InventoryNumber.ToLower().Contains(search.Text.ToLower())
            || p.Description.ToLower().Contains(search.Text.ToLower())).ToList();
        }

        /// <summary>
        /// Загружает изображение-заглушку
        /// </summary>
        private System.Windows.Media.Imaging.BitmapImage LoadStubImage()
        {
            try
            {
                //создать и настроить BitmapImage для заглушки
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();

                //попробовать загрузить из папки Resources
                string stubPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "stub.jpg");
                if (File.Exists(stubPath))
                {
                    bitmap.UriSource = new Uri(stubPath, UriKind.Absolute);
                }
                else
                {
                    //загрузить из ресурсов сборки
                    bitmap.UriSource = new Uri("pack://application:,,,/Resources/stub.jpg");
                }

                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Преобразует список Equipment в список BrieflyAboutEquipment с изображениями
        /// </summary>
        private List<BrieflyAboutEquipment> LoadListEquipment(List<Equipment> listAboutGostLong)
        {
            List<BrieflyAboutEquipment> listAboutGostBriefly = new List<BrieflyAboutEquipment>();

            foreach (var eq in listAboutGostLong)
            {
                //создать краткую информацию об оборудовании
                var briefItem = new BrieflyAboutEquipment(eq);

                //загрузить изображение
                if (eq.Photo != null)
                {
                    string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                    string resourcesPath = Path.Combine(projectRoot, "Resources");
                    string photoPath = Path.Combine(resourcesPath, eq.Photo);

                    if (File.Exists(photoPath))
                    {
                        try
                        {
                            //загрузить изображение из файла
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

                //определить аудиторию
                var audience = _context.Audiences.FirstOrDefault(a => a.IdAudience == eq.IdAudience);
                if (audience != null)
                {
                    briefItem.AuditorNumber = audience.NumberAudience;
                }
                else
                {
                    briefItem.AuditorNumber = "Не указана";
                }

                //определить подразделение через связь OfficesAudiences
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

                //установить статус в зависимости от роли
                if (UserStatic.role == "гость" || UserStatic.role == "лаборант" || UserStatic.role == "техник" || UserStatic.role == "инженер")
                {
                    briefItem.StatusText = string.Empty;
                }
                else
                {
                    //рассчитать дату окончания срока службы
                    var date = (eq.DateTransferToCompanyBalance.ToDateTime(TimeOnly.MinValue)).AddYears(eq.StandardServiceLife);

                    //установить цвет и текст статуса
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
                }

                listAboutGostBriefly.Add(briefItem);
            }

            return listAboutGostBriefly;
        }

        /// <summary>
        /// Фильтрует список по выбранному подразделению
        /// </summary>
        private void OfficeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OfficeBox.SelectedItem is string value)
            {
                //показать все подразделения
                if (value == "Все подразделения")
                {
                    var listAboutGostLong = _context.Equipment.ToList();
                    equipmentList.ItemsSource = LoadListEquipment(listAboutGostLong);
                }
                else
                {
                    //показать оборудование в выбранном подразделении
                    var office = _context.Offices.Where(p => p.FullTitle == value).Select(p => p.IdOffice).FirstOrDefault();
                    var listAboutGostLong = _context.Equipment.Where(p => p.IdOffices == office || p.IdWorker == (_context.Workers.Where(c => c.IdWorker == p.IdWorker && c.IdOffices == office).Select(c => c.IdWorker).FirstOrDefault())).ToList();
                    equipmentList.ItemsSource = LoadListEquipment(listAboutGostLong);
                }
            }
        }
    }
}