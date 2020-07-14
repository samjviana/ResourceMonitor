using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB.Modelos {
    public class GPU {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        public double Temperatura { get; set; }
        public double ClockNucleo { get; set; }
        public double ClockMemoria { get; set; }
        public int Numero { get; set; }
        public ICollection<Leitura> Leituras { get; set; }
        [Required]
        public Computador Computador { get; set; }
        [Required]
        public DateTime? DataCriacao { get; set; }
        [Required]
        public DateTime? DataUpdate { get; set; }
        [Timestamp]
        public byte[] Versao { get; set; }
    }
}
