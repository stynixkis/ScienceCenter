using ScienceCenter.Models;
using ScienceCenter.Models.DataModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScienceCenter.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddEquipmentWindow.xaml
    /// </summary>
    public partial class AddEquipmentWindow : Window
    {
        private ScientificResearchInstituteContext _context = new ScientificResearchInstituteContext();
        private Equipment newEquipment { get; set; }
        public AddEquipmentWindow()
        {
            InitializeComponent();
            DataContext = this;
            newEquipment = new Equipment();
            newEquipment.IdEquipment = _context.Equipment.Max(p => p.IdEquipment) + 1;
            idEq.Content = $"id оборудования: {newEquipment.IdEquipment}";

            var numbers = _context.Audiences.Select(p => p.NumberAudience).OrderBy(p => p).ToList();
            numbers.Add(null);
            Place.ItemsSource = numbers;

            if (UserStatic.role == "администратор бд")
            {
                var title = _context.Offices.Select(p => p.FullTitle).OrderBy(p => p).ToList();
                title.Add(null);
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

        private void PlaceLong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Place.SelectedItem is string value)
            {
                newEquipment.IdAudience = _context.Audiences
                    .Where(p => p.NumberAudience == value)
                    .Select(p => p.IdAudience)
                    .FirstOrDefault();
            }
        }

        private void OfficeLong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Оборудование закрепляется за подразделением или зав. " +
                "кафедрой выбранного подразделения?\nДА - за подразделением\n" +
                "НЕТ - за зав. кафедрой выбранного подразделения", "Закреп оборудования", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (Office.SelectedItem is string value)
                {
                    newEquipment.IdOffices = _context.Offices
                        .Where(p => p.FullTitle == value)
                        .Select(p => p.IdOffice)
                        .FirstOrDefault();
                    newEquipment.IdWorker = null;
                    return;

                }
            }
            else
            {
                if (Office.SelectedItem is string value)
                {

                    var office = _context.Offices.Where(p => p.FullTitle == value).Select(p => p.IdOffice).FirstOrDefault();
                    var res = _context.Workers.Where(p => p.IdOffices == office && p.IdPost == 1).Select(p => p.IdWorker).FirstOrDefault();
                    if (res != 0)
                    {
                        newEquipment.IdWorker = res;
                        newEquipment.IdOffices = null;
                    }
                    else
                    {
                        newEquipment.IdWorker = null;
                        newEquipment.IdOffices = office;
                        MessageBox.Show("Нельзя закрепить оборудование за зав. кафедрой, т.к. он не назначен.\nОборудование было закреплено за подраделением");
                    }
                    return;
                }
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите сохранить изменения?", "СОХРАНЕНИЕ", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (Name.Text.Trim() != null)
                    {
                        newEquipment.TitleEquipment = Name.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Названия оборудования!");
                        return;
                    }

                    if (Description.Text.Trim() != null)
                    {
                        newEquipment.Description = Description.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Описания оборудования!");
                        return;
                    }

                    if (vs.Text.Trim() != null)
                    {
                        newEquipment.WeightInKg = double.Parse(vs.Text);
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Вес, в кг оборудования!");
                        return;
                    }

                    if (Invent.Text.Trim() != null)
                    {
                        newEquipment.InventoryNumber = Invent.Text;
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Инвентарный номер оборудования!");
                        return;
                    }

                    if (AVG_Year.Text.Trim() != null)
                    {
                        newEquipment.StandardServiceLife = int.Parse(AVG_Year.Text);
                    }
                    else
                    {
                        MessageBox.Show("Сохранение невозможно! Некорректное поле Стандартная жизнь оборудования!");
                        return;
                    }

                    if (Office.SelectedItem == null)
                    {
                        MessageBox.Show($"Сохранение невозможно! Некорректное поле Подразделение - выберите подразделение!");
                        return;
                    }

                    if (Place.SelectedItem == null)
                        newEquipment.IdAudience = null;

                    _context.Add(newEquipment);
                    _context.SaveChanges();
                    MessageBox.Show("Сохранение успешно!\n Чтобы сохранение отобразилось на главном экране " +
                        "- нажмите на значок обновления списка оборудования - 🔄");
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

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? selectedDate = datePicker.SelectedDate;
            newEquipment.DateTransferToCompanyBalance = DateOnly.FromDateTime((DateTime)selectedDate);
        }

        private void LiveCheck(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(e.Text, out _))
                e.Handled = true;
        }
    }
}
