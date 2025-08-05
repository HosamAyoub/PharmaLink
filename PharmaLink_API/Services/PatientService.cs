using AutoMapper;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PatientDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;

        public PatientService(IPatientRepository patientRepository, IMapper mapper)
        {
            _patientRepository = patientRepository;
            _mapper = mapper;
        }
        public async Task<PatientDTO> GetPatientByIdAsync(string accountId)
        {
            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            var patientDTO = _mapper.Map<PatientDTO>(patient);
            return patientDTO;
        }

        public async Task UpdatePatientAsync(PatientDTO patientDTO, string accountId)
        {
            var selectedPatient = await _patientRepository.GetAsync(p => p.AccountId == accountId, tracking: true);
            if (selectedPatient == null)
                throw new Exception("Patient not found.");

            // Map updated fields from DTO to the existing entity
            _mapper.Map(patientDTO, selectedPatient);

            await _patientRepository.UpdateAsync(selectedPatient);
        }
    }
}
