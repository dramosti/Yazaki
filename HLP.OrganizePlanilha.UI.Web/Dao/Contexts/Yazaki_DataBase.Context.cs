﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HLP.OrganizePlanilha.UI.Web.Dao.Contexts
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DB_YAZAKIEntities : DbContext
    {
        public DB_YAZAKIEntities()
            : base("name=DB_YAZAKIEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<TB_MAQUINA> TB_MAQUINA { get; set; }
        public virtual DbSet<TB_PLANILHA> TB_PLANILHA { get; set; }
        public virtual DbSet<TB_PROJETO> TB_PROJETO { get; set; }
    }
}
