using Microsoft.EntityFrameworkCore;
using PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Entities;

namespace PinkRoosterAi.Framework.PersistentChatClient.EntityFramework.Context;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    public DbSet<ChatConversationEntity> Conversations { get; set; } = null!;
    public DbSet<ChatMessageEntity> Messages { get; set; } = null!;
    public DbSet<ChatResponseEntity> Responses { get; set; } = null!;

    // Content entity sets
    public DbSet<TextContentEntity> TextContents { get; set; } = null!;
    public DbSet<ReasoningContentEntity> ReasoningContents { get; set; } = null!;
    public DbSet<DataContentEntity> DataContents { get; set; } = null!;
    public DbSet<UriContentEntity> UriContents { get; set; } = null!;
    public DbSet<ErrorContentEntity> ErrorContents { get; set; } = null!;
    public DbSet<FunctionCallContentEntity> FunctionCallContents { get; set; } = null!;
    public DbSet<FunctionResultContentEntity> FunctionResultContents { get; set; } = null!;
    public DbSet<UsageContentEntity> UsageContents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure table-per-hierarchy inheritance for content entities
        modelBuilder.Entity<BaseContentEntity>()
            .HasDiscriminator<string>("ContentType")
            .HasValue<TextContentEntity>(ContentTypeConstants.Text)
            .HasValue<ReasoningContentEntity>(ContentTypeConstants.Reasoning)
            .HasValue<DataContentEntity>(ContentTypeConstants.Data)
            .HasValue<UriContentEntity>(ContentTypeConstants.Uri)
            .HasValue<ErrorContentEntity>(ContentTypeConstants.Error)
            .HasValue<FunctionCallContentEntity>(ContentTypeConstants.FunctionCall)
            .HasValue<FunctionResultContentEntity>(ContentTypeConstants.FunctionResult)
            .HasValue<UsageContentEntity>(ContentTypeConstants.Usage);

        // Configure indexes for performance
        modelBuilder.Entity<ChatConversationEntity>()
            .HasIndex(e => e.ConversationId)
            .IncludeProperties(e => e.CreatedAt);

        modelBuilder.Entity<ChatMessageEntity>()
            .HasIndex(e => new { e.ConversationId, e.OrderIndex })
            .IncludeProperties(e => e.CreatedAt);

        modelBuilder.Entity<ChatResponseEntity>()
            .HasIndex(e => e.ConversationId)
            .IncludeProperties(e => e.ResponseId);

        modelBuilder.Entity<BaseContentEntity>()
            .HasIndex(e => new { e.MessageId, e.OrderIndex });

        // Configure enum conversions
        modelBuilder.Entity<ChatMessageEntity>()
            .Property(e => e.Role)
            .HasConversion<string>();

        modelBuilder.Entity<ChatResponseEntity>()
            .Property(e => e.FinishReason)
            .HasConversion<int>();

        base.OnModelCreating(modelBuilder);
    }
}