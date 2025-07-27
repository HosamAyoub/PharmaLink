using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.DrugDto;
using System.Threading.Tasks;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Models.DTO.DrugDTO;
using System.Collections;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PharmaLink_API.Controllers
{
    //[Authorize(Roles = "Admin,Pharmacist,Doctor,Patient")]
    [Route("api/[controller]")]
    [ApiController]
    public class DrugController : ControllerBase
    {
        private readonly IDrugRepository DrugRepo;
        private readonly IPharmacyStockRepository PharmaStockRepo;
        public readonly ApplicationDbContext Context;

        public DrugController(IDrugRepository drugRepo, IPharmacyStockRepository pharmaStockRepo, ApplicationDbContext _context)
        {
            this.DrugRepo = drugRepo;
            this.PharmaStockRepo = pharmaStockRepo;
            Context = _context;
        }


        [HttpGet]

        public async Task<FullPharmaDrugDTO> GetDrugWithPharmacyStock (int DrugID)
        {
            Drug Result = await DrugRepo.GetAsync(D => D.DrugID == DrugID,false,D=>D.PharmacyStock);
            var Pharmas  = PharmaStockRepo.getPharmaciesThatHaveDrug(DrugID);
            return new FullPharmaDrugDTO
            {
                Drug_Info = new DrugDetailsDTO
                {
                    DrugID = Result.DrugID,
                    CommonName = Result.CommonName,
                    Category = Result.Category,
                    ActiveIngredient = Result.ActiveIngredient,
                    Alternatives_names = Result.Alternatives_names,
                    AlternativesGpID = Result.AlternativesGpID,
                    Indications_and_usage = Result.Indications_and_usage,
                    Dosage_and_administration = Result.Dosage_and_administration,
                    Dosage_forms_and_strengths = Result.Dosage_forms_and_strengths,
                    Contraindications = Result.Contraindications,
                    Warnings_and_cautions = Result.Warnings_and_cautions,
                    Drug_interactions = Result.Drug_interactions,
                    Description = Result.Description,
                    Storage_and_handling = Result.Storage_and_handling,
                    Adverse_reactions = Result.Adverse_reactions,
                    Drug_UrlImg = Result.Drug_UrlImg
                },
                Pharma_Info = Result.PharmacyStock.Select(P => new PharmaDataDTO
                {
                    Pharma_Id = P.PharmacyId,
                    Pharma_Name = Pharmas.FirstOrDefault(Ph => Ph.PharmacyID == P.PharmacyId).Name,
                    Pharma_Location = Pharmas.FirstOrDefault(Ph => Ph.PharmacyID == P.PharmacyId).Address,
                    Price = P.Price,
                    QuantityAvailable = P.QuantityAvailable
                }).ToList()
            };


        }



        [HttpGet("{PageIndex:int}")]

        public async Task<List<DrugDetailsDTO>> GetBatch(int PageIndex)
        {
            var Batch =  await DrugRepo.GetBatchDrugs(PageIndex);
            return Batch.Select(D => new DrugDetailsDTO
            {
                DrugID = D.DrugID,
                CommonName = D.CommonName,
                Category = D.Category,
                ActiveIngredient = D.ActiveIngredient,
                Alternatives_names = D.Alternatives_names,
                AlternativesGpID = D.AlternativesGpID,
                Indications_and_usage = D.Indications_and_usage,
                Dosage_and_administration = D.Dosage_and_administration,
                Dosage_forms_and_strengths = D.Dosage_forms_and_strengths,
                Contraindications = D.Contraindications,
                Warnings_and_cautions = D.Warnings_and_cautions,
                Drug_interactions = D.Drug_interactions,
                Description = D.Description,
                Storage_and_handling = D.Storage_and_handling,
                Adverse_reactions = D.Adverse_reactions,
                Drug_UrlImg = D.Drug_UrlImg
            }).ToList();
        }

        // GET api/<DrugController>/paracetamol
        //[Authorize(Roles = "Admin")]
        [HttpGet("Drug_Name")]
        public async Task<List<DrugDetailsDTO>> GetByName(string Dname)
        {
            var Result = await DrugRepo.GetAllAsync(D => D.CommonName.ToLower().StartsWith(Dname.ToLower()));
            return Result.Select(D => new DrugDetailsDTO
            {
                DrugID = D.DrugID,
                CommonName = D.CommonName,
                Category = D.Category,
                ActiveIngredient = D.ActiveIngredient,
                Alternatives_names = D.Alternatives_names,
                AlternativesGpID = D.AlternativesGpID,
                Indications_and_usage = D.Indications_and_usage,
                Dosage_and_administration = D.Dosage_and_administration,
                Dosage_forms_and_strengths = D.Dosage_forms_and_strengths,
                Contraindications = D.Contraindications,
                Warnings_and_cautions = D.Warnings_and_cautions,
                Drug_interactions = D.Drug_interactions,
                Description = D.Description,
                Storage_and_handling = D.Storage_and_handling,
                Adverse_reactions = D.Adverse_reactions,
                Drug_UrlImg = D.Drug_UrlImg
            }).ToList();
        }

        //[Authorize(Roles = "User, Admin, Pharmacy")]
        [HttpGet("Category")]
        public async Task<List<DrugDetailsDTO>> GetByCategory(string Cname)
        {
            var Result =  await DrugRepo.GetAllAsync(D => D.Category.ToLower().StartsWith(Cname.ToLower()));
            return Result.Select(D => new DrugDetailsDTO
            {
                DrugID = D.DrugID,
                CommonName = D.CommonName,
                Category = D.Category,
                ActiveIngredient = D.ActiveIngredient,
                Alternatives_names = D.Alternatives_names,
                AlternativesGpID = D.AlternativesGpID,
                Indications_and_usage = D.Indications_and_usage,
                Dosage_and_administration = D.Dosage_and_administration,
                Dosage_forms_and_strengths = D.Dosage_forms_and_strengths,
                Contraindications = D.Contraindications,
                Warnings_and_cautions = D.Warnings_and_cautions,
                Drug_interactions = D.Drug_interactions,
                Description = D.Description,
                Storage_and_handling = D.Storage_and_handling,
                Adverse_reactions = D.Adverse_reactions,
                Drug_UrlImg = D.Drug_UrlImg
            }).ToList();
        }

        [HttpGet("Active_Ingredient")]
        public async Task<List<DrugDetailsDTO>> GetByActiveIngredient(string Active_Ingredient)
        {
            var Result = await DrugRepo.GetAllAsync(D => D.Alternatives_names.ToLower().StartsWith(Active_Ingredient.ToLower()));
            return Result.Select(D => new DrugDetailsDTO
            {
                DrugID = D.DrugID,
                CommonName = D.CommonName,
                Category = D.Category,
                ActiveIngredient = D.ActiveIngredient,
                Alternatives_names = D.Alternatives_names,
                AlternativesGpID = D.AlternativesGpID,
                Indications_and_usage = D.Indications_and_usage,
                Dosage_and_administration = D.Dosage_and_administration,
                Dosage_forms_and_strengths = D.Dosage_forms_and_strengths,
                Contraindications = D.Contraindications,
                Warnings_and_cautions = D.Warnings_and_cautions,
                Drug_interactions = D.Drug_interactions,
                Description = D.Description,
                Storage_and_handling = D.Storage_and_handling,
                Adverse_reactions = D.Adverse_reactions,
                Drug_UrlImg = D.Drug_UrlImg
            }).ToList();
        }

        [HttpGet("q={SearchAnything}")]
        public async Task<List<DrugDetailsDTO>> Search(string SearchAnything)
        { 
            List<Drug> SearchList = new List<Drug>();
            SearchList.AddRange(await DrugRepo.GetAllAsync(D => D.CommonName.ToLower().StartsWith(SearchAnything.ToLower())));
            SearchList.AddRange(await DrugRepo.GetAllAsync(D => D.Category.ToLower().StartsWith(SearchAnything.ToLower())));
            SearchList.AddRange(await DrugRepo.GetAllAsync(D => D.ActiveIngredient.ToLower().StartsWith(SearchAnything.ToLower())));
            return SearchList.Select(D => new DrugDetailsDTO
            {
                DrugID = D.DrugID,
                CommonName = D.CommonName,
                Category = D.Category,
                ActiveIngredient = D.ActiveIngredient,
                Alternatives_names = D.Alternatives_names,
                AlternativesGpID = D.AlternativesGpID,
                Indications_and_usage = D.Indications_and_usage,
                Dosage_and_administration = D.Dosage_and_administration,
                Dosage_forms_and_strengths = D.Dosage_forms_and_strengths,
                Contraindications = D.Contraindications,
                Warnings_and_cautions = D.Warnings_and_cautions,
                Drug_interactions = D.Drug_interactions,
                Description = D.Description,
                Storage_and_handling = D.Storage_and_handling,
                Adverse_reactions = D.Adverse_reactions,
                Drug_UrlImg = D.Drug_UrlImg
            }).ToList();
        }


        // POST api/<DrugController>
        [HttpPost]
        public async Task Post([FromBody] DrugDetailsDTO NewDrugDTO)
        {
            Drug NewDrug = new Drug
            {
                CommonName = NewDrugDTO.CommonName,
                Category = NewDrugDTO.Category,
                ActiveIngredient = NewDrugDTO.ActiveIngredient,
                Alternatives_names = NewDrugDTO.Alternatives_names,
                AlternativesGpID = NewDrugDTO.AlternativesGpID,
                Indications_and_usage = NewDrugDTO.Indications_and_usage,
                Dosage_and_administration = NewDrugDTO.Dosage_and_administration,
                Dosage_forms_and_strengths = NewDrugDTO.Dosage_forms_and_strengths,
                Contraindications = NewDrugDTO.Contraindications,
                Warnings_and_cautions = NewDrugDTO.Warnings_and_cautions,
                Drug_interactions = NewDrugDTO.Drug_interactions,
                Description = NewDrugDTO.Description,
                Storage_and_handling = NewDrugDTO.Storage_and_handling,
                Adverse_reactions = NewDrugDTO.Adverse_reactions,
                Drug_UrlImg = NewDrugDTO.Drug_UrlImg
            };
            await DrugRepo.CreateAsync(NewDrug);
        }

        // PUT api/<DrugController>/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] DrugDetailsDTO EditedDrugDTO)
        {
            Drug EditedDrug = new Drug
            {
                CommonName = EditedDrugDTO.CommonName,
                Category = EditedDrugDTO.Category,
                ActiveIngredient = EditedDrugDTO.ActiveIngredient,
                Alternatives_names = EditedDrugDTO.Alternatives_names,
                AlternativesGpID = EditedDrugDTO.AlternativesGpID,
                Indications_and_usage = EditedDrugDTO.Indications_and_usage,
                Dosage_and_administration = EditedDrugDTO.Dosage_and_administration,
                Dosage_forms_and_strengths = EditedDrugDTO.Dosage_forms_and_strengths,
                Contraindications = EditedDrugDTO.Contraindications,
                Warnings_and_cautions = EditedDrugDTO.Warnings_and_cautions,
                Drug_interactions = EditedDrugDTO.Drug_interactions,
                Description = EditedDrugDTO.Description,
                Storage_and_handling = EditedDrugDTO.Storage_and_handling,
                Adverse_reactions = EditedDrugDTO.Adverse_reactions,
                Drug_UrlImg = EditedDrugDTO.Drug_UrlImg
            };
            await DrugRepo.EditDrug(id, EditedDrug);
        }

        // DELETE api/<DrugController>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            Drug DeletedDrug = await DrugRepo.GetAsync(D => D.DrugID == id);
            await DrugRepo.RemoveAsync(DeletedDrug);
        }
    }
}
