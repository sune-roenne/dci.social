using DCI.Social.HeadQuarters.Persistance.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Persistance;
internal class SocialDbContext : DbContext
{

    public SocialDbContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<ContestDbo> Contests { get; set; }
    public DbSet<RoundDbo> Rounds { get; set; }
    public DbSet<RoundOptionDbo> RoundOptions { get; set; }
    public DbSet<ContestExecutionDbo> Executions { get; set; }
    public DbSet<RoundExecutionDbo> RoundExecutions { get; set; }
    public DbSet<RoundExecutionSelectionDbo> RoundExecutionSelections { get; set; }
    public DbSet<RoundExecutionBuzzDbo> RoundExecutionBuzzes { get; set; }
    public DbSet<SoundDbo> Sounds { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContestDbo>().SetUpperCaseIdentifiers();
        modelBuilder.Entity<RoundDbo>().SetUpperCaseIdentifiers();
        modelBuilder.Entity<RoundOptionDbo>().SetUpperCaseIdentifiers();
        modelBuilder.Entity<ContestExecutionDbo>().SetUpperCaseIdentifiers();
        modelBuilder.Entity<RoundExecutionDbo>().SetUpperCaseIdentifiers();
        modelBuilder.Entity<RoundExecutionBuzzDbo>().SetUpperCaseIdentifiers()
            .HasKey(_ => new {_.RoundExecutionId, _.UserId});
        modelBuilder.Entity<RoundExecutionSelectionDbo>().SetUpperCaseIdentifiers()
            .HasKey(_ => new { _.RoundExecutionId, _.UserId });


    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<bool>()
            .HaveConversion<int>();
    }

}


internal static class EntityTypeBuilderExtensions
{
    public static List<PropertyBuilder> AllProperties<T>(this EntityTypeBuilder<T> builder, Func<PropertyInfo, bool>? filter = null) where T : class
    {
        EntityTypeBuilder<T> builder2 = builder;
        IEnumerable<PropertyInfo> source = typeof(T).GetProperties().AsEnumerable();
        if (filter != null)
        {
            source = source.Where(filter);
        }

        return source.Select((PropertyInfo x) => builder2.Property(x.PropertyType, x.Name)).ToList();
    }

    public static EntityTypeBuilder<T> SetUpperCaseIdentifiers<T>(this EntityTypeBuilder<T> builder, params string[] exceptNames) where T : class
    {
        List<string> second = (from prop in typeof(T).GetProperties()
                               where prop.PropertyType.IsClass && prop.PropertyType.Name.ToLower() != "string"
                               select prop.Name.ToLower()).ToList();
        HashSet<string> toFilterAway = exceptNames.Select((string en) => en.ToLower()).Union(second).ToHashSet();
        builder.AllProperties((PropertyInfo prop) => !toFilterAway.Contains(prop.Name.ToLower())).ForEach(delegate (PropertyBuilder prop)
        {
            prop.HasColumnName(prop.Metadata.Name.ToUpper());
        });
        return builder;
    }
}