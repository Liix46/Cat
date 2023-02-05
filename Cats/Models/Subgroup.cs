using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cats.Models
{
	public class Subgroup
	{
        [Column("Id")]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(40, MinimumLength = 0)]
        public string? Name { get; set; }

		public ICollection<Part>? Parts { get; set; }

		[ForeignKey("GroupId")]
		public int GroupId { get; set; }
		public Group? Group { get; set; }

	}
}

