﻿using FluentAssertions;
using System.Threading.Tasks;
using TaskoMask.Application.Tests.Integration.TestData;
using TaskoMask.Application.Tests.Integration.TestData.Fixtures;
using TaskoMask.Application.Workspace.Projects.Services;
using Xunit;
using Xunit.Priority;

namespace TaskoMask.Application.Tests.Integration.Workspace.OwnerTestCollection
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("Owner Collection Fixture")]
    public class OTC3_ProjectServiceIntegrationTests
    {
        #region Fields

        private readonly IProjectService _projectService;
        private readonly OwnerCollectionFixture _fixture;

        #endregion

        #region Ctor

        public OTC3_ProjectServiceIntegrationTests(OwnerCollectionFixture fixture)
        {
            _fixture = fixture;
            _projectService = _fixture.GetRequiredService<IProjectService>();
        }

        #endregion

        #region Test Mthods






        #endregion

    }
}