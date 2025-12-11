using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parikmaherskaya
{
    public class Sotrudnik
    {
        public int IdSotrudnika { get; set; }
        public string FIO => $"{Familiya} {Imya} {Otchestvo ?? ""}".Trim();
        public string Familiya { get; set; } = string.Empty;
        public string Imya { get; set; } = string.Empty;
        public string? Otchestvo { get; set; }
        public string Telefon { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Dolzhnost { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Specializacii { get; set; } = string.Empty;
    }
}