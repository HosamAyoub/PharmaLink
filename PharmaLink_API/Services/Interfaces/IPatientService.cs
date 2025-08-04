using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;

namespace PharmaLink_API.Services.Interfaces
{
    public interface IPatientService
    {
        //Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient> GetPatientByUserNameAsync(string accountId);
        //Task AddPatientAsync(Patient patient);
        Task UpdatePatientAsync(Patient patient);
        //Task DeletePatientAsync(int id);
    }
}
