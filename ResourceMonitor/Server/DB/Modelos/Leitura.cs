using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.DB.Modelos {
    [Table("Leitura")]
    public  class Leitura {
        [Key]
        public int Id { get; set; }
        [Required]
        public string JSON { get; set; }
        [Required]
        public DateTime? Horario { get; set; }
        [Required]
        public string Causa { get; set; }
        [Required]
        public DateTime? DataCriacao { get; set; }
        [Required]
        public DateTime? DataUpdate { get; set; }
        [Timestamp]
        public byte[] Versao { get; set; }
    }
}