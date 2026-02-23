using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ScienceCenter.Models;
using ScienceCenter.Models.DataModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ScienceCenter.Pages
{
    /// <summary>
    /// Страница подробной информации об оборудовании
    /// </summary>
    public partial class MoreInformationAboutEquipmentPage : Page
    {
        private ScientificResearchInstituteContext _context = new ScientificResearchInstituteContext();
        private Equipment itemSelect { get; set; }

        /// <summary>
        /// Конструктор страницы подробной информации
        /// </summary>
        /// <param name="item">Выбранное оборудование</param>
        public MoreInformationAboutEquipmentPage(Equipment item)
        {
            InitializeComponent();
            DataContext = this;

            itemSelect = item;

            //загрузить информацию об оборудовании
            LoadInformation();

            //отобразить интерфейс в зависимости от роли пользователя
            if (UserStatic.role == "техник" || UserStatic.role == "инженер")
                PrintInformationShort();

            if (UserStatic.role == "заведующий лабораторией" || UserStatic.role == "администратор бд")
                PrintInformationLong();
        }

        /// <summary>
        /// Загружает изображение оборудования
        /// </summary>
        private void LoadInformation()
        {
            //проверить наличие фото в базе данных
            if (itemSelect.Photo != null)
            {
                //сформировать путь к папке Resources в корне проекта
                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string resourcesPath = System.IO.Path.Combine(projectRoot, "Resources");
                string photoPath = System.IO.Path.Combine(resourcesPath, itemSelect.Photo);

                //проверить существование файла
                if (File.Exists(photoPath))
                {
                    try
                    {
                        //создать и настроить BitmapImage для загрузки изображения
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(photoPath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        //отобразить изображение
                        Image.Source = bitmap;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                        //загрузить заглушку при ошибке
                        LoadStubImage();
                    }
                }
                else
                {
                    //загрузить заглушку если файл не найден
                    LoadStubImage();
                }
            }
            else
            {
                //загрузить заглушку если фото нет в базе
                LoadStubImage();
            }
        }

        /// <summary>
        /// Загружает изображение-заглушку
        /// </summary>
        private void LoadStubImage()
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

                //отобразить заглушку
                Image.Source = bitmap;
            }
            catch
            {
                //убрать изображение при ошибке
                Image.Source = null;
            }
        }

        /// <summary>
        /// Отображает сокращенную информацию для техников и инженеров
        /// </summary>
        private void PrintInformationShort()
        {
            //скрыть кнопку редактирования и длинную форму
            editButn.Visibility = Visibility.Collapsed;
            fromShortUser.Visibility = Visibility.Visible;
            fromLongUser.Visibility = Visibility.Collapsed;

            //заполнить основные поля
            NameShort.Content = itemSelect.TitleEquipment;
            DescriptionShort.Content = "Описание: " + itemSelect.Description;

            //определить и отобразить аудиторию
            if (itemSelect.IdAudience != null)
            {
                var audience = _context.Audiences
                    .FirstOrDefault(a => a.IdAudience == itemSelect.IdAudience);
                PlaceShort.Content = "Аудитория: " + (audience?.NumberAudience ?? "Не указана");
            }
            else
            {
                PlaceShort.Content = "Аудитория: Не указана";
            }

            //определить и отобразить подразделение
            if (itemSelect.IdWorker != null)
            {
                var worker = _context.Workers
                    .Include(w => w.IdOfficesNavigation)
                    .FirstOrDefault(p => p.IdWorker == itemSelect.IdWorker);

                if (worker?.IdOfficesNavigation != null)
                {
                    OfficeShort.Content = "Подразделение: " + worker.IdOfficesNavigation.FullTitle;
                }
                else
                {
                    OfficeShort.Content = "Подразделение: Не указано";
                }
            }
            else if (itemSelect.IdOffices != null)
            {
                var office = _context.Offices
                    .FirstOrDefault(p => p.IdOffice == itemSelect.IdOffices);

                OfficeShort.Content = "Подразделение: " + (office?.FullTitle ?? "Не указано");
            }
            else
            {
                OfficeShort.Content = "Подразделение: Не указано";
            }

            //рассчитать и отобразить дату окончания срока службы
            var date = (itemSelect.DateTransferToCompanyBalance.ToDateTime(TimeOnly.MinValue)).AddYears(itemSelect.StandardServiceLife);
            dateShort.Content = "Дата постановки на учет: " + date.ToString();
            vsShort.Content = "Вес, в кг: " + itemSelect.WeightInKg.ToString();
            inventShort.Content = "Инвентарный номер: " + itemSelect.InventoryNumber;
            standartShort.Content = "Предполагаемый срок службы: " + itemSelect.StandardServiceLife.ToString();
        }

        /// <summary>
        /// Отображает полную информацию для заведующих и администраторов
        /// </summary>
        private void PrintInformationLong()
        {
            //показать кнопку редактирования и длинную форму
            editButn.Visibility = Visibility.Visible;
            fromShortUser.Visibility = Visibility.Collapsed;
            fromLongUser.Visibility = Visibility.Visible;

            //скрыть кнопку удаления для не-администраторов
            if (UserStatic.role != "администратор бд")
                deliteLong.Visibility = Visibility.Collapsed;
            else
                deliteLong.Visibility = Visibility.Visible;

            //заполнить текстовые поля
            NameLong.Text = itemSelect.TitleEquipment;
            DescriptionLong.Text = itemSelect.Description;

            //заполнить выпадающий список аудиторий
            var numbers = _context.Audiences.Select(p => p.NumberAudience).OrderBy(p => p).ToList();
            numbers.Add(string.Empty);
            PlaceLong.ItemsSource = numbers;

            //установить выбранную аудиторию
            if (itemSelect.IdAudience == null)
            {
                PlaceLong.SelectedIndex = PlaceLong.Items.Count - 1;
            }
            else
            {
                var audienceNumbers = PlaceLong.ItemsSource.Cast<string>().ToList();
                var selectedIndex = audienceNumbers
                    .Select((number, index) => new { number, index })
                    .FirstOrDefault(x => _context.Audiences.Any(p => p.IdAudience == itemSelect.IdAudience && p.NumberAudience == x.number))
                    ?.index;

                if (selectedIndex.HasValue)
                {
                    PlaceLong.SelectedIndex = selectedIndex.Value;
                }
            }

            //заполнить выпадающий список подразделений
            var title = _context.Offices.Select(p => p.FullTitle).OrderBy(p => p).ToList();
            title.Add(string.Empty);
            OfficeLong.ItemsSource = title;

            //установить выбранное подразделение
            if (itemSelect.IdWorker == null && itemSelect.IdOffices == null)
            {
                OfficeLong.SelectedIndex = OfficeLong.Items.Count - 1;
            }
            else if (itemSelect.IdOffices != null)
            {
                var offices = OfficeLong.Items.Cast<string>().ToList();
                var selectedIndex = offices
                    .Select((title, index) => new { title, index })
                    .FirstOrDefault(x => _context.Offices.Any(p => p.IdOffice == itemSelect.IdOffices && p.FullTitle == x.title))
                    ?.index;

                if (selectedIndex.HasValue)
                {
                    OfficeLong.SelectedIndex = selectedIndex.Value;
                }
            }
            else if (itemSelect.IdWorker != null)
            {
                var audit = _context.Workers.Where(p => p.IdWorker == itemSelect.IdWorker).Select(p => p.IdOffices).FirstOrDefault();
                var offices = OfficeLong.Items.Cast<string>().ToList();
                var selectedIndex = offices
                    .Select((title, index) => new { title, index })
                    .FirstOrDefault(x => _context.Offices.Any(p => p.IdOffice == audit && p.FullTitle == x.title))
                    ?.index;

                if (selectedIndex.HasValue)
                {
                    OfficeLong.SelectedIndex = selectedIndex.Value;
                }
            }

            //рассчитать и отобразить дату окончания срока службы
            var date = (itemSelect.DateTransferToCompanyBalance.ToDateTime(TimeOnly.MinValue)).AddYears(itemSelect.StandardServiceLife);
            datePicker.Text = date.ToString();
            vs.Text = itemSelect.WeightInKg.ToString();
            Invent.Text = itemSelect.InventoryNumber;
            AVG_Year.Text = itemSelect.StandardServiceLife.ToString();

            //установить статус в зависимости от срока службы
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
            else
            {
                statusLong.Background = base.Background;
                statusLong.Content = $"СРОК СЛУЖБЫ ДО: {date.ToString("dd. MM. yyyy г.")}";
            }
        }

        /// <summary>
        /// Обрабатывает нажатие кнопки "Назад"
        /// </summary>
        private void BackByClick(object sender, RoutedEventArgs e)
        {
            //перейти на страницу списка оборудования
            NavigationService.Navigate(new ListEquipmentPage());
        }

        /// <summary>
        /// Сохраняет изменения в оборудовании
        /// </summary>
        private void EditSave(object sender, RoutedEventArgs e)
        {
            //подтвердить сохранение
            MessageBoxResult result = MessageBox.Show("Вы точно хотите сохранить изменения?", "СОХРАНЕНИЕ", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    //проверить и сохранить название
                    if (NameLong.Text.Trim() != null)
                    {
                        itemSelect.TitleEquipment = NameLong.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Названия оборудования!");
                        return;
                    }

                    //проверить и сохранить описание
                    if (DescriptionLong.Text.Trim() != null)
                    {
                        itemSelect.Description = DescriptionLong.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Описания оборудования!");
                        return;
                    }

                    //проверить и сохранить дату
                    if (datePicker.Text.Trim() == null)
                    {
                        itemSelect.DateTransferToCompanyBalance = DateOnly.FromDateTime(DateTime.Today);
                        datePicker.Text = DateTime.Today.ToString();
                    }

                    //проверить и сохранить вес
                    if (vs.Text.Trim() != null)
                    {
                        itemSelect.WeightInKg = double.Parse(vs.Text);
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Вес, в кг оборудования!");
                        return;
                    }

                    //проверить и сохранить инвентарный номер
                    if (Invent.Text.Trim() != null)
                    {
                        itemSelect.InventoryNumber = Invent.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Инвентарный номер оборудования!");
                        return;
                    }

                    //проверить и сохранить срок службы
                    if (AVG_Year.Text.Trim() != null)
                    {
                        itemSelect.StandardServiceLife = int.Parse(AVG_Year.Text);
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Стандартная жизнь оборудования!");
                        return;
                    }

                    //найти существующую запись в базе
                    var existingItem = _context.Equipment.Find(itemSelect.IdEquipment);

                    if (existingItem == null)
                    {
                        MessageBox.Show("Запись не найдена в базе данных!");
                        return;
                    }

                    //обновить поля записи
                    existingItem.TitleEquipment = NameLong.Text.Trim();
                    existingItem.Description = DescriptionLong.Text.Trim();
                    existingItem.DateTransferToCompanyBalance = itemSelect.DateTransferToCompanyBalance;
                    existingItem.WeightInKg = itemSelect.WeightInKg;
                    existingItem.InventoryNumber = itemSelect.InventoryNumber;
                    existingItem.StandardServiceLife = itemSelect.StandardServiceLife;
                    existingItem.IdAudience = itemSelect.IdAudience;
                    existingItem.IdOffices = itemSelect.IdOffices;
                    existingItem.IdWorker = itemSelect.IdWorker;

                    //сохранить изменения в базе
                    _context.SaveChanges();
                    MessageBox.Show("Редактирование успешно!");
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

        /// <summary>
        /// Обрабатывает изменение выбранной аудитории
        /// </summary>
        private void PlaceLong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaceLong.SelectedItem is string value)
            {
                if (value == string.Empty)
                {
                    itemSelect.IdAudience = null;
                    return;
                }
                //найти и установить ID выбранной аудитории
                itemSelect.IdAudience = _context.Audiences
                    .Where(p => p.NumberAudience == value)
                    .Select(p => p.IdAudience)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Обрабатывает изменение выбранного подразделения
        /// </summary>
        private void OfficeLong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OfficeLong.SelectedItem is string value)
            {
                if (value == string.Empty)
                {
                    itemSelect.IdOffices = null;
                    return;
                }
                if (itemSelect.IdOffices != null)
                {
                    //обновить подразделение
                    itemSelect.IdOffices = _context.Offices
                        .Where(p => p.FullTitle == value)
                        .Select(p => p.IdOffice)
                        .FirstOrDefault();
                    return;
                }
                if (itemSelect.IdWorker != null)
                {
                    //обновить подразделение и ответственного
                    var office = _context.Offices.Where(p => p.FullTitle == value).Select(p => p.IdOffice).FirstOrDefault();
                    itemSelect.IdWorker = _context.Workers.Where(p => p.IdOffices == office && p.IdPost == 1).Select(p => p.IdWorker).FirstOrDefault();
                    return;
                }
            }
        }

        /// <summary>
        /// Проверяет ввод для поля с дробными числами
        /// </summary>
        private void LiveCheckFromDouble(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string currentText = textBox.Text;

            //разрешить только одну запятую или точку
            if (e.Text == "," || e.Text == ".")
            {
                if (currentText.Contains(",") || currentText.Contains("."))
                {
                    e.Handled = true;
                }
                return;
            }

            //проверить что вводится число
            string newText = currentText + e.Text;
            if (!double.TryParse(newText, out _))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Проверяет ввод для поля с целыми числами
        /// </summary>
        private void LiveCheckFromInt(object sender, TextCompositionEventArgs e)
        {
            //проверить что вводится целое число
            if (!int.TryParse(e.Text, out _))
                e.Handled = true;
        }

        /// <summary>
        /// Удаляет оборудование
        /// </summary>
        private void DeleteSave(object sender, RoutedEventArgs e)
        {
            //подтвердить удаление
            MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить оборудование?", "УДАЛЕНИЕ", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var existingItem = _context.Equipment.Find(itemSelect.IdEquipment);

                    //проверить возможность удаления (оборудование на складе и срок истек)
                    if (existingItem.IdOffices != (_context.Offices.Where(p => p.FullTitle.ToLower() == "склад").Select(p => p.IdOffice).FirstOrDefault())
                        || statusLong.Background != (Brush)new BrushConverter().ConvertFrom("#E32636"))
                    {
                        MessageBox.Show("Удаление невозможно - оборудование не на складе или срок эксплуатации не истек!");
                        return;
                    }

                    if (existingItem != null)
                    {
                        //удалить запись из базы
                        _context.Equipment.Remove(existingItem);
                        _context.SaveChanges();
                        MessageBox.Show("Удаление успешно!");

                        //вернуться к списку
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

        /// <summary>
        /// Редактирует фотографию оборудования
        /// </summary>
        private async void EditButn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //открыть диалог выбора файла
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files|*.jpg;*.jpeg;*.png|All files|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == true)
                {
                    //найти запись в базе данных
                    var existingItem = _context.Equipment.Find(itemSelect.IdEquipment);
                    if (existingItem == null)
                    {
                        MessageBox.Show("Запись не найдена в базе данных!");
                        return;
                    }

                    //запомнить имя старого фото
                    string oldPhoto = existingItem.Photo;

                    //получить путь к папке Resources
                    string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                    string resourcesPath = System.IO.Path.Combine(projectRoot, "Resources");

                    //создать папку при необходимости
                    if (!Directory.Exists(resourcesPath))
                    {
                        Directory.CreateDirectory(resourcesPath);
                    }

                    //освободить текущее изображение
                    if (Image.Source != null)
                    {
                        var oldBitmap = Image.Source as BitmapImage;
                        if (oldBitmap != null)
                        {
                            //закрыть поток если был открыт
                            if (oldBitmap.StreamSource != null)
                            {
                                oldBitmap.StreamSource.Close();
                                oldBitmap.StreamSource.Dispose();
                            }

                            //очистить источник URI
                            oldBitmap.UriSource = null;
                        }

                        //убрать ссылку на изображение
                        Image.Source = null;
                    }

                    //принудительно вызвать сборщик мусора
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    //дать время на освобождение ресурсов
                    await Task.Delay(200);

                    //загрузить выбранный файл в память
                    byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);
                    MemoryStream memoryStream = new MemoryStream(imageData);

                    //создать изображение из потока памяти
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = memoryStream;
                    bitmap.DecodePixelWidth = 300;
                    bitmap.DecodePixelHeight = 200;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    //определить расширение файла
                    string extension = System.IO.Path.GetExtension(openFileDialog.FileName).ToLower();
                    string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                    string photoPath = System.IO.Path.Combine(resourcesPath, fileName);

                    //сохранить изображение на диск
                    using (FileStream stream = new FileStream(photoPath, FileMode.Create))
                    {
                        if (extension == ".png")
                        {
                            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                            pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                            pngEncoder.Save(stream);
                        }
                        else
                        {
                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmap));
                            encoder.Save(stream);
                        }
                    }

                    //закрыть и освободить поток памяти
                    memoryStream.Close();
                    memoryStream.Dispose();

                    //обновить имя фото в объекте и базе
                    itemSelect.Photo = fileName;
                    existingItem.Photo = fileName;
                    _context.SaveChanges();

                    //создать изображение для отображения
                    BitmapImage displayBitmap = new BitmapImage();
                    displayBitmap.BeginInit();
                    displayBitmap.UriSource = new Uri(photoPath);
                    displayBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    displayBitmap.EndInit();

                    //отобразить новое изображение
                    Image.Source = displayBitmap;

                    //удалить старое фото если это другой файл. также, если старое фото не используется в каком-либо другом оборудовании
                    if (!string.IsNullOrEmpty(oldPhoto) && oldPhoto != fileName)
                    {
                        var equipmentOther = _context.Equipment.Where(p => p.Photo == oldPhoto).ToList();
                        if (equipmentOther.Count() == 0)
                        {
                            string oldImagePath = System.IO.Path.Combine(resourcesPath, oldPhoto);

                            if (File.Exists(oldImagePath))
                            {
                                try
                                {
                                    File.Delete(oldImagePath);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Не удалось удалить старое фото: {ex.Message}");
                                }
                            }
                        }
                    }

                    MessageBox.Show("Редактирование успешно!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}