﻿using MassTransit;
using TaskoMask.BuildingBlocks.Application.Bus;
using TaskoMask.BuildingBlocks.Application.Notifications;
using TaskoMask.BuildingBlocks.Web.MVC.Consumers;
using TaskoMask.BuildingBlocks.Contracts.Events;
using TaskoMask.Services.Owners.Write.Domain.Data;

namespace TaskoMask.Services.Owners.Write.Api.Consumers
{
    /// <summary>
    /// UserRegistered consume by identity service and it publish UserRegistered after registering user for the owner
    /// it means everything is ok about the current owner registration and now we must publish OwnerRegisterationCompleted to be consumed by 
    /// owner read service for updating its database
    /// </summary>
    public class UserRegisteredConsumer : BaseConsumer<NewUserRegistered>
    {
        private readonly IOwnerAggregateRepository _ownerAggregateRepository;


        public UserRegisteredConsumer(IInMemoryBus inMemoryBus, INotificationHandler notifications , IOwnerAggregateRepository ownerAggregateRepository) : base(inMemoryBus, notifications)
        {
            _ownerAggregateRepository = ownerAggregateRepository;
        }


        public override async Task ConsumeMessage(ConsumeContext<NewUserRegistered> context)
        {
            var owner = await _ownerAggregateRepository.GetByEmailAsync(context.Message.Email);

            await context.Publish(new OwnerRegisterationCompleted(owner.Id, owner.Email.Value, owner.DisplayName.Value));
        }
    }
}
