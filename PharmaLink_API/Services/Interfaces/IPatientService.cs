using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PatientDTO;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IPatientService
    {
        //Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<PatientDTO> GetPatientByIdAsync(string accountId);
        //Task AddPatientAsync(Patient patient);
        Task UpdatePatientAsync(PatientDTO patientDTO, string accountId);
        Task DeletePatientAsync(string accountId);
        Task<PatientMedicalInfoDTO> GetPatientMedicalInfoByIdAsync(string accountId);
    }
}
