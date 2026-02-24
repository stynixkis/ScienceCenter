using Microsoft.Win32;
using ScienceCenter.Models;
using ScienceCenter.Models.DataModels;
using ScienceCenter.Pages;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ScienceCenter.Windows
{
    /// <summary>
    /// Окно добавления нового оборудования
    /// </summary>
    public partial class AddEquipmentWindow : Window
    {
        private ScientificResearchInstituteContext _context = new ScientificResearchInstituteContext();
        private Equipment newEquipment { get; set; }
        private ListEquipmentPage NewList { get; set; }

        /// <summary>
        /// Конструктор окна добавления оборудования
        /// </summary>
        /// <param name="lists">Ссылка на страницу списка для обновления</param>
        public AddEquipmentWindow(ListEquipmentPage lists)
        {
            InitializeComponent();
            DataContext = this;

            //создать новое оборудование
            newEquipment = new Equipment();

            //установить ID нового оборудования
            if (_context.Equipment.Count() != 0)
                newEquipment.IdEquipment = _context.Equipment.Max(p => p.IdEquipment) + 1;
            else
                newEquipment.IdEquipment = 1;

            //отобразить ID оборудования
            idEq.Content = $"id оборудования: {newEquipment.IdEquipment}";

            //сохранить ссылку на страницу списка
            NewList = lists;

            //заполнить выпадающий список аудиторий
            var numbers = _context.Audiences.Select(p => p.NumberAudience).OrderBy(p => p).ToList();
            numbers.Add(string.Empty);
            Place.ItemsSource = numbers;

            //заполнить выпадающий список подразделений в зависимости от роли
            if (UserStatic.role == "администратор бд")
            {
                var title = _context.Offices.Select(p => p.FullTitle).OrderBy(p => p).ToList();
                title.Add(string.Empty);
                Office.ItemsSource = title;
            }
            else
            {
                var list = new List<string>();
                var office = _context.Workers.Where(p => p.IdWorker == UserStatic.worker_id).Select(p => p.IdOffices).OrderBy(p => p).FirstOrDefault();
                var title = _context.Offices.Where(p => p.IdOffice == office).Select(p => p.FullTitle).FirstOrDefault();
                list.Add(title);
                Office.ItemsSource = list;
            }
        }

        /// <summary>
        /// Обрабатывает изменение выбранной аудитории
        /// </summary>
        private void PlaceLong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Place.SelectedItem is string value)
            {
                //очистить аудиторию при выборе пустой строки
                if (value == string.Empty)
                {
                    newEquipment.IdAudience = null;
                    return;
                }

                //установить ID выбранной аудитории
                newEquipment.IdAudience = _context.Audiences
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
            if (Office.SelectedItem is string value)
            {
                //очистить подразделение при выборе пустой строки
                if (value == string.Empty)
                {
                    newEquipment.IdOffices = null;
                }

                //установить ID выбранного подразделения и ответственного
                newEquipment.IdOffices = _context.Offices
                    .Where(p => p.FullTitle == value)
                    .Select(p => p.IdOffice)
                    .FirstOrDefault();
                newEquipment.IdWorker = UserStatic.worker_id;
                return;
            }
        }

        /// <summary>
        /// Сохраняет новое оборудование в базу данных
        /// </summary>
        private void Save(object sender, RoutedEventArgs e)
        {
            //запросить подтверждение сохранения
            MessageBoxResult result = MessageBox.Show("Вы точно хотите сохранить изменения?", "СОХРАНЕНИЕ", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            //очистить предыдущие сообщения об ошибках
            ClearErrorMessages();

            //флаг наличия ошибок
            bool hasErrors = false;

            try
            {
                //валидация названия
                if (string.IsNullOrWhiteSpace(Name.Text))
                {
                    ShowError("Название оборудования не может быть пустым!", Name);
                    hasErrors = true;
                }
                else if (Name.Text.Length < 3)
                {
                    ShowError("Название должно содержать минимум 3 символа!", Name);
                    hasErrors = true;
                }
                else if (Name.Text.Length > 100)
                {
                    ShowError("Название не может превышать 100 символов!", Name);
                    hasErrors = true;
                }
                else
                {
                    newEquipment.TitleEquipment = Name.Text.Trim();
                }

                //валидация описания
                if (string.IsNullOrWhiteSpace(Description.Text))
                {
                    ShowError("Описание не может быть пустым!", Description);
                    hasErrors = true;
                }
                else if (Description.Text.Length < 5)
                {
                    ShowError("Описание должно содержать минимум 5 символов!", Description);
                    hasErrors = true;
                }
                else if (Description.Text.Length > 500)
                {
                    ShowError("Описание не может превышать 500 символов!", Description);
                    hasErrors = true;
                }
                else
                {
                    newEquipment.Description = Description.Text.Trim();
                }

                //валидация даты
                if (datePicker.SelectedDate == null)
                {
                    //если дата не выбрана, ставим сегодня
                    newEquipment.DateTransferToCompanyBalance = DateOnly.FromDateTime(DateTime.Today);
                    datePicker.SelectedDate = DateTime.Today;
                }
                else if (datePicker.SelectedDate > DateTime.Today)
                {
                    ShowError("Дата постановки на учет не может быть в будущем!", datePicker);
                    hasErrors = true;
                }
                else if (datePicker.SelectedDate < new DateTime(2000, 1, 1))
                {
                    ShowError("Дата постановки на учет не может быть раньше 2000 года!", datePicker);
                    hasErrors = true;
                }
                else
                {
                    newEquipment.DateTransferToCompanyBalance = DateOnly.FromDateTime(datePicker.SelectedDate.Value);
                }

                //валидация веса
                if (string.IsNullOrWhiteSpace(vs.Text))
                {
                    ShowError("Вес не может быть пустым!", vs);
                    hasErrors = true;
                }
                else
                {
                    if (!double.TryParse(vs.Text, out double weight))
                    {
                        ShowError("Вес должен быть числом!", vs);
                        hasErrors = true;
                    }
                    else if (weight <= 0)
                    {
                        ShowError("Вес должен быть положительным числом!", vs);
                        hasErrors = true;
                    }
                    else if (weight > 10000)
                    {
                        ShowError("Вес не может превышать 10000 кг!", vs);
                        hasErrors = true;
                    }
                    else
                    {
                        newEquipment.WeightInKg = weight;
                    }
                }

                //валидация инвентарного номера
                if (string.IsNullOrWhiteSpace(Invent.Text))
                {
                    ShowError("Инвентарный номер не может быть пустым!", Invent);
                    hasErrors = true;
                }
                else
                {
                    string inv = Invent.Text.Trim();

                    //проверка уникальности инвентарного номера
                    if (_context.Equipment.Any(e => e.InventoryNumber == inv && e.IdEquipment != newEquipment.IdEquipment))
                    {
                        ShowError("Оборудование с таким инвентарным номером уже существует!", Invent);
                        hasErrors = true;
                    }
                    else
                    {
                        newEquipment.InventoryNumber = inv;
                    }
                }

                //валидация срока службы
                if (string.IsNullOrWhiteSpace(AVG_Year.Text))
                {
                    ShowError("Срок службы не может быть пустым!", AVG_Year);
                    hasErrors = true;
                }
                else
                {
                    if (!int.TryParse(AVG_Year.Text, out int years))
                    {
                        ShowError("Срок службы должен быть целым числом!", AVG_Year);
                        hasErrors = true;
                    }
                    else if (years <= 0)
                    {
                        ShowError("Срок службы должен быть положительным числом!", AVG_Year);
                        hasErrors = true;
                    }
                    else if (years > 100)
                    {
                        ShowError("Срок службы не может превышать 100 лет!", AVG_Year);
                        hasErrors = true;
                    }
                    else
                    {
                        newEquipment.StandardServiceLife = years;
                    }
                }

                //валидация подразделения
                if (Office.SelectedItem != null)
                {
                    string selectedOffice = Office.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(selectedOffice))
                    {
                        var office = _context.Offices.FirstOrDefault(o => o.FullTitle == selectedOffice);
                        if (office != null)
                        {
                            newEquipment.IdOffices = office.IdOffice;
                            newEquipment.IdWorker = UserStatic.worker_id; //или найти ответственного
                        }
                    }
                    else
                    {
                        newEquipment.IdOffices = null;
                        newEquipment.IdWorker = null;
                    }
                }
                else
                {
                    newEquipment.IdOffices = null;
                    newEquipment.IdWorker = null;
                }

                //валидация аудитории
                if (Place.SelectedItem != null)
                {
                    string selectedPlace = Place.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(selectedPlace))
                    {
                        var audience = _context.Audiences.FirstOrDefault(a => a.NumberAudience == selectedPlace);
                        if (audience != null)
                        {
                            newEquipment.IdAudience = audience.IdAudience;
                        }
                    }
                    else
                    {
                        newEquipment.IdAudience = null;
                    }
                }
                else
                {
                    newEquipment.IdAudience = null;
                }

                //если есть ошибки - не сохраняем
                if (hasErrors)
                {
                    MessageBox.Show("Исправьте ошибки перед сохранением!", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                //добавить запись в базу данных
                _context.Add(newEquipment);
                _context.SaveChanges();

                MessageBox.Show("Сохранение успешно!", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                //обновить список оборудования на странице
                NewList?.LoadData();

                //закрыть окно
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //вспомогательные методы для валидации
        private void ShowError(string message, Control control)
        {
            //подсветить поле с ошибкой
            control.BorderBrush = System.Windows.Media.Brushes.Red;
            control.BorderThickness = new Thickness(2);
            control.ToolTip = message;

            //показать сообщение
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ClearErrorMessages()
        {
            //очистить подсветку и подсказки у всех полей ввода
            foreach (var control in new Control[] { Name, Description, vs, Invent, AVG_Year, datePicker, Office, Place })
            {
                if (control != null)
                {
                    control.BorderBrush = System.Windows.Media.Brushes.Gray;
                    control.BorderThickness = new Thickness(1);
                    control.ToolTip = null;
                }
            }
        }

        /// <summary>
        /// Обрабатывает изменение выбранной даты
        /// </summary>
        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? selectedDate = datePicker.SelectedDate;
            newEquipment.DateTransferToCompanyBalance = DateOnly.FromDateTime((DateTime)selectedDate);
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
        /// Проверяет ввод на соответствие числовому формату
        /// </summary>
        private void LiveCheckFromInt(object sender, TextCompositionEventArgs e)
        {
            //разрешить только ввод чисел
            if (!double.TryParse(e.Text, out _))
                e.Handled = true;
        }

        private void SaveButn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //открыть диалог выбора файла
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files|*.jpg;*.jpeg;*.png|All files|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == true)
                {

                    //запомнить имя старого фото
                    string oldPhoto = newEquipment.Photo;

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

                    //обновить имя фото в объекте
                    newEquipment.Photo = fileName;
                    
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

                    MessageBox.Show("Добавление успешно!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                      MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButn_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = null;
            newEquipment.Photo = null;
            MessageBox.Show("Фотография удалена!");
        }
    }
}