using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cats.Models
{
	public class Model
	{
        [Column("Id")]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(20)]
        public string? Name { get; set; }

        [Required]
        [StringLength(10)]
        public string ModelCode { get; set; } //unique

        [StringLength(20)]
        public string? DateManufacture { get; set; }

        [StringLength(20)]
        public string? Catalog { get; set; }

        [StringLength(20)]
        public string? Region { get; set; }

        [StringLength(100)]
        public string? ShortComplactations { get; set; }

        public ICollection<Complectation>? Complectations { get; set; }

    }
}

