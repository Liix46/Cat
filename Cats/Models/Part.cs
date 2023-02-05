using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cats.Models
{
	public class Part
	{
        [Column("Id")]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(15)]
        public string? Code { get; set; }

		public int Count { get; set; }
        [StringLength(60)]
        public string? Info { get; set; }

        [StringLength(10)]
        public string? TreeCode { get; set; }

        [StringLength(50)]
        public string? Tree { get; set; }

        [StringLength(20)]
        public string? Data { get; set; }

        [StringLength(100)]
        public string? LinkPrefTable { get; set; }

		[ForeignKey("SubGroupId")]
		public int SubgroupId { get; set; }
		public Subgroup? Subgroup { get; set; }
	}
}

