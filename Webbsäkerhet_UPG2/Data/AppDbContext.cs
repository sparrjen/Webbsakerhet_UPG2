using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Webbsäkerhet_UPG2.Models;

namespace Webbsäkerhet_UPG2.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Comment> Comment { get; set; }

        public DbSet<ForumFile> ForumFile { get; set; }
    }
}
    
