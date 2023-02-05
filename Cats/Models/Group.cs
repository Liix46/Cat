using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cats.Models
{
	public class Group
	{
        [Column("Id")]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(20, MinimumLength = 0)]
        public string? Name { get; set; }

		public ICollection<Subgroup>? Subgroups { get; set; }

        [ForeignKey("ComplectId")]
        public int ComplectationId { get; set; }
        public Complectation? Complectation { get; set; }
	}
}

