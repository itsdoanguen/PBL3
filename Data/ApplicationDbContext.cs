using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<StoryModel> Stories { get; set; }
        public DbSet<ChapterModel> Chapters { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<FollowStoryModel> FollowStories { get; set; }
        public DbSet<FollowUserModel> FollowUsers { get; set; }
        public DbSet<LikeChapterModel> LikeChapters { get; set; }
        public DbSet<BookmarkModel> Bookmarks { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<GenreModel> Genres { get; set; }
        public DbSet<StoryGenreModel> StoryGenres { get; set; }
        public DbSet<StyleModel> Styles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FollowUserModel>().HasKey(f => new { f.FollowerID, f.FollowingID });
            modelBuilder.Entity<FollowUserModel>()
                .HasOne(f => f.Follower).
                WithMany(u => u.Followings).
                HasForeignKey(f => f.FollowerID).
                OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FollowUserModel>()
                .HasOne(f => f.Following).
                WithMany(u => u.Followers).
                HasForeignKey(f => f.FollowingID).
                OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookmarkModel>().HasKey(bm => new { bm.UserID, bm.ChapterID });
            modelBuilder.Entity<BookmarkModel>()
                .HasOne(bm => bm.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(bm => bm.UserID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<BookmarkModel>()
                .HasOne(bm => bm.Chapter)
                .WithMany(c => c.Bookmarks)
                .HasForeignKey(bm => bm.ChapterID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LikeChapterModel>().HasKey(lk => new { lk.UserID, lk.ChapterID });
            modelBuilder.Entity<LikeChapterModel>()
                .HasOne(lk => lk.User)
                .WithMany(u => u.LikeChapters)
                .HasForeignKey(lk => lk.UserID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<LikeChapterModel>()
                .HasOne(lk => lk.Chapter)
                .WithMany(c => c.Likes)
                .HasForeignKey(lk => lk.ChapterID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FollowStoryModel>()
                .HasKey(fs => new { fs.UserID, fs.StoryID });
            modelBuilder.Entity<FollowStoryModel>()
                .HasOne(fs => fs.User)
                .WithMany(u => u.FollowStories)
                .HasForeignKey(fs => fs.UserID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<FollowStoryModel>()
                .HasOne(fs => fs.Story)
                .WithMany(s => s.Followers)
                .HasForeignKey(fs => fs.StoryID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.Chapter)
                .WithMany(ch => ch.Comments)
                .HasForeignKey(c => c.ChapterID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<StoryGenreModel>().HasKey(sg => new { sg.StoryID, sg.GenreID });

            // NotificationModel relations
            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.FromUser)
                .WithMany()
                .HasForeignKey(n => n.FromUserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.Story)
                .WithMany()
                .HasForeignKey(n => n.StoryID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.Chapter)
                .WithMany()
                .HasForeignKey(n => n.ChapterID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.Comment)
                .WithMany()
                .HasForeignKey(n => n.CommentID)
                .OnDelete(DeleteBehavior.NoAction);

            // HistoryModel relations
            modelBuilder.Entity<HistoryModel>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HistoryModel>()
                .HasOne(h => h.Story)
                .WithMany()
                .HasForeignKey(h => h.StoryID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<HistoryModel>()
                .HasOne(h => h.Chapter)
                .WithMany()
                .HasForeignKey(h => h.ChapterID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
