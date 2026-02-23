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
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    //проверить и сохранить название
                    if (Name.Text.Trim() != null)
                    {
                        newEquipment.TitleEquipment = Name.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Названия оборудования!");
                        return;
                    }

                    //проверить и сохранить описание
                    if (Description.Text.Trim() != null)
                    {
                        newEquipment.Description = Description.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Описания оборудования!");
                        return;
                    }

                    //проверить и сохранить дату
                    if (datePicker.SelectedDate == null)
                    {
                        newEquipment.DateTransferToCompanyBalance = DateOnly.FromDateTime(DateTime.Today);
                        datePicker.SelectedDate = DateTime.Today;
                    }

                    //проверить и сохранить вес
                    if (vs.Text.Trim() != null)
                    {
                        newEquipment.WeightInKg = double.Parse(vs.Text);
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Вес, в кг оборудования!");
                        return;
                    }

                    //проверить и сохранить инвентарный номер
                    if (Invent.Text.Trim() != null)
                    {
                        newEquipment.InventoryNumber = Invent.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Инвентарный номер оборудования!");
                        return;
                    }

                    //проверить и сохранить срок службы
                    if (AVG_Year.Text.Trim() != null)
                    {
                        newEquipment.StandardServiceLife = int.Parse(AVG_Year.Text);
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Стандартная жизнь оборудования!");
                        return;
                    }

                    //обработать пустые значения подразделения и аудитории
                    if (Office.SelectedItem == null)
                    {
                        newEquipment.IdOffices = null;
                        newEquipment.IdWorker = null;
                    }

                    if (Place.SelectedItem == null)
                        newEquipment.IdAudience = null;

                    //добавить запись в базу данных
                    _context.Add(newEquipment);
                    _context.SaveChanges();

                    MessageBox.Show("Сохранение успешно!");

                    //обновить список оборудования на странице
                    NewList.LoadData();

                    //закрыть окно
                    this.Close();
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

                    MessageBox.Show("Редактирование успешно!");
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