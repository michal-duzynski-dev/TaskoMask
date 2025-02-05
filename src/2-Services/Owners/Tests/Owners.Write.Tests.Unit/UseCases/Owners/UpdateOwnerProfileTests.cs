﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using NSubstitute;
using TaskoMask.BuildingBlocks.Application.Exceptions;
using TaskoMask.BuildingBlocks.Contracts.Events;
using TaskoMask.BuildingBlocks.Contracts.Resources;
using TaskoMask.BuildingBlocks.Domain.Resources;
using TaskoMask.Services.Owners.Write.Api.Domain.Owners.Events.Owners;
using TaskoMask.Services.Owners.Write.Api.UseCases.Owners.UpdateOwnerProfile;
using TaskoMask.Services.Owners.Write.Tests.Unit.Fixtures;
using Xunit;

namespace TaskoMask.Services.Owners.Write.Tests.Unit.UseCases.Owners;

public class UpdateOwnerProfileTests : TestsBaseFixture
{
    #region Fields

    private UpdateOwnerProfileUseCase updateOwnerProfileUseCase;

    #endregion

    #region Ctor

    public UpdateOwnerProfileTests() { }

    #endregion

    #region Test Methods


    [Fact]
    public async Task Owner_Profile_is_updated()
    {
        //Arrange
        var expectedOwner = Owners.FirstOrDefault();
        var updateOwnerProfileRequest = new UpdateOwnerProfileRequest(expectedOwner.Id, "Test_New_DisplayName", "Test_New@email.com");

        //Act
        var result = await updateOwnerProfileUseCase.Handle(updateOwnerProfileRequest, CancellationToken.None);

        //Assert
        result.EntityId.Should().Be(expectedOwner.Id);
        expectedOwner.Email.Value.Should().Be(updateOwnerProfileRequest.Email);
        await InMemoryBus.Received(1).PublishEvent(Arg.Any<OwnerProfileUpdatedEvent>());
        await MessageBus.Received(1).Publish(Arg.Any<OwnerProfileUpdated>());
    }

    [Fact]
    public async Task Owner_profile_update_throw_exception_when_owner_id_is_not_existed()
    {
        //Arrange
        var notExistedOwnerId = ObjectId.GenerateNewId().ToString();
        var updateOwnerProfileRequest = new UpdateOwnerProfileRequest(notExistedOwnerId, "Test_New_DisplayName", "Test_New@email.com");
        var expectedMessage = string.Format(ContractsMessages.Data_Not_exist, DomainMetadata.Owner);

        //Act
        System.Func<Task> act = async () => await updateOwnerProfileUseCase.Handle(updateOwnerProfileRequest, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<ApplicationException>().Where(e => e.Message.Equals(expectedMessage));
    }

    #endregion

    #region Fixture

    protected override void TestClassFixtureSetup()
    {
        updateOwnerProfileUseCase = new UpdateOwnerProfileUseCase(OwnerAggregateRepository, OwnerValidatorService, MessageBus, InMemoryBus);
    }

    #endregion
}
