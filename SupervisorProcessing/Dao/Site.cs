using System.ComponentModel.DataAnnotations.Schema;

namespace SupervisorProcessing.Dao
{
    [Table("repli_tb_sites")]
    public class Site
    {
        public Site()
        {
        }

        [Column("ID")]
        public int Id { get; set; }

        [Column("CLE_GENERAL")]
        public string CleGeneral { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("WgAgentName")]
        public string AgentName { get; set; }

        [Column("TYPE_INDEXATION")]
        public string TypeIndexation { get; set; }

        [Column("STATUT_PRODUCTION")]
        public bool StatutProduction { get; set; }

        public string GrabbingSettings { get; set; }

        [Column("REMARQUES_OPERATEURS")]
        public string Commentaire { get; set; }

        [NotMapped]
        public string AgentNameClean { get => AgentName.Replace("\"", ""); }
    }
}