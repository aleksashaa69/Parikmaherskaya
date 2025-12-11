using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parikmaherskaya
{
    public class ZapisView
    {
        public int IdZapisi { get; set; }
        public DateTime VremyaData { get; set; }
        public string KlientFIO { get; set; } = string.Empty;
        public string MasterFIO { get; set; } = string.Empty;
        public string Usluga { get; set; } = string.Empty;
        public int Stoimost { get; set; }
        public string Status { get; set; } = string.Empty;

        public string DateTimeFormat => VremyaData.ToString("dd.MM.yyyy HH:mm");
        public string StoimostFormat => $"{Stoimost:N0} ₽";
        public int StoimostSort => Stoimost;
    }
}