using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.DrugDto;
using PharmaLink_API.Models.DTO.DrugDTO;
using PharmaLink_API.Models.DTO.FavoriteDTO;
using PharmaLink_API.Repository.Interfaces;
using System.Collections;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PharmaLink_API.Controllers
{
    /// <summary>
    /// API Controller for managing drug-related operations in the PharmaLink system.
    /// Provides endpoints for CRUD operations, searching, and retrieving drug information with pharmacy stock data.
    /// </summary>
    /// <remarks>
    /// This controller handles various drug operations including:
    /// - Retrieving drug details with pharmacy stock information
    /// - Batch retrieval of drugs with pagination
    /// - Searching drugs by name, category, or active ingredient
    /// - Creating, updating, and deleting drug records
    /// </remarks>
    //[Authorize(Roles = "Admin,Pharmacist,Doctor,Patient")]
    [Route("api/[controller]")]
    [ApiController]
    public class DrugController : ControllerBase
    {
        /// <summary>
        /// Repository interface for drug-related data operations.
        /// </summary>
        private readonly IDrugRepository DrugRepo;

        /// <summary>
        /// Repository interface for pharmacy stock-related data operations.
        /// </summary>
        private readonly IPharmacyStockRepository PharmaStockRepo;

        /// <summary>
        /// Entity Framework database context for direct database access.
        /// </summary>
        public readonly ApplicationDbContext Context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the DrugController class with dependency injection.
        /// </summary>
        /// <param name="drugRepo">The drug repository for data access operations.</param>
        /// <param name="pharmaStockRepo">The pharmacy stock repository for stock-related operations.</param>
        /// <param name="_context">The Entity Framework database context.</param>
        public DrugController(IDrugRepository drugRepo, IPharmacyStockRepository pharmaStockRepo, ApplicationDbContext _context , IMapper mapper)
        {
            this.DrugRepo = drugRepo;
            this.PharmaStockRepo = pharmaStockRepo;
            Context = _context;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves comprehensive drug information including pharmacy stock details.
        /// </summary>
        /// <param name="DrugID">The unique identifier of the drug to retrieve.</param>
        /// <returns>
        /// A FullPharmaDrugDTO containing complete drug information and a list of pharmacies 
        /// that have the drug in stock with their pricing and availability details.
        /// </returns>
        /// <response code="200">Returns the drug information with pharmacy stock details.</response>
        /// <response code="404">If the drug with the specified ID is not found.</response>
        [HttpGet]
        public async Task<FullPharmaDrugDTO> GetDrugWithPharmacyStock(int DrugID)
        {
            Drug Result = await DrugRepo.GetAsync(D => D.DrugID == DrugID, false, D => D.PharmacyStock);
            if (Result == null)
            {
                return new FullPharmaDrugDTO
                {
                    Drug_Info = null,
                    Pharma_Info = new List<PharmaDataDTO>()
                };
            }

            var Pharmas = PharmaStockRepo.getPharmaciesThatHaveDrug(DrugID);
            if (Pharmas == null || !Pharmas.Any())
            {
                return new FullPharmaDrugDTO
                {
                    Drug_Info = _mapper.Map<DrugDetailsDTO>(Result),
                    Pharma_Info = new List<PharmaDataDTO>()
                };
            }


            return new FullPharmaDrugDTO
            {
                Drug_Info = _mapper.Map<DrugDetailsDTO>(Result),
                Pharma_Info = Result.PharmacyStock.Select(P => new PharmaDataDTO
                {
                    Pharma_Id = P.PharmacyId,
                    Pharma_Name = Pharmas.FirstOrDefault(Ph => Ph.PharmacyID == P.PharmacyId)?.Name,
                    Pharma_Address = Pharmas.FirstOrDefault(Ph => Ph.PharmacyID == P.PharmacyId)?.Address,
                    Pharma_Location = "5 K.M",
                    Price = P.Price,
                    QuantityAvailable = P.QuantityAvailable
                }).ToList()
            };
        }

        /// <summary>
        /// Retrieves a batch of drugs with pagination support using randomized results.
        /// </summary>
        /// <param name="PageIndex">The page number for pagination (1-based indexing).</param>
        /// <returns>A list of DrugDetailsDTO objects containing drug information (typically 10 items per page).</returns>
        /// <response code="200">Returns a list of drugs for the specified page.</response>
        /// <remarks>
        /// This endpoint uses randomized pagination to provide varied results across requests.
        /// Each page contains up to 10 drug records.
        /// </remarks>
        [HttpGet("{PageIndex:int}")]
        public async Task<List<FavoriteDrugDTO>> GetBatch(int PageIndex)
        {
            var Batch = await DrugRepo.GetBatchDrugs(PageIndex);
            return Batch.Select(D => new FavoriteDrugDTO
            {
                DrugId = D.DrugID,
                Name = D.CommonName,
                Description = D.Description,
                ImageUrl = D.Drug_UrlImg,
                DrugCategory = D.Category
            }).ToList();
        }

        /// <summary>
        /// Searches for drugs by their common name using partial matching.
        /// </summary>
        /// <param name="Dname">The drug name or partial name to search for (case-insensitive).</param>
        /// <returns>A list of DrugDetailsDTO objects for drugs whose names start with the specified string.</returns>
        /// <response code="200">Returns a list of matching drugs.</response>
        /// <remarks>
        /// The search is case-insensitive and matches drugs whose common names start with the provided string.
        /// </remarks>
        /// <example>
        /// GET /api/Drug/Drug_Name?Dname=para
        /// Returns drugs like "Paracetamol", "Paracetamol Extra", etc.
        /// </example>
        //[Authorize(Roles = "Admin")]
        [HttpGet("Drug_Name")]
        public async Task<List<DrugDetailsDTO>> GetByName(string Dname)
        {
            var Result = await DrugRepo.GetAllAsync(D => D.CommonName.ToLower().StartsWith(Dname.ToLower()) && D.DrugStatus == Status.Approved );
            return Result.Select(D => _mapper.Map<DrugDetailsDTO>(D)).ToList();
        }

        /// <summary>
        /// Searches for drugs by their category using partial matching.
        /// </summary>
        /// <param name="Cname">The category name or partial category name to search for (case-insensitive).</param>
        /// <returns>A list of DrugDetailsDTO objects for drugs whose categories start with the specified string.</returns>
        /// <response code="200">Returns a list of drugs in the matching category.</response>
        /// <remarks>
        /// The search is case-insensitive and matches drugs whose categories start with the provided string.
        /// </remarks>
        /// <example>
        /// GET /api/Drug/Category?Cname=anti
        /// Returns drugs in categories like "Antibiotic", "Antiviral", etc.
        /// </example>
        //[Authorize(Roles = "User, Admin, Pharmacy")]
        [HttpGet("Category")]
        public async Task<List<FavoriteDrugDTO>> GetByCategory(string Cname)
        {
            var Result = await DrugRepo.GetAllAsync(D => D.Category.ToLower().StartsWith(Cname.ToLower()) && D.DrugStatus == Status.Approved, F => F.PatientFavorites);
            return Result.Select(D => new FavoriteDrugDTO
            {
                DrugId = D.DrugID,
                Name = D.CommonName,
                Description = D.Description,
                ImageUrl = D.Drug_UrlImg,
                DrugCategory = D.Category
            }).ToList();
        }

        /// <summary>
        /// Searches for drugs by their active ingredient using partial matching.
        /// </summary>
        /// <param name="Active_Ingredient">The active ingredient name or partial name to search for (case-insensitive).</param>
        /// <returns>A list of DrugDetailsDTO objects for drugs containing the specified active ingredient.</returns>
        /// <response code="200">Returns a list of drugs with the matching active ingredient.</response>
        /// <remarks>
        /// The search is case-insensitive and currently searches in the Alternatives_names field.
        /// Note: There appears to be a potential bug as this searches Alternatives_names instead of ActiveIngredient.
        /// </remarks>
        [HttpGet("Active_Ingredient")]
        public async Task<List<DrugDetailsDTO>> GetByActiveIngredient(string Active_Ingredient)
        {
            var Result = await DrugRepo.GetAllAsync(D => D.Alternatives_names.ToLower().StartsWith(Active_Ingredient.ToLower()) && D.DrugStatus == Status.Approved);
            return Result.Select(D => _mapper.Map<DrugDetailsDTO>(D)).ToList();
        }

        /// <summary>
        /// Performs a comprehensive search across multiple drug fields (name, category, and active ingredient).
        /// </summary>
        /// <param name="SearchAnything">The search term to look for across drug name, category, and active ingredient fields.</param>
        /// <returns>A consolidated list of DrugDetailsDTO objects matching the search term in any of the searched fields.</returns>
        /// <response code="200">Returns a combined list of drugs matching the search criteria.</response>
        /// <remarks>
        /// This endpoint searches across three fields: CommonName, Category, and ActiveIngredient.
        /// Results from all three searches are combined into a single list.
        /// The search is case-insensitive and uses partial matching (starts with).
        /// May contain duplicate entries if a drug matches multiple search criteria.
        /// </remarks>
        /// <example>
        /// GET /api/Drug/q=aspirin
        /// Returns drugs where name, category, or active ingredient starts with "aspirin".
        /// </example>
        /// 

//        ////SearchList = Add.UnionBy(
//        await DrugRepo.GetAllAsync(D => EF.Functions.Like(D.CommonName, $"{SearchAnything}%")), 
//    u => u.DrugID
//).ToList();
        [HttpGet("Search")]
        public async Task<List<DrugDetailsDTO>> Search( [FromQuery]string SearchAnything)
        {
            var Add = await DrugRepo.GetAllAsync(D => EF.Functions.Like(D.CommonName.ToLower(), $"{SearchAnything.ToLower()}%") && D.DrugStatus == Status.Approved);
            var SearchList = Add.UnionBy(await DrugRepo.GetAllAsync(D => EF.Functions.Like(D.Category.ToLower(), $"{SearchAnything.ToLower()}%") && D.DrugStatus == Status.Approved), u => u.DrugID)
                .UnionBy(await DrugRepo.GetAllAsync(D => EF.Functions.Like(D.ActiveIngredient.ToLower(), $"{SearchAnything.ToLower()}%") && D.DrugStatus == Status.Approved), u => u.DrugID);
            return SearchList.Select(D => _mapper.Map<DrugDetailsDTO>(D)).ToList(); 
        }

        /// <summary>
        /// Creates a new drug record in the database.
        /// </summary>
        /// <param name="NewDrugDTO">The drug information to create as a DrugDetailsDTO object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <response code="200">Drug created successfully.</response>
        /// <response code="400">Invalid drug data provided.</response>
        /// <remarks>
        /// All drug properties except DrugID should be provided in the request body.
        /// The DrugID will be automatically generated by the database.
        /// </remarks>
        [HttpPost]
        public async Task Post([FromBody] DrugDetailsDTO NewDrugDTO)
        {
            await DrugRepo.CreateAndSaveAsync(_mapper.Map<Drug>(NewDrugDTO));
        }

        /// <summary>
        /// Updates an existing drug record with new information.
        /// </summary>
        /// <param name="id">The unique identifier of the drug to update.</param>
        /// <param name="EditedDrugDTO">The updated drug information as a DrugDetailsDTO object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <response code="200">Drug updated successfully.</response>
        /// <response code="404">Drug with the specified ID not found.</response>
        /// <response code="400">Invalid drug data provided.</response>
        /// <remarks>
        /// All drug properties can be updated except the DrugID.
        /// If the drug with the specified ID doesn't exist, the operation will complete without error.
        /// </remarks>
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] DrugDetailsDTO EditedDrugDTO)
        {
            await DrugRepo.EditDrug(id, _mapper.Map<Drug>(EditedDrugDTO));
        }

        /// <summary>
        /// Deletes a drug record from the database.
        /// </summary>
        /// <param name="id">The unique identifier of the drug to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <response code="200">Drug deleted successfully.</response>
        /// <response code="404">Drug with the specified ID not found.</response>
        /// <remarks>
        /// This operation permanently removes the drug record from the database.
        /// If the drug with the specified ID doesn't exist, this may result in an error.
        /// </remarks>
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            Drug DeletedDrug = await DrugRepo.GetAsync(D => D.DrugID == id);
            await DrugRepo.RemoveAsync(DeletedDrug);
        }
    }
}