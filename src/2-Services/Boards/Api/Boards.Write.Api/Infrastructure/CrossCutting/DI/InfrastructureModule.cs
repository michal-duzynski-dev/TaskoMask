﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TaskoMask.BuildingBlocks.Infrastructure.Extensions;
using TaskoMask.BuildingBlocks.Infrastructure.MongoDB;
using TaskoMask.Services.Boards.Write.Api.Domain.Boards.Data;
using TaskoMask.Services.Boards.Write.Api.Domain.Boards.Services;
using TaskoMask.Services.Boards.Write.Api.Infrastructure.Data.DbContext;
using TaskoMask.Services.Boards.Write.Api.Infrastructure.Data.Repositories;
using TaskoMask.Services.Boards.Write.Api.Infrastructure.Data.Services;
using TaskoMask.Services.Boards.Write.Api.Resources;

namespace TaskoMask.Services.Boards.Write.Api.Infrastructure.CrossCutting.DI;

/// <summary>
///
/// </summary>
public static class InfrastructureModule
{
    /// <summary>
    ///
    /// </summary>
    public static void AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration, Type consumerAssemblyMarkerType)
    {
        services.AddBuildingBlocksInfrastructure(configuration, consumerAssemblyMarkerType, handlerAssemblyMarkerType: typeof(ApplicationMessages));

        services.AddMongoDbContext(configuration);

        services.AddDomainServices();

        services.AddRepositories();
    }

    /// <summary>
    ///
    /// </summary>
    private static void AddMongoDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("MongoDB");
        services.AddScoped<BoardWriteDbContext>().AddOptions<MongoDbOptions>().Bind(options);
    }

    /// <summary>
    ///
    /// </summary>
    private static void AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IBoardValidatorService, BoardValidatorService>();
    }

    /// <summary>
    ///
    /// </summary>
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBoardAggregateRepository, BoardAggregateRepository>();
    }
}
