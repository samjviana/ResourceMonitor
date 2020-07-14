using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB.Modelos {
    [Table("Armazenamento")]
    public class Armazenamento {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        public int Capacidade { get; set; }
        public char Disco { get; set; }
        [Required]
        public Computador Computador { get; set; }
        public ICollection<Leitura> Leituras { get; set; }
        [Required]
        public DateTime? DataCriacao { get; set; }
        [Required]
        public DateTime? DataUpdate { get; set; }
        [Timestamp]
        public byte[] Versao { get; set; }

    }
}
