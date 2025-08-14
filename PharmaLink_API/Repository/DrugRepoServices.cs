using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using System.Linq.Expressions;

namespace PharmaLink_API.Repository
{
    /// <summary>
    /// Repository service class for Drug entity data access operations.
    /// Inherits from generic Repository&lt;Drug&gt; and implements IDrugRepository interface.
    /// Provides specialized methods for drug-related database operations using Entity Framework.
    /// </summary>
    public class DrugRepoServices : Repository<Drug>, IDrugRepository
    {

        public ApplicationDbContext Context { get; }
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the DrugRepoServices class.
        /// </summary>
        /// <param name="context">The Entity Framework database context for data operations.</param>
        public DrugRepoServices(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            Context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a randomized batch of drugs from the database with pagination support.
        /// This method adds a random offset to pagination to ensure varied results across requests.
        /// </summary>
        /// <param name="pageNumber">The page number for pagination (1-based indexing).</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of Drug entities (maximum 10 items per batch).
        /// </returns>
        /// <remarks>
        /// - Uses a fixed page size of 10 drugs per batch
        /// - Generates a random starting offset (up to 2000 records) to randomize results
        /// - May return fewer than 10 items if near the end of the dataset
        /// - Random seed is generated fresh for each method call
        /// </remarks>
        /// <example>
        /// <code>
        /// // Get first page of randomized drugs
        /// var firstBatch = await drugRepo.GetBatchDrugs(1);
        /// 
        /// // Get second page of randomized drugs  
        /// var secondBatch = await drugRepo.GetBatchDrugs(2);
        /// </code>
        /// </example>
        public async Task<List<Drug>> GetBatchDrugs(int pageNumber)
        {
            // Fixed page size for consistent batch sizes
            int pageSize = 10;

            // Generate random offset to ensure varied results across requests
            Random random = new Random();
            int RandomStart = random.Next(2000);

            // Take exactly pageSize number of drugs
            return await Context.Drugs.Where(D => D.DrugStatus == Status.Approved)
                .Skip(((pageNumber - 1) * pageSize) + RandomStart)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing drug record in the database with new information.
        /// Performs a complete update of all drug properties except the primary key.
        /// </summary>
        /// <param name="id">The unique identifier (DrugID) of the drug to be updated.</param>
        /// <param name="editedDrug">A Drug entity containing the new values to be applied.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        /// <remarks>
        /// - If the drug with the specified ID is not found, the method completes without action
        /// - All properties are updated regardless of whether they have changed
        /// - The operation is atomic - either all changes are saved or none are saved
        /// - The DrugID property is not updated as it serves as the primary key
        /// - Uses Entity Framework change tracking for efficient database updates
        /// </remarks>
        /// <example>
        /// <code>
        /// var updatedDrug = new Drug 
        /// {
        ///     CommonName = "Updated Drug Name",
        ///     ActiveIngredient = "New Active Ingredient",
        ///     // ... other properties
        /// };
        /// 
        /// await drugRepo.EditDrug(123, updatedDrug);
        /// </code>
        /// </example>
        public async Task EditDrug(Drug editedDrug)
        {
            Drug Updated = await Context.Drugs.FirstOrDefaultAsync(D => D.DrugID == editedDrug.DrugID);


            if (Updated != null)
            {
                _mapper.Map(editedDrug, Updated);
                await Context.SaveChangesAsync();
            }
        }
    }
}