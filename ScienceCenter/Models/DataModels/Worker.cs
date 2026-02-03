namespace ScienceCenter.Models.DataModels;

public partial class Worker
{
    public int IdWorker { get; set; }

    public string LastName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Patronymic { get; set; }

    public int IdPost { get; set; }

    public decimal Salary { get; set; }

    public int YearBirthday { get; set; }

    public int IdOffices { get; set; }

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<EquipmentWriteOff> EquipmentWriteOffs { get; set; } = new List<EquipmentWriteOff>();

    public virtual Office IdOfficesNavigation { get; set; } = null!;

    public virtual Post IdPostNavigation { get; set; } = null!;

    public virtual User? User { get; set; }
}
