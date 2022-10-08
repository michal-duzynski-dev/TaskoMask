﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TaskoMask.Services.Owner.Infrastructure.Data.DbContext;
using System;

namespace TaskoMask.Services.Owner.Infrastructure.Data.DataProviders
{
    public static class DbInitialization
    {


        /// <summary>
        /// 
        /// </summary>
        public static void InitialDatabase(this IServiceProvider serviceProvider)
        {
            using var serviceScope = serviceProvider.CreateScope();

            var dbContext = serviceScope.ServiceProvider.GetService<OwnerDbContext>();

            dbContext.Database.EnsureCreated();
        }




        /// <summary>
        /// 
        /// </summary>
        public static void DropDatabase(this IServiceProvider serviceProvider)
        {
            using var serviceScope = serviceProvider.CreateScope();

            var dbContext = serviceScope.ServiceProvider.GetService<OwnerDbContext>();

            dbContext.Database.EnsureDeleted();
        }
    }
}
