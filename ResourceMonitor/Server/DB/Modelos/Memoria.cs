using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB.Modelos {
    [Table("Memoria")]
    public class Memoria {
        [Key]
        public int Id { get; set; }
        [Required]
        public int Total { get; set; }
        [Required]
        public int Pentes { get; set; }
        public ICollection<Leitura> Leituras { get; set; }
        [Required]
        public DateTime? DataCriacao { get; set; }
        [Required]
        public DateTime? DataUpdate { get; set; }
        [Timestamp]
        public byte[] Versao { get; set; }
    }
}
