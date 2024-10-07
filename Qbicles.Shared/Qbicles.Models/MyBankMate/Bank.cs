using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.MyBankMate
{
    [Table("bm_bank")]
    public class Bank
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LogoUri { get; set; }
    }
}
