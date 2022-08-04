﻿using System.ComponentModel.DataAnnotations;
using TaskoMask.Domain.Share.Resources;

namespace TaskoMask.Application.Workspace.Organizations.Commands.Models
{
   public class AddOrganizationToOwnerWorkspaceCommand : OrganizationBaseCommand
    {
        public AddOrganizationToOwnerWorkspaceCommand(string name, string description, string ownerId)
            :base(name,description)
        {
            OwnerId = ownerId;
        }

        [Required(ErrorMessageResourceName = nameof(DomainMessages.Required), ErrorMessageResourceType = typeof(DomainMessages))]
        public string OwnerId { get; }
    }
}