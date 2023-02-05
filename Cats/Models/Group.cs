using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cats.Models
{
	public class Group
	{
        [Column("Id")]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        int Id { get; set; }

        [StringLength(20, MinimumLength = 0)]
        string? Name { get; set; }

		public ICollection<Subgroup>? Subgroups { get; set; }
	}
}

