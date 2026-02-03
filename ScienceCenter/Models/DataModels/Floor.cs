namespace ScienceCenter.Models.DataModels;

public partial class Floor
{
    public int IdFloor { get; set; }

    public int FloorTitle { get; set; }

    public virtual ICollection<Audience> Audiences { get; set; } = new List<Audience>();
}
