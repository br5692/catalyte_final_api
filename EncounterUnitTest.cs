using Catalyte.Apparel.Data.Interfaces;
using Catalyte.Apparel.Data.Model;
using Catalyte.Apparel.Providers.Interfaces;
using Catalyte.Apparel.Providers.Providers;
using Catalyte.Apparel.Utilities.HttpResponseExceptions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Catalyte.Apparel.Test.Unit
{
    public class EncounterUnitTests
    {
        private readonly List<Encounter> encounters;
        private readonly IEncounterProvider encounterProvider;
        private readonly Mock<IEncounterRepository> encounterRepo;
        private readonly Mock<ILogger<EncounterProvider>> logger;

        public EncounterUnitTests()
        {
            encounterRepo = new Mock<IEncounterRepository>();
            logger = new Mock<ILogger<EncounterProvider>>();
            encounterProvider = new EncounterProvider(encounterRepo.Object, logger.Object);

            encounters = new List<Encounter>()
            {
                new Encounter()
                {
                    Id = 1,
                    PatientId = 1,
                    Notes = "new note",
                    VisitCode = "N3W 3C3",
                    Provider = "new provider",
                    BillingCode = "123.456.789-00",
                    Icd10 = "Z99",
                    TotalCost = 150.30m,
                    Copay = 10.10m,
                    ChiefComplaint = "Pain",
                    Pulse = 110,
                    Systolic = 60,
                    Diastolic = 90,
                    Date = "2020-03-15"


                },
                new Encounter()
                {

                    Id = 2,
                    PatientId = 2,
                    Notes = "new note",
                    VisitCode = "N3W 3C3",
                    Provider = "new provider",
                    BillingCode = "123.456.789-00",
                    Icd10 = "Z99",
                    TotalCost = 150.30m,
                    Copay = 10.10m,
                    ChiefComplaint = "Pain",
                    Pulse = 110,
                    Systolic = 60,
                    Diastolic = 90,
                    Date = "2020-03-15"

                }
            };

        }

        [Fact]
        public async void GetAllEncountersAsync_ReturnsAllEncounters()
        {
            var encounterList = encounters;
            encounterRepo.Setup(p => p.GetAllEncountersAsync()).ReturnsAsync(encounters);

            var actual = await encounterProvider.GetAllEncountersAsync();
            Assert.Same(encounterList, actual);
            Assert.Equal(encounterList, actual);
        }

        [Fact]
        public async void GetEncountersAsync_DatabaseErrorReturnsException()
        {
            var exception = new ServiceUnavailableException("There was a problem connecting to the database.");

            encounterRepo.Setup(m => m.GetAllEncountersAsync()).ThrowsAsync(exception);
            try
            {
                await encounterProvider.GetAllEncountersAsync();
            }
            catch (Exception ex)
            {
                Assert.Same(ex.GetType(), exception.GetType());
                Assert.Equal(ex.Message, exception.Message);
            }
        }

        [Fact]
        public async void GetEncountersByIdAsync_IdExistsReturnsEncounter()
        {
            var encounter = encounters.FirstOrDefault(x => x.Id == 1);
            encounterRepo.Setup(m => m.GetEncounterByIdAsync(1)).ReturnsAsync(encounter);

            var actual = await encounterProvider.GetEncounterByIdAsync(1);
            Assert.Same(encounter, actual);
            Assert.Equal(1, actual.Id);
        }

        [Fact]
        public async void GetEncounterByIdAsync_DatabaseErrorReturnsException()
        {
            var exception = new ServiceUnavailableException("There was a problem connecting to the database.");

            encounterRepo.Setup(m => m.GetEncounterByIdAsync(1)).ThrowsAsync(exception);
            try
            {
                await encounterProvider.GetEncounterByIdAsync(1);
            }
            catch (Exception ex)
            {
                Assert.Same(ex.GetType(), exception.GetType());
                Assert.Equal(ex.Message, exception.Message);
            }
        }

        [Fact]
        public async void GetEncounterByIdAsync_IdIsNullReturnsNotFoundError()
        {
            Encounter encounter = null;
            var encounterId = 2;
            var exception = new NotFoundException($"Encounter with id: {encounterId} could not be found.");

            encounterRepo.Setup(m => m.GetEncounterByIdAsync(encounterId)).ReturnsAsync(encounter);
            try
            {
                await encounterProvider.GetEncounterByIdAsync(encounterId);
            }
            catch (Exception ex)
            {
                Assert.Same(ex.GetType(), exception.GetType());
                Assert.Equal(ex.Message, exception.Message);
            }
        }

        [Fact]
        public async void GetEncounterByIdAsync_IdIsDefaultReturnsNotFoundError()
        {
            Encounter encounter = default;
            var encounterId = 2;
            var exception = new NotFoundException($"Encounter with id: {encounterId} could not be found.");

            encounterRepo.Setup(m => m.GetEncounterByIdAsync(encounterId)).ReturnsAsync(encounter);
            try
            {
                await encounterProvider.GetEncounterByIdAsync(encounterId);
            }
            catch (Exception ex)
            {
                Assert.Same(ex.GetType(), exception.GetType());
                Assert.Equal(ex.Message, exception.Message);
            }
        }

        [Fact]
        public async Task CreateEncounterAsync_CreatesNewEncounter_ReturnsSavedEncounter()
        {
            var newEncounter = new Encounter
            {
                PatientId = 2,
                Notes = "new note",
                VisitCode = "N3W 3C3",
                Provider = "new provider",
                BillingCode = "123.456.789-00",
                Icd10 = "Z99",
                TotalCost = 150.30m,
                Copay = 10.10m,
                ChiefComplaint = "Pain",
                Pulse = 110,
                Systolic = 60,
                Diastolic = 90,
                Date = "2020-03-15"
            };

            encounterRepo.Setup(m => m.CreateEncounterAsync(newEncounter)).ReturnsAsync(newEncounter);
            var actual = await encounterProvider.CreateEncounterAsync(newEncounter);
            var expected = newEncounter;

            Assert.NotNull(actual);
            Assert.IsType<Encounter>(actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task CreateEncounterAsync_RequestDoesNotReachDatabase_ReturnsServiceUnavailableException()
        {
            var newEncounter = new Encounter
            {
                PatientId = 2,
                Notes = "new note",
                VisitCode = "N3W 3C3",
                Provider = "new provider",
                BillingCode = "123.456.789-00",
                Icd10 = "Z99",
                TotalCost = 150.30m,
                Copay = 10.10m,
                ChiefComplaint = "Pain",
                Pulse = 110,
                Systolic = 60,
                Diastolic = 90,
                Date = "2020-03-15"
            };

            encounterRepo.Setup(m => m.CreateEncounterAsync(newEncounter)).ThrowsAsync(
                new Exception("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => encounterProvider.CreateEncounterAsync(newEncounter));
        }

        [Fact]
        public async Task UpdateEncounterAsyncGetEncounterByIdDoesNotReachDatabase_ReturnsServiceUnavailableExcpetion()
        {

            var encounter = encounters.FirstOrDefault(m => m.Id == 1);
            var newEncounter = new Encounter
            {
                Id = 1,
                PatientId = 1,
                Notes = "new note",
                VisitCode = "N3W 3C3",
                Provider = "new provider",
                BillingCode = "123.456.789-00",
                Icd10 = "Z99",
                TotalCost = 150.30m,
                Copay = 10.10m,
                ChiefComplaint = "Pain",
                Pulse = 110,
                Systolic = 60,
                Diastolic = 90,
                Date = "2020-03-15"
            };

            encounterRepo.Setup(m => m.GetEncounterByIdAsync(newEncounter.Id)).ThrowsAsync(
                new ServiceUnavailableException("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => encounterProvider.GetEncounterByIdAsync(newEncounter.Id));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => encounterProvider.UpdateEncounterAsync(
                encounter.Id, newEncounter));
        }

        [Fact]
        public async Task UpdateEncounterByIdAsync_GetRequestDoesNotReachDatabase_ReturnsServiceUnavailableExcpetion()
        {
            var encounter = encounters.FirstOrDefault(r => r.Id == 1);
            var updatedEncounter = new Encounter
            {
                Id = 2,
                PatientId = 2,
                Notes = "new note",
                VisitCode = "N3W 3C3",
                Provider = "new provider",
                BillingCode = "123.456.789-00",
                Icd10 = "Z99",
                TotalCost = 150.30m,
                Copay = 10.10m,
                ChiefComplaint = "Pain",
                Pulse = 110,
                Systolic = 60,
                Diastolic = 90,
                Date = "2020-03-15"
            };

            encounterRepo.Setup(m => m.GetEncounterByIdAsync(encounter.Id)).ThrowsAsync(
                new Exception("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => encounterProvider.GetEncounterByIdAsync(encounter.Id));
            await Assert.ThrowsAsync<ServiceUnavailableException>(() => encounterProvider.UpdateEncounterAsync(
                encounter.Id, updatedEncounter));
        }

        [Fact]
        public async Task UpdateEncounterAsync_RequestDoesNotReachDatabase_ReturnsServiceUnavailableException1()
        {
            var encounter = encounters.FirstOrDefault(r => r.Id == 1);
            var newEncounter = new Encounter
            {
                Id = 1,
                PatientId = 1,
                Notes = "new note",
                VisitCode = "N3W 3C3",
                Provider = "new provider",
                BillingCode = "123.456.789-00",
                Icd10 = "Z99",
                TotalCost = 150.30m,
                Copay = 10.10m,
                ChiefComplaint = "Pain",
                Pulse = 110,
                Systolic = 60,
                Diastolic = 90,
                Date = "2020-03-15"
            };
            encounterRepo.Setup(m => m.GetEncounterByIdAsync(encounter.Id)).ReturnsAsync(encounter);
            encounterRepo.Setup(m => m.UpdateEncounterAsync(newEncounter)).ThrowsAsync(
                new Exception("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => encounterProvider.UpdateEncounterAsync(newEncounter.Id, newEncounter));
        }

        [Fact]
        public async Task UpdateEncounterByIdAsync_EncounterIdIsOneAndEncounterIsDefault_ReturnsNotFoundException()
        {
            Encounter encounter = default;
            var encounterId = 1;
            var updatedEncounter = new Encounter()
            {
                Id = 2,
                PatientId = 2,
                Notes = "new note",
                VisitCode = "N3W 3C3",
                Provider = "new provider",
                BillingCode = "123.456.789-00",
                Icd10 = "Z99",
                TotalCost = 150.30m,
                Copay = 10.10m,
                ChiefComplaint = "Pain",
                Pulse = 110,
                Systolic = 60,
                Diastolic = 90,
                Date = "2020-03-15"
            };
            var exception = new NotFoundException($"Encounter with id: {encounterId} not found.");

            encounterRepo.Setup(m => m.GetEncounterByIdAsync(encounterId)).ReturnsAsync(encounter);
            encounterRepo.Setup(m => m.UpdateEncounterAsync(updatedEncounter)).ThrowsAsync(
                new NotFoundException($"Encounter with id: {encounterId} not found."));

            await Assert.ThrowsAsync<NotFoundException>(() => encounterProvider.UpdateEncounterAsync(encounterId, updatedEncounter));
        }

        [Fact]
        public async Task UpdateEncounterByIdAsync_EncounterIdIsOneAndEncounterIsNull_ReturnsNotFoundException()
        {
            Encounter encounter = null;
            var encounterId = 1;
            var updatedEncounter = new Encounter()
            {
                Id = 2,
                PatientId = 2,
                Notes = "new note",
                VisitCode = "N3W 3C3",
                Provider = "new provider",
                BillingCode = "123.456.789-00",
                Icd10 = "Z99",
                TotalCost = 150.30m,
                Copay = 10.10m,
                ChiefComplaint = "Pain",
                Pulse = 110,
                Systolic = 60,
                Diastolic = 90,
                Date = "2020-03-15"
            };
            var exception = new NotFoundException($"Encounter with id: {encounterId} not found.");

            encounterRepo.Setup(m => m.GetEncounterByIdAsync(encounterId)).ReturnsAsync(encounter);
            encounterRepo.Setup(m => m.UpdateEncounterAsync(updatedEncounter)).ThrowsAsync(
                new NotFoundException($"Encounter with id: {encounterId} not found."));

            await Assert.ThrowsAsync<NotFoundException>(() => encounterProvider.UpdateEncounterAsync(encounterId, updatedEncounter));
        }

        [Fact]
        public async Task UpdateEncounterAsync_ChangeEncounterICD10_ReturnsUpdatedEncounter()
        {
            var encounter = encounters.FirstOrDefault(r => r.Id == 1);

            var updatedEncounter = new Encounter()
            {
                Id = 1,
                PatientId = 1,
                Notes = "new note",
                VisitCode = "N3W 3C3",
                Provider = "new provider",
                BillingCode = "123.456.789-00",
                Icd10 = "A44",
                TotalCost = 150.30m,
                Copay = 10.10m,
                ChiefComplaint = "Pain",
                Pulse = 110,
                Systolic = 60,
                Diastolic = 90,
                Date = "2020-03-15"
            };

            encounterRepo.Setup(m => m.GetEncounterByIdAsync(encounter.Id)).ReturnsAsync(encounter);
            encounterRepo.Setup(m => m.UpdateEncounterAsync(updatedEncounter)).ReturnsAsync(updatedEncounter);
            var expected = updatedEncounter;
            var actual = await encounterProvider.UpdateEncounterAsync(encounter.Id, updatedEncounter);

            Assert.Same(expected, actual);
        }

    }
}

