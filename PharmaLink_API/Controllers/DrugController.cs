using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.DrugDto;
using PharmaLink_API.Models.DTO.DrugDTO;
using PharmaLink_API.Models.DTO.FavoriteDTO;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PharmaLink_API.Controllers
{
    /// <summary>
    /// API Controller for managing drug-related operations in the PharmaLink system.
    /// Provides endpoints for CRUD operations, searching, and retrieving drug information with pharmacy stock data.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DrugController : ControllerBase
    {
        private readonly IDrugRepository DrugRepo;
        private readonly IPharmacyStockRepository PharmaStockRepo;
        private readonly IPharmacyService _PharmaService;
        public readonly ApplicationDbContext Context;
        private readonly IMapper _mapper;
        private readonly ILogger<DrugController> _logger;

        public DrugController(IDrugRepository drugRepo, IPharmacyStockRepository pharmaStockRepo, IPharmacyService pharmaservice, ApplicationDbContext _context, IMapper mapper, ILogger<DrugController> logger)
        {
            this.DrugRepo = drugRepo ?? throw new ArgumentNullException(nameof(drugRepo));
            this.PharmaStockRepo = pharmaStockRepo ?? throw new ArgumentNullException(nameof(pharmaStockRepo));
            this._PharmaService = pharmaservice ?? throw new ArgumentNullException(nameof(pharmaservice));
            Context = _context ?? throw new ArgumentNullException(nameof(_context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves comprehensive drug information including pharmacy stock details and availability.
        /// </summary>
        /// <param name="DrugID">The unique identifier of the drug to retrieve</param>
        /// <returns>
        /// Returns a FullPharmaDrugDTO containing:
        /// - Complete drug information (name, category, active ingredient, etc.)
        /// - List of pharmacies that have the drug in stock with pricing and availability
        /// - Only includes pharmacies with available stock (status = Available)
        /// </returns>
        /// <response code="200">Successfully retrieved drug information with pharmacy stock details</response>
        /// <response code="400">Invalid DrugID provided (must be positive integer)</response>
        /// <response code="404">Drug with specified ID not found</response>
        /// <response code="500">Internal server error occurred while processing request</response>
        /// <example>
        /// GET /api/Drug?DrugID=123
        /// </example>
        [HttpGet]
        public async Task<ActionResult<FullPharmaDrugDTO>> GetDrugWithPharmacyStock([FromQuery][Required] int DrugID)
        {
            try
            {
                if (DrugID <= 0)
                {
                    return BadRequest("DrugID must be a positive integer.");
                }

                Drug Result = await DrugRepo.GetAsync(D => D.DrugID == DrugID, false, D => D.PharmacyStock);
                if (Result == null)
                {
                    return NotFound($"Drug with ID {DrugID} not found.");
                }

                var Pharmas = PharmaStockRepo.getPharmaciesThatHaveDrug(DrugID);
                var drugInfo = _mapper.Map<DrugDetailsDTO>(Result);

                if (Pharmas == null || !Pharmas.Any())
                {
                    return Ok(new FullPharmaDrugDTO
                    {
                        Drug_Info = drugInfo,
                        Pharma_Info = new List<PharmaDataDTO>()
                    });
                }

                var pharmaInfo = Result.PharmacyStock?.Where(Ph => Ph.Status == Product_Status.Available).Select(P => new PharmaDataDTO
                {
                    Pharma_Id = P.PharmacyId,
                    Pharma_Name =  Pharmas.FirstOrDefault(Ph => Ph.PharmacyID == P.PharmacyId)?.Name,
                    Pharma_Address = Pharmas.FirstOrDefault(Ph => Ph.PharmacyID == P.PharmacyId)?.Address,
                    Pharma_Location = "5 K.M",
                    Price = P.Price,
                    QuantityAvailable = P.QuantityAvailable
                }).ToList() ?? new List<PharmaDataDTO>();

                return Ok(new FullPharmaDrugDTO
                {
                    Drug_Info = drugInfo,
                    Pharma_Info = pharmaInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drug with pharmacy stock for DrugID: {DrugID}", DrugID);
                return StatusCode(500, "An error occurred while retrieving drug information.");
            }
        }

        /// <summary>
        /// Retrieves administrative dashboard data including drug statistics and pending requests.
        /// </summary>
        /// <returns>
        /// Returns administrative data containing:
        /// - Count of approved drugs in the system
        /// - Count of pending drug requests
        /// - Number of unique drug categories
        /// - List of pending drug requests with pharmacy information
        /// </returns>
        /// <response code="200">Successfully retrieved administrative data</response>
        /// <response code="404">No drugs found in the system</response>
        /// <response code="500">Internal server error occurred while processing request</response>
        /// <remarks>
        /// This endpoint is typically used by administrators to monitor:
        /// - System drug inventory statistics
        /// - Pending pharmacy requests for new drugs
        /// - Overall system health metrics
        /// </remarks>
        /// <example>
        /// GET /api/Drug/GetAdminData
        /// </example>
        [HttpGet("GetAdminData")]
        public async Task<IActionResult> GetAdminData()
        {
            try
            {
                List<Drug> result = await DrugRepo.GetAllAsync();
                if (result == null || !result.Any())
                {
                    return NotFound("No drugs found.");
                }

                int ApprovedDrugCount = result.Count(d => d.DrugStatus == Status.Approved);
                int RequestedDrugCount = result.Count(d => d.DrugStatus == Status.Pending);
                int Categories = result.Select(D => D.Category).Distinct().Count();
                List<Drug> DrugRequests = result.Where(d => d.DrugStatus == Status.Pending).ToList();
                var requests = new List<DrugRequestDTO>();

                foreach (var D in DrugRequests)
                {
                    var pharmacy = await _PharmaService.GetPharmacyByIdAsync(D.CreatedByPharmacy ?? 0);
                    requests.Add(new DrugRequestDTO
                    {
                        Pharmacy = new Pharmacy_Sender
                        {
                            Pharmacy_Id = D.CreatedByPharmacy ?? 0,
                            Pharmacy_Name = pharmacy?.Name ?? "Unknown Pharmacy"
                        },
                        NewDrug = _mapper.Map<DrugDetailsDTO>(D),
                        IsRead = D.IsRead ?? false,
                        CreatedAt = D.CreatedAt
                    });
                }

                return Ok(new
                {
                    Approved = ApprovedDrugCount,
                    Requests = RequestedDrugCount,
                    No_Categories = Categories,
                    DrugRequests = requests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin data");
                return StatusCode(500, "An error occurred while retrieving admin data.");
            }
        }

        /// <summary>
        /// Retrieves a paginated batch of drugs for browsing and discovery.
        /// </summary>
        /// <param name="PageIndex">The page number to retrieve (1-based indexing)</param>
        /// <returns>
        /// Returns a list of FavoriteDrugDTO containing:
        /// - Drug ID, name, description, and image URL
        /// - Drug category information
        /// - Limited to approved drugs only
        /// </returns>
        /// <response code="200">Successfully retrieved drug batch for the specified page</response>
        /// <response code="400">Invalid PageIndex provided (must be positive integer starting from 1)</response>
        /// <response code="500">Internal server error occurred while processing request</response>
        /// <remarks>
        /// This endpoint supports pagination for efficient loading of drug listings.
        /// Only approved drugs are returned to ensure quality control.
        /// Typically used for drug browsing interfaces and mobile app feeds.
        /// </remarks>
        /// <example>
        /// GET /api/Drug/1 (retrieves first page)
        /// GET /api/Drug/5 (retrieves fifth page)
        /// </example>
        [HttpGet("{PageIndex:int}")]
        public async Task<ActionResult<List<FavoriteDrugDTO>>> GetBatch( int PageIndex)
        {
            try
            {
                if (PageIndex <= 0)
                {
                    return BadRequest("PageIndex must be a positive integer starting from 1.");
                }

                var Batch = await DrugRepo.GetBatchDrugs(PageIndex);
                if (Batch == null)
                {
                    return Ok(new List<FavoriteDrugDTO>());
                }

                return Ok(Batch.Select(D => new FavoriteDrugDTO
                {
                    DrugId = D.DrugID,
                    Name = D.CommonName,
                    Description = D.Description,
                    ImageUrl = D.Drug_UrlImg,
                    DrugCategory = D.Category
                }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drug batch for PageIndex: {PageIndex}", PageIndex);
                return StatusCode(500, "An error occurred while retrieving drug batch.");
            }
        }

        /// <summary>
        /// Searches for drugs by their common name with partial matching support.
        /// </summary>
        /// <param name="Dname">The drug name or partial name to search for (1-100 characters)</param>
        /// <returns>
        /// Returns a list of DrugDetailsDTO containing complete drug information for matches.
        /// Only approved drugs are included in search results.
        /// </returns>
        /// <response code="200">Successfully retrieved drugs matching the search criteria</response>
        /// <response code="400">Invalid or empty drug name provided</response>
        /// <response code="500">Internal server error occurred while processing request</response>
        /// <remarks>
        /// Search is case-insensitive and uses prefix matching (starts with).
        /// Only returns drugs with approved status for patient safety.
        /// Ideal for autocomplete functionality and drug name lookups.
        /// </remarks>
        /// <example>
        /// GET /api/Drug/Drug_Name?Dname=aspirin
        /// GET /api/Drug/Drug_Name?Dname=para (finds paracetamol, etc.)
        /// </example>
        [HttpGet("Drug_Name")]
        public async Task<ActionResult<List<DrugDetailsDTO>>> GetByName([FromQuery][Required][StringLength(100, MinimumLength = 1)] string Dname)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Dname))
                {
                    return BadRequest("Drug name cannot be null or empty.");
                }

                var Result = await DrugRepo.GetAllAsync(D => D.CommonName.ToLower().StartsWith(Dname.ToLower()) && D.DrugStatus == Status.Approved);
                
                return Ok(Result?.Select(D => _mapper.Map<DrugDetailsDTO>(D)).ToList() ?? new List<DrugDetailsDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching drugs by name: {Dname}", Dname);
                return StatusCode(500, "An error occurred while searching for drugs.");
            }
        }

        /// <summary>
        /// Searches for drugs by their therapeutic category with partial matching support.
        /// </summary>
        /// <param name="Cname">The category name or partial name to search for (1-50 characters)</param>
        /// <returns>
        /// Returns a list of FavoriteDrugDTO containing essential drug information for category matches.
        /// Only approved drugs are included in search results.
        /// </returns>
        /// <response code="200">Successfully retrieved drugs in the specified category</response>
        /// <response code="400">Invalid or empty category name provided</response>
        /// <response code="500">Internal server error occurred while processing request</response>
        /// <remarks>
        /// Search is case-insensitive and uses prefix matching (starts with).
        /// Useful for browsing drugs by therapeutic area (e.g., "cardio", "anti", "pain").
        /// Includes patient favorites information for personalized recommendations.
        /// </remarks>
        /// <example>
        /// GET /api/Drug/Category?Cname=antibiotics
        /// GET /api/Drug/Category?Cname=pain (finds pain relievers)
        /// </example>
        [HttpGet("Category")]
        public async Task<ActionResult<List<FavoriteDrugDTO>>> GetByCategory([FromQuery][Required][StringLength(50, MinimumLength = 1)] string Cname)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Cname))
                {
                    return BadRequest("Category name cannot be null or empty.");
                }

                var Result = await DrugRepo.GetAllAsync(D => D.Category.ToLower().StartsWith(Cname.ToLower()) && D.DrugStatus == Status.Approved, F => F.PatientFavorites);
                
                return Ok(Result?.Select(D => new FavoriteDrugDTO
                {
                    DrugId = D.DrugID,
                    Name = D.CommonName,
                    Description = D.Description,
                    ImageUrl = D.Drug_UrlImg,
                    DrugCategory = D.Category
                }).ToList() ?? new List<FavoriteDrugDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching drugs by category: {Cname}", Cname);
                return StatusCode(500, "An error occurred while searching for drugs by category.");
            }
        }

        /// <summary>
        /// Searches for drugs by their active pharmaceutical ingredient with partial matching support.
        /// </summary>
        /// <param name="Active_Ingredient">The active ingredient name or partial name to search for (1-100 characters)</param>
        /// <returns>
        /// Returns a list of DrugDetailsDTO containing complete drug information for active ingredient matches.
        /// Only approved drugs are included in search results.
        /// </returns>
        /// <response code="200">Successfully retrieved drugs with the specified active ingredient</response>
        /// <response code="400">Invalid or empty active ingredient provided</response>
        /// <response code="500">Internal server error occurred while processing request</response>
        /// <remarks>
        /// Search is case-insensitive and uses prefix matching (starts with).
        /// Essential for finding drug alternatives and generic equivalents.
        /// Helps healthcare providers identify drugs with same active compounds.
        /// Useful for allergy checking and drug interaction analysis.
        /// </remarks>
        /// <example>
        /// GET /api/Drug/Active_Ingredient?Active_Ingredient=ibuprofen
        /// GET /api/Drug/Active_Ingredient?Active_Ingredient=acetamino (finds acetaminophen)
        /// </example>
        [HttpGet("Active_Ingredient")]
        public async Task<ActionResult<List<DrugDetailsDTO>>> GetByActiveIngredient([FromQuery][Required][StringLength(100, MinimumLength = 1)] string Active_Ingredient)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Active_Ingredient))
                {
                    return BadRequest("Active ingredient cannot be null or empty.");
                }

                var Result = await DrugRepo.GetAllAsync(D => D.ActiveIngredient.ToLower().StartsWith(Active_Ingredient.ToLower()) && D.DrugStatus == Status.Approved);
                
                return Ok(Result?.Select(D => _mapper.Map<DrugDetailsDTO>(D)).ToList() ?? new List<DrugDetailsDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching drugs by active ingredient: {Active_Ingredient}", Active_Ingredient);
                return StatusCode(500, "An error occurred while searching for drugs by active ingredient.");
            }
        }

        /// <summary>
        /// Performs a comprehensive search across multiple drug attributes including name, category, and active ingredient.
        /// </summary>
        /// <param name="SearchAnything">The search term to match against drug name, category, or active ingredient (1-100 characters)</param>
        /// <returns>
        /// Returns a unified list of DrugDetailsDTO containing all drugs that match the search term in any searchable field.
        /// Results are deduplicated and only include approved drugs.
        /// </returns>
        /// <response code="200">Successfully retrieved drugs matching the search term</response>
        /// <response code="400">Invalid or empty search term provided</response>
        /// <response code="500">Internal server error occurred while processing request</response>
        /// <remarks>
        /// This is the most flexible search endpoint, searching across:
        /// - Drug common names (e.g., "aspirin")
        /// - Drug categories (e.g., "pain reliever")
        /// - Active ingredients (e.g., "acetylsalicylic acid")
        /// 
        /// Search is case-insensitive with prefix matching.
        /// Results are automatically deduplicated to prevent duplicate entries.
        /// Ideal for general search functionality and "search anything" interfaces.
        /// </remarks>
        /// <example>
        /// GET /api/Drug/Search?SearchAnything=pain
        /// GET /api/Drug/Search?SearchAnything=aspirin
        /// </example>
        [HttpGet("Search")]
        public async Task<ActionResult<List<DrugDetailsDTO>>> Search([FromQuery][Required][StringLength(100, MinimumLength = 1)] string SearchAnything)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchAnything))
                {
                    return BadRequest("Search term cannot be null or empty.");
                }

                var Add = await DrugRepo.GetAllAsync(D => EF.Functions.Like(D.CommonName.ToLower(), $"{SearchAnything.ToLower()}%") && D.DrugStatus == Status.Approved);
                var SearchList = Add.UnionBy(await DrugRepo.GetAllAsync(D => EF.Functions.Like(D.Category.ToLower(), $"{SearchAnything.ToLower()}%") && D.DrugStatus == Status.Approved), u => u.DrugID)
                    .UnionBy(await DrugRepo.GetAllAsync(D => EF.Functions.Like(D.ActiveIngredient.ToLower(), $"{SearchAnything.ToLower()}%") && D.DrugStatus == Status.Approved), u => u.DrugID);
                
                return Ok(SearchList?.Select(D => _mapper.Map<DrugDetailsDTO>(D)).ToList() ?? new List<DrugDetailsDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search with term: {SearchAnything}", SearchAnything);
                return StatusCode(500, "An error occurred while performing the search.");
            }
        }

        /// <summary>
        /// Creates a new drug entry in the system with comprehensive validation.
        /// </summary>
        /// <param name="NewDrugDTO">The drug data transfer object containing all drug information</param>
        /// <returns>
        /// Returns a success message upon successful drug creation.
        /// The new drug will have a status of "Requested" pending administrative approval.
        /// </returns>
        /// <response code="200">Successfully created new drug entry</response>
        /// <response code="400">Invalid drug data provided or validation failed</response>
        /// <response code="500">Internal server error occurred while creating drug</response>
        /// <remarks>
        /// This endpoint allows pharmacies to request new drugs to be added to the system.
        /// All new drugs start with "Requested" status and require admin approval.
        /// 
        /// Required fields include:
        /// - CommonName: The drug's common/brand name
        /// - Category: Therapeutic category
        /// - ActiveIngredient: Primary active pharmaceutical ingredient
        /// 
        /// Optional fields include detailed drug information like dosage, contraindications, etc.
        /// </remarks>
        /// <example>
        /// POST /api/Drug
        /// Content-Type: application/json
        /// {
        ///   "CommonName": "Example Drug",
        ///   "Category": "Pain Relief",
        ///   "ActiveIngredient": "Example Compound"
        /// }
        /// </example>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody][Required] DrugDetailsDTO NewDrugDTO)
        {
            try
            {
                if (NewDrugDTO == null)
                {
                    return BadRequest("Drug data cannot be null.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Additional validation
                if (string.IsNullOrWhiteSpace(NewDrugDTO.CommonName))
                {
                    return BadRequest("Drug common name is required.");
                }

                await DrugRepo.CreateAndSaveAsync(_mapper.Map<Drug>(NewDrugDTO));
                return Ok(new { Message = "Drug created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new drug");
                return StatusCode(500, "An error occurred while creating the drug.");
            }
        }

        /// <summary>
        /// Updates an existing drug's information with comprehensive validation and existence checking.
        /// </summary>
        /// <param name="EditedDrugDTO">The drug data transfer object containing updated drug information including DrugID</param>
        /// <returns>
        /// Returns a success message upon successful drug update.
        /// All drug fields can be updated except the DrugID which is used for identification.
        /// </returns>
        /// <response code="200">Successfully updated drug information</response>
        /// <response code="400">Invalid drug data provided, missing DrugID, or validation failed</response>
        /// <response code="404">Drug with specified ID not found</response>
        /// <response code="500">Internal server error occurred while updating drug</response>
        /// <remarks>
        /// This endpoint allows modification of existing drug information.
        /// The DrugID field is required and must match an existing drug in the system.
        /// 
        /// All other fields can be updated including:
        /// - Drug details (name, category, active ingredient)
        /// - Clinical information (dosage, contraindications, warnings)
        /// - Administrative data (status, descriptions)
        /// 
        /// Changes are immediately applied and reflected in search results.
        /// </remarks>
        /// <example>
        /// PUT /api/Drug
        /// Content-Type: application/json
        /// {
        ///   "DrugID": 123,
        ///   "CommonName": "Updated Drug Name",
        ///   "Category": "Updated Category"
        /// }
        /// </example>
        [HttpPut]
        public async Task<IActionResult> Put([FromBody][Required] DrugDetailsDTO EditedDrugDTO)
        {
            try
            {
                if (EditedDrugDTO == null)
                {
                    return BadRequest("Drug data cannot be null.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (EditedDrugDTO.DrugID <= 0)
                {
                    return BadRequest("Valid DrugID is required for update.");
                }

                // Check if drug exists
                var existingDrug = await DrugRepo.GetAsync(D => D.DrugID == EditedDrugDTO.DrugID);
                if (existingDrug == null)
                {
                    return NotFound($"Drug with ID {EditedDrugDTO.DrugID} not found.");
                }

                await DrugRepo.EditDrug(_mapper.Map<Drug>(EditedDrugDTO));
                return Ok(new { Message = "Drug updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drug with ID: {DrugID}", EditedDrugDTO?.DrugID);
                return StatusCode(500, "An error occurred while updating the drug.");
            }
        }

        /// <summary>
        /// Permanently removes a drug from the system with safety validations.
        /// </summary>
        /// <param name="id">The unique identifier of the drug to delete (must be positive integer)</param>
        /// <returns>
        /// Returns a success message upon successful drug deletion.
        /// The drug and all its associations will be permanently removed.
        /// </returns>
        /// <response code="200">Successfully deleted drug from system</response>
        /// <response code="400">Invalid DrugID provided (must be positive integer)</response>
        /// <response code="404">Drug with specified ID not found</response>
        /// <response code="500">Internal server error occurred while deleting drug</response>
        /// <remarks>
        /// This endpoint permanently removes a drug from the system.
        /// 
        /// ⚠️ **Warning: This operation is irreversible!**
        /// 
        /// Before deletion, consider:
        /// - Existing pharmacy stock will need to be handled separately
        /// - Patient favorites referencing this drug may be affected
        /// - Order history containing this drug will lose drug details
        /// 
        /// Use with extreme caution, especially for drugs with existing stock or order history.
        /// Consider marking drugs as "Discontinued" instead of deletion for data integrity.
        /// </remarks>
        /// <example>
        /// DELETE /api/Drug/123
        /// </example>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("DrugID must be a positive integer.");
                }

                Drug DeletedDrug = await DrugRepo.GetAsync(D => D.DrugID == id);
                if (DeletedDrug == null)
                {
                    return NotFound($"Drug with ID {id} not found.");
                }

                await DrugRepo.RemoveAsync(DeletedDrug);
                return Ok(new { Message = "Drug deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting drug with ID: {id}", id);
                return StatusCode(500, "An error occurred while deleting the drug.");
            }
        }
    }
}