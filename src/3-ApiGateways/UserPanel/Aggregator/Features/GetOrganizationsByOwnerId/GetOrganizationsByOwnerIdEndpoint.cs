using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskoMask.BuildingBlocks.Application.Bus;
using TaskoMask.BuildingBlocks.Contracts.Helpers;
using TaskoMask.BuildingBlocks.Contracts.Services;
using TaskoMask.BuildingBlocks.Contracts.ViewModels;
using TaskoMask.BuildingBlocks.Web.MVC.Controllers;

namespace TaskoMask.ApiGateways.UserPanel.Aggregator.Features.GetOrganizationsByOwnerId
{

    [Authorize("user-read-access")]
    public partial class OrganizationsController : BaseApiController
    {
        public OrganizationsController(IAuthenticatedUserService authenticatedUserService, IInMemoryBus inMemoryBus) : base(authenticatedUserService, inMemoryBus)
        {
        }



        /// <summary>
        /// get organizations with detail information for current owner
        /// </summary>
        [HttpGet]
        [Route("organizations")]
        public async Task<Result<IEnumerable<OrganizationDetailsViewModel>>> GetOrganizationsByOwnerId()
        {
            return await _inMemoryBus.SendQuery(new GetOrganizationsByOwnerIdRequest(GetCurrentUserId()));
        }
    }

}