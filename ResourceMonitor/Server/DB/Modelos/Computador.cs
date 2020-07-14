﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB.Modelos {
    [Table("Computador")]
    public class Computador {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        [Required]
        public ICollection<Armazenamento> Armazenamentos { get; set; }
        [Required]
        public Memoria Memoria { get; set; }
        [Required]
        public ICollection<CPU> CPUs { get; set; }
        [Required]
        public ICollection<GPU> GPUs { get; set; }
        public SistemaOperacional SistemaOperacional { get; set; }
        public bool Estado { get; set; }
        public string IP { get; set; }
        public string MAC { get; set; }
        public string SNMP { get; set; }
        [Required]
        public DateTime? DataCriacao { get; set; }
        [Required]
        public DateTime? DataUpdate { get; set; }
        [Timestamp]
        public byte[] Versao { get; set; }
    }
}
