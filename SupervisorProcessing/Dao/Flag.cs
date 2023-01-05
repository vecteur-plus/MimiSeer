using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupervisorProcessing.Dao
{
    [Table("Flag_repli_tb_sites")]
    public class Flag
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Column("Date_creation")]
        public DateTime DateCreation { get; set; }

        [Column("Type_modification")]
        public string TypeModification { get; set; }

        [Column("Est_vu")]
        public bool IsSeen { get; set; }
    }
}