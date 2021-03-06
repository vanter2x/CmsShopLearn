﻿using System.Data.Entity;

namespace CmsShop.Models.Data
{
    public class Db: DbContext
    {
        public DbSet<PageDTO> Pages { get; set; }
        public DbSet<SidebarDTO> Sidebar { get; set; }
        public DbSet<CategoryDto> Categories { get; set; }
        public DbSet<ProductDto> Products { get; set; }
    }
}