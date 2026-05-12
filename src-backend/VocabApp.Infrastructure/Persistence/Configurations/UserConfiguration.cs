using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VocabApp.Domain.Aggregates.UserAggregate;
using VocabApp.Domain.Entities;
using VocabApp.Domain.ValueObjects;

namespace VocabApp.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        var userIdConverter = new ValueConverter<UserId, Guid>(
            id => id.Value,
            value => new UserId(value));

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id)
            .HasConversion(userIdConverter)
            .ValueGeneratedNever();

        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(user => user.DailyGoal)
            .IsRequired();

        builder.Property(user => user.Streak)
            .IsRequired();

        builder.Property(user => user.LastActivity)
            .IsRequired();

        builder.Property(user => user.ReviewCount)
            .IsRequired();

        builder.Ignore(user => user.Badges);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.OwnsMany<Badge>("_badges", badges =>
        {
            badges.WithOwner().HasForeignKey("UserId");
            badges.Property<Guid>("Id");
            badges.HasKey("Id");
            badges.Property(b => b.Type)
                .HasConversion<int>()
                .IsRequired();
            badges.Property(b => b.AwardedAt)
                .IsRequired();
            badges.ToTable("UserBadges");
        });

        builder.Navigation("_badges").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
