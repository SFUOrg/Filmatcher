using Filmatch.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Filmatch.ModelsConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(u => u.MatchesAsUser1)
            .WithOne()
            .HasForeignKey(m => m.User1Id)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(u => u.MatchesAsUser2)
            .WithOne()
            .HasForeignKey(m => m.User2Id)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(u => u.Swipes)
            .WithOne()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}