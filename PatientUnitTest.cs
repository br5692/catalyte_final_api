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
    public class PatientUnitTests
    {
        private readonly List<Patient> patients;
        private readonly IPatientProvider patientProvider;
        private readonly Mock<IPatientRepository> patientRepo;
        private readonly Mock<ILogger<PatientProvider>> logger;

        public PatientUnitTests()
        {
            patientRepo = new Mock<IPatientRepository>();
            logger = new Mock<ILogger<PatientProvider>>();
            patientProvider = new PatientProvider(patientRepo.Object, logger.Object);

            patients = new List<Patient>()
            {
                new Patient()
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Ssn = "123-12-1234",
                    Email = "email1@mail.com",
                    Age = 29,
                    Height = 80,
                    Weight = 150,
                    Insurance = "Self-Insured",
                    Gender = "Male",
                    Street = "1 Main St",
                    City = "New York",
                    State = "NY",
                    ZipCode = "29445"


                },
                new Patient()
                {
                   Id = 2,
                    FirstName = "John",
                    LastName = "Smith",
                    Ssn = "123-12-1234",
                    Email = "email2@mail.com",
                    Age = 29,
                    Height = 80,
                    Weight = 150,
                    Insurance = "Self-Insured",
                    Gender = "Male",
                    Street = "1 Main St",
                    City = "New York",
                    State = "NY",
                    ZipCode = "29445"

                }
            };

        }

        [Fact]
        public async void GetPatientsAsync_ReturnsAllPatients()
        {
            var patientList = patients;
            patientRepo.Setup(p => p.GetPatientsAsync()).ReturnsAsync(patients);

            var actual = await patientProvider.GetPatientsAsync();
            Assert.Same(patientList, actual);
            Assert.Equal(patientList, actual);
        }

        [Fact]
        public async void GetPatientsAsync_DatabaseErrorReturnsException()
        {
            var exception = new ServiceUnavailableException("There was a problem connecting to the database.");

            patientRepo.Setup(m => m.GetPatientsAsync()).ThrowsAsync(exception);
            try
            {
                await patientProvider.GetPatientsAsync();
            }
            catch (Exception ex)
            {
                Assert.Same(ex.GetType(), exception.GetType());
                Assert.Equal(ex.Message, exception.Message);
            }
        }

        [Fact]
        public async void GetPatientsByIdAsync_IdExistsReturnsPatient()
        {
            var patient = patients.FirstOrDefault(x => x.Id == 1);
            patientRepo.Setup(m => m.GetPatientByIdAsync(1)).ReturnsAsync(patient);

            var actual = await patientProvider.GetPatientByIdAsync(1);
            Assert.Same(patient, actual);
            Assert.Equal(1, actual.Id);
        }

        [Fact]
        public async void GetPatientByIdAsync_DatabaseErrorReturnsException()
        {
            var exception = new ServiceUnavailableException("There was a problem connecting to the database.");

            patientRepo.Setup(m => m.GetPatientByIdAsync(1)).ThrowsAsync(exception);
            try
            {
                await patientProvider.GetPatientByIdAsync(1);
            }
            catch (Exception ex)
            {
                Assert.Same(ex.GetType(), exception.GetType());
                Assert.Equal(ex.Message, exception.Message);
            }
        }

        [Fact]
        public async void GetPatientByIdAsync_IdIsNullReturnsNotFoundError()
        {
            Patient patient = null;
            var patientId = 2;
            var exception = new NotFoundException($"Patient with id: {patientId} could not be found.");

            patientRepo.Setup(m => m.GetPatientByIdAsync(patientId)).ReturnsAsync(patient);
            try
            {
                await patientProvider.GetPatientByIdAsync(patientId);
            }
            catch (Exception ex)
            {
                Assert.Same(ex.GetType(), exception.GetType());
                Assert.Equal(ex.Message, exception.Message);
            }
        }

        [Fact]
        public async void GetPatientByIdAsync_IdIsDefaultReturnsNotFoundError()
        {
            Patient patient = default;
            var patientId = 2;
            var exception = new NotFoundException($"Patient with id: {patientId} could not be found.");

            patientRepo.Setup(m => m.GetPatientByIdAsync(patientId)).ReturnsAsync(patient);
            try
            {
                await patientProvider.GetPatientByIdAsync(patientId);
            }
            catch (Exception ex)
            {
                Assert.Same(ex.GetType(), exception.GetType());
                Assert.Equal(ex.Message, exception.Message);
            }
        }

        [Fact]
        public async Task CreatePatientAsync_CreatesNewPatient_ReturnsSavedPatient()
        {
            var newPatient = new Patient
            {
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email3@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };

            patientRepo.Setup(m => m.CreatePatientAsync(newPatient)).ReturnsAsync(newPatient);
            var actual = await patientProvider.CreatePatientAsync(newPatient);
            var expected = newPatient;

            Assert.NotNull(actual);
            Assert.IsType<Patient>(actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task CreatePatientAsync_CreatesPatientWithExistingSku_ReturnsConflictException()
        {
            var newPatient = new Patient
            {
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };

            patientRepo.Setup(m => m.GetPatientByEmailAsync(newPatient.Email)).ReturnsAsync(newPatient);
            patientRepo.Setup(m => m.CreatePatientAsync(newPatient)).ThrowsAsync(new Exception());

            await Assert.ThrowsAsync<ConflictException>(() => patientProvider.CreatePatientAsync(newPatient));

        }

        [Fact]
        public async Task CreatePatientAsync_GetPatientByEmailAsyncThrowsDatabaseError_ReturnsServiceUnavailableException()
        {
            var newPatient = new Patient
            {
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email5@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };

            patientRepo.Setup(m => m.GetPatientByEmailAsync(newPatient.Email)).ThrowsAsync(new Exception());

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.CreatePatientAsync(newPatient));

        }

        [Fact]
        public async Task UpdatePatientAsync_GetPatientByEmailAsyncThrowsDatabaseError_ReturnsServiceUnavailableException()
        {
            var newPatient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };

            patientRepo.Setup(m => m.GetPatientByEmailAsync(newPatient.Email)).ThrowsAsync(new Exception());

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.UpdatePatientAsync(newPatient.Id, newPatient));

        }

        [Fact]
        public async Task UpdatePatientAsync_RequestDoesNotReachDatabase_ReturnsServiceUnavailableException1()
        {
            var newPatient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };
            patientRepo.Setup(m => m.GetPatientByIdAsync(newPatient.Id)).ReturnsAsync(newPatient);
            patientRepo.Setup(m => m.GetPatientByEmailAsync(newPatient.Email)).ReturnsAsync(newPatient);
            patientRepo.Setup(m => m.UpdatePatientAsync(newPatient)).ThrowsAsync(
                new Exception("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.UpdatePatientAsync(newPatient.Id, newPatient));
        }

        [Fact]
        public async Task CreatePatientAsync_RequestDoesNotReachDatabase_ReturnsServiceUnavailableException()
        {
            var newPatient = new Patient
            {
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };

            patientRepo.Setup(m => m.CreatePatientAsync(newPatient)).ThrowsAsync(
                new Exception("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.CreatePatientAsync(newPatient));
        }

        [Fact]
        public async Task DeletePatientByIdAsync_PatientDoesNotExist_ReturnsNotFoundException()
        {
            Patient patient = null;
            var patientId = 1;
            var exception = new NotFoundException($"Patient with ID {patientId} could not be found.");

            patientRepo.Setup(m => m.GetPatientByIdAsync(patientId)).ReturnsAsync(patient);
            patientRepo.Setup(m => m.DeletePatientByIdAsync(patient)).ThrowsAsync(
                new NotFoundException($"Patient with id: {patientId} not found."));

            await Assert.ThrowsAsync<NotFoundException>(() => patientProvider.DeletePatientByIdAsync(patientId));
        }

        [Fact]
        public async Task DeletePatientByIdAsync_GetPatientByIdReturnsDatabaseErrorReturnsException1()
        {
            var patient = patients.FirstOrDefault(m => m.Id == 1);
            patientRepo.Setup(m => m.GetPatientByIdAsync(patient.Id)).ThrowsAsync(
                new Exception("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.DeletePatientByIdAsync(patient.Id));
        }

        [Fact]
        public async Task UpdatePatientAsyncGetPatientByIdDoesNotReachDatabase_ReturnsServiceUnavailableExcpetion()
        {

            var patient = patients.FirstOrDefault(m => m.Id == 1);
            var newPatient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };

            patientRepo.Setup(m => m.GetPatientByIdAsync(newPatient.Id)).ThrowsAsync(
                new ServiceUnavailableException("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.GetPatientByIdAsync(newPatient.Id));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.UpdatePatientAsync(
                patient.Id, newPatient));
        }

        [Fact]
        public async Task UpdatePatientAsync_GetRequestDoesNotReachDatabase_ReturnsServiceUnavailableExcpetion()
        {
            var patient = patients.FirstOrDefault(r => r.Id == 1);
            var updatedPatient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };

            patientRepo.Setup(m => m.GetPatientByIdAsync(patient.Id)).ThrowsAsync(
                new Exception("There was a problem connecting to the database."));

            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.GetPatientByIdAsync(patient.Id));
            await Assert.ThrowsAsync<ServiceUnavailableException>(() => patientProvider.UpdatePatientAsync(
                patient.Id, updatedPatient));
        }

        [Fact]
        public async Task UpdatePatientAsync_PatientIdIsOneAndPatientIsDefault_ReturnsNotFoundException()
        {
            Patient patient = default;
            var patientId = 1;
            var updatedPatient = new Patient()
            {
                Id = patientId,
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };
            var exception = new NotFoundException($"Patient with id: {patientId} not found.");

            patientRepo.Setup(m => m.GetPatientByIdAsync(patientId)).ReturnsAsync(patient);
            patientRepo.Setup(m => m.UpdatePatientAsync(updatedPatient)).ThrowsAsync(
                new NotFoundException($"Patient with id: {patientId} not found."));

            await Assert.ThrowsAsync<NotFoundException>(() => patientProvider.UpdatePatientAsync(patientId, updatedPatient));
        }

        [Fact]
        public async Task UpdatePatientAsync_PatientIdIsOneAndPatientIsNull_ReturnsNotFoundException()
        {
            Patient patient = null;
            var patientId = 1;
            var updatedPatient = new Patient()
            {
                Id = patientId,
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Male",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };
            var exception = new NotFoundException($"Patient with id: {patientId} not found.");

            patientRepo.Setup(m => m.GetPatientByIdAsync(patientId)).ReturnsAsync(patient);
            patientRepo.Setup(m => m.UpdatePatientAsync(updatedPatient)).ThrowsAsync(
                new NotFoundException($"Patient with id: {patientId} not found."));

            await Assert.ThrowsAsync<NotFoundException>(() => patientProvider.UpdatePatientAsync(patientId, updatedPatient));
        }

        [Fact]
        public async Task UpdatePatientAsync_ChangePatientGender_ReturnsUpdatedPatient()
        {
            var patient = patients.FirstOrDefault(r => r.Id == 1);

            var updatedPatient = new Patient()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Ssn = "123-12-1234",
                Email = "email@mail.com",
                Age = 29,
                Height = 80,
                Weight = 150,
                Insurance = "Self-Insured",
                Gender = "Female",
                Street = "1 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "29445"
            };

            patientRepo.Setup(m => m.GetPatientByIdAsync(patient.Id)).ReturnsAsync(patient);
            patientRepo.Setup(m => m.UpdatePatientAsync(updatedPatient)).ReturnsAsync(updatedPatient);
            var expected = updatedPatient;
            var actual = await patientProvider.UpdatePatientAsync(patient.Id, updatedPatient);

            Assert.Same(expected, actual);
        }

    }
}

