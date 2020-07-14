using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB.Modelos {
    public class SistemaOperacional {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        public int VersaoSO { get; set; }
        public int Build { get; set; }
        public int Arquitetura { get; set; }
        public string Usuario { get; set; }
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
