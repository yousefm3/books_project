﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BooksExchange.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class book_exchangeEntities : DbContext
    {
        public book_exchangeEntities()
            : base("name=book_exchangeEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<comment> comments { get; set; }
        public virtual DbSet<favBook> favBooks { get; set; }
        public virtual DbSet<Genera> Generas { get; set; }
        public virtual DbSet<GiftRequest> GiftRequests { get; set; }
        public virtual DbSet<message> messages { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<PostsGenera> PostsGeneras { get; set; }
        public virtual DbSet<recommendtion> recommendtions { get; set; }
        public virtual DbSet<UserRate> UserRates { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
