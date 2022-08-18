﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TaskoMask.BuildingBlocks.Contracts.Dtos.Workspace.Comments;
using TaskoMask.BuildingBlocks.Contracts.Helpers;

namespace TaskoMask.Services.Monolith.Application.Workspace.Comments.Services
{
    public  interface ICommentService
    {
        Task<Result<CommentBasicInfoDto>> GetByIdAsync(string id);
        Task<Result<IEnumerable<CommentBasicInfoDto>>> GetListByTaskIdAsync(string taskId);
        Task<Result<CommandResult>> AddAsync(AddCommentDto input);
        Task<Result<CommandResult>> UpdateAsync(UpdateCommentDto input);
        Task<Result<CommandResult>> DeleteAsync(string id);
    }
}