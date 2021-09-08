﻿using TaskoMask.Domain.Core.Events;
using TaskoMask.Domain.Entities;

namespace TaskoMask.Domain.Events
{
    public class OperatorCreatedEvent : DomainEvent
    {
        public OperatorCreatedEvent(string id, string displayName, string email, string userName) : base(entityId: id, entityType: nameof(Operator))
        {
            Id = id;
            DisplayName = displayName;
            UserName = userName;
            Email = email;
        }


        public string Id { get; }
        public string DisplayName { get; }
        public string UserName { get; }
        public string Email { get; }
    }
}
