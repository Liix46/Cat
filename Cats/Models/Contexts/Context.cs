using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Cats.Models.Contexts
{
	public class Context : DbContext
	{
        public DbSet<Model> Models { get; set; } = null!;
        public DbSet<Complectation> Complectations { get; set; } = null!;

        public Context(DbContextOptions<Context> options) : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _ = optionsBuilder.LogTo(s => Debug.WriteLine(s));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

    }

   
}

