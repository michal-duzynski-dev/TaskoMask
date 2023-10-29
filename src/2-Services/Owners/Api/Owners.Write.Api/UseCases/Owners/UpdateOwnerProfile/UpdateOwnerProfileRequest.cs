﻿using System.ComponentModel.DataAnnotations;
using TaskoMask.BuildingBlocks.Application.Commands;
using TaskoMask.BuildingBlocks.Contracts.Helpers;
using TaskoMask.BuildingBlocks.Contracts.Resources;

namespace TaskoMask.Services.Owners.Write.Api.UseCases.Owners.UpdateOwnerProfile
{
    public class UpdateOwnerProfileRequest : BaseCommand
    {
        public UpdateOwnerProfileRequest(string id, string displayName, string email)
        {
            DisplayName = displayName;
            Email = email;
            Id = id;
        }

        [StringLength(
            DomainConstValues.Owner_DisplayName_Max_Length,
            MinimumLength = DomainConstValues.Owner_DisplayName_Min_Length,
            ErrorMessageResourceName = nameof(ContractsMetadata.Length_Error),
            ErrorMessageResourceType = typeof(ContractsMetadata)
        )]
        [Required(ErrorMessageResourceName = nameof(ContractsMetadata.Required), ErrorMessageResourceType = typeof(ContractsMetadata))]
        public string DisplayName { get; }

        [Required(ErrorMessageResourceName = nameof(ContractsMetadata.Required), ErrorMessageResourceType = typeof(ContractsMetadata))]
        [StringLength(
            DomainConstValues.Owner_Email_Max_Length,
            MinimumLength = DomainConstValues.Owner_Email_Min_Length,
            ErrorMessageResourceName = nameof(ContractsMetadata.Length_Error),
            ErrorMessageResourceType = typeof(ContractsMetadata)
        )]
        [EmailAddress]
        public string Email { get; }

        [Required(ErrorMessageResourceName = nameof(ContractsMetadata.Required), ErrorMessageResourceType = typeof(ContractsMetadata))]
        public string Id { get; }
    }
}
