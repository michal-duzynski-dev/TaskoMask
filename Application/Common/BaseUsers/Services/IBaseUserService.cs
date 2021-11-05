﻿using TaskoMask.Application.Core.Helpers;
using System.Threading.Tasks;
using TaskoMask.Application.Core.Dtos.Common.Users;
using TaskoMask.Application.Common.BaseEntities.Services;
using TaskoMask.Application.Core.Commands;

namespace TaskoMask.Application.Common.BaseEntitiesUsers.Services
{
    public interface IBaseUserService : IBaseEntityService
    {
        Task<Result<UserBasicInfoDto>> GetBaseUserByIdAsync(string id);
        Task<Result<UserBasicInfoDto>> GetBaseUserByUserNameAsync(string userName);
        Task<Result<UserBasicInfoDto>> GetBaseUserByPhoneNumberAsync(string phoneNumber);
        Task<Result<bool>> ValidateUserPasswordAsync(string userName,string password);
        Task<Result<CommandResult>> SetIsActiveAsync(string id, bool isActive);
        Task<Result<CommandResult>> ChangePasswordAsync(string id, string oldPassword, string newPassword);
        Task<Result<CommandResult>> ResetPasswordAsync(string id, string newPassword);

    }
}
