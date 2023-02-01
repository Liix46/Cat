using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cats.Models.Enums;

namespace Cats.Models
{
	public class Complectation
	{
        [Column("Id")]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string? Name { get; set; }

        [StringLength(20)]
        public string? Date { get; set; }

        [StringLength(10)]
        public string? Engine_1 { get; set; }

        [StringLength(5)]
        public string? Body { get; set; }

        [StringLength(5)]
        public string? Grade { get; set; }

        [StringLength(4, MinimumLength = 4)]
        public string? Transmission { get; set; }

        [StringLength(3, MinimumLength = 3)]
        public string? GearShiftType { get; set; }

        [StringLength(3, MinimumLength = 3)]
        public string? DriverPosition { get; set; }

        [StringLength(2, MinimumLength = 2)]
        public string? Doors { get; set; }

        [StringLength(5)]
        public string? Destination_1 { get; set; }

        [ForeignKey("ModelId")]
        public int ModelId { get; set; }
        public Model? Model { get; set; }

        //public string Cab { get; set; }
        //public string TransmissionModel { get; set; }
        //public string DriverPosition { get; set; }

    }
}

