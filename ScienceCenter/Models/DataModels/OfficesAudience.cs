namespace ScienceCenter.Models.DataModels;

public partial class OfficesAudience
{
    public int IdOfficeAudiences { get; set; }

    public int IdOffice { get; set; }

    public int IdAudience { get; set; }

    public virtual Audience IdAudienceNavigation { get; set; } = null!;

    public virtual Office IdOfficeNavigation { get; set; } = null!;
}
