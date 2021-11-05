﻿using AutoMapper;
using TaskoMask.Application.Core.Dtos.Common.Users;

using TaskoMask.Application.Mapper.MappingActions;
using TaskoMask.Application.Core.Dtos.Administration.Operators;
using TaskoMask.Application.Core.Dtos.Team.Members;
using TaskoMask.Domain.Core.Models;
using TaskoMask.Domain.Team.Entities;
using TaskoMask.Domain.Administration.Entities;

namespace TaskoMask.Application.Mapper.Profiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {

            CreateMap<BaseUser, UserBasicInfoDto>()
              .AfterMap<UserMappingAction>();

            CreateMap<UserBasicInfoDto, AuthenticatedUser>();

            CreateMap<BaseUser, UserUpsertDto>()
                .AfterMap<UserMappingAction>();

            CreateMap<UserBasicInfoDto, UserUpsertDto>();
            CreateMap<UserBasicInfoDto, UserBaseDto>();

            CreateMap<Member, UserBasicInfoDto>();
            CreateMap<Operator, UserBasicInfoDto>();

            CreateMap<Member, MemberBasicInfoDto>();
            CreateMap<Operator, OperatorBasicInfoDto>();

            CreateMap<Operator, OperatorOutputDto>();
            CreateMap<Operator, OperatorUpsertDto>();

        }
    }
}
