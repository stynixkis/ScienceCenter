using ScienceCenter.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Media;

namespace ScienceCenter.Models
{
    public class BrieflyAboutEquipment : Equipment
    {
        public System.Windows.Media.Imaging.BitmapImage BitmapImage { get; set; }
        public string AuditorNumber { get; set; }
        public string OfficeTitle { get; set; }
        public string StatusText { get; set; }
        public Brush StatusColor { get; set; }

        public BrieflyAboutEquipment() { }

        public BrieflyAboutEquipment(Equipment equipment)
        {
            IdEquipment = equipment.IdEquipment;
            TitleEquipment = equipment.TitleEquipment;
            Description = equipment.Description;
            InventoryNumber = equipment.InventoryNumber;
            DateTransferToCompanyBalance = equipment.DateTransferToCompanyBalance;
            StandardServiceLife = equipment.StandardServiceLife;
            Photo = equipment.Photo;
            IdAudience = equipment.IdAudience;
            IdOffices = equipment.IdOffices;
            IdWorker = equipment.IdWorker;
            WeightInKg = equipment.WeightInKg;

            IdAudienceNavigation = equipment.IdAudienceNavigation;
            IdOfficesNavigation = equipment.IdOfficesNavigation;
            IdWorkerNavigation = equipment.IdWorkerNavigation;

            if (equipment.EquipmentWriteOffs != null && equipment.EquipmentWriteOffs.Any())
            {
                EquipmentWriteOffs = new List<EquipmentWriteOff>(equipment.EquipmentWriteOffs);
            }
        }
    }
}
