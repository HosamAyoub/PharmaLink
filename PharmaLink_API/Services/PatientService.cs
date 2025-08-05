using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }
        Task<Patient> IPatientService.GetPatientByUserNameAsync(string accountId)
        {
            return _patientRepository.GetAsync(p => p.AccountId == accountId);
        }

        Task IPatientService.UpdatePatientAsync(Patient patient)
        {
            throw new NotImplementedException();
        }
    }
}
