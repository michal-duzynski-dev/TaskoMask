﻿using FluentValidation;

namespace TaskoMask.Services.Owners.Write.Api.UseCases.Organizations.DeleteOrganization
{
    public abstract class DeleteOrganizationValidation<TRequest> : AbstractValidator<TRequest>
        where TRequest : DeleteOrganizationRequest
    {
        public DeleteOrganizationValidation() { }
    }
}
