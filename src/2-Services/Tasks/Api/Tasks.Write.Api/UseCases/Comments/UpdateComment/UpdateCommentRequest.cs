﻿using System.ComponentModel.DataAnnotations;
using TaskoMask.BuildingBlocks.Application.Commands;
using TaskoMask.BuildingBlocks.Contracts.Helpers;
using TaskoMask.BuildingBlocks.Contracts.Resources;

namespace TaskoMask.Services.Tasks.Write.Api.UseCases.Comments.UpdateComment;

public class UpdateCommentRequest : BaseCommand
{
    public UpdateCommentRequest(string id, string content)
    {
        Id = id;
        Content = content;
    }

    [Required(ErrorMessageResourceName = nameof(ContractsMetadata.Required), ErrorMessageResourceType = typeof(ContractsMetadata))]
    public string Id { get; }

    [MaxLength(
        DomainConstValues.COMMENT_CONTENT_MAX_LENGTH,
        ErrorMessageResourceName = nameof(ContractsMetadata.Max_Length_Error),
        ErrorMessageResourceType = typeof(ContractsMetadata)
    )]
    [Required(ErrorMessageResourceName = nameof(ContractsMetadata.Required), ErrorMessageResourceType = typeof(ContractsMetadata))]
    public string Content { get; }
}
