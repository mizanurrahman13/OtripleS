﻿using Moq;
using OtripleS.Web.Api.Models.Registrations;
using OtripleS.Web.Api.Models.Registrations.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OtripleS.Web.Api.Tests.Unit.Services.Registrations
{
    public partial class RegistrationServiceTests
    {
        [Fact]
        public async Task ShouldThrowDependencyExceptionOnAddWhenSqlExceptionOccursAndLogItAsync()
        {
            // given
            DateTimeOffset dateTime = GetRandomDateTime();
            Registration randomRegistration = CreateRandomRegistration(dateTime);
            Registration inputRegistration = randomRegistration;
            inputRegistration.UpdatedBy = inputRegistration.CreatedBy;
            var sqlException = GetSqlException();

            var expectedRegistrationDependencyException =
                new RegistrationDependencyException(sqlException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTime())
                    .Returns(dateTime);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertRegistrationAsync(inputRegistration))
                    .ThrowsAsync(sqlException);

            // when
            ValueTask<Registration> createRegistrationTask =
                this.registrationService.AddRegistrationAsync(inputRegistration);

            // then
            await Assert.ThrowsAsync<RegistrationDependencyException>(() =>
                createRegistrationTask.AsTask());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(expectedRegistrationDependencyException))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertRegistrationAsync(inputRegistration),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTime(),
                    Times.Once);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
