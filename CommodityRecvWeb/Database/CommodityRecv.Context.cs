﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CommodityRecvWeb.Database
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CommodityRecv_DashboardEntities : DbContext
    {
        public CommodityRecv_DashboardEntities()
            : base("name=CommodityRecv_DashboardEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<sys_user> sys_user { get; set; }
        public virtual DbSet<tbl_cr_Baan> tbl_cr_Baan { get; set; }
        public virtual DbSet<tbl_cr_Condition> tbl_cr_Condition { get; set; }
        public virtual DbSet<tbl_cr_mailReceiver> tbl_cr_mailReceiver { get; set; }
        public virtual DbSet<v_CommodityRecvCmp> v_CommodityRecvCmp { get; set; }
    }
}
