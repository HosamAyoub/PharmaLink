using Microsoft.EntityFrameworkCore;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using System.Linq.Expressions;

namespace PharmaLink_API.Repository
{
    public class DrugRepoServices : Repository<Drug> , IDrugRepository 
    {
       public ApplicationDbContext Context { get; }

        public DrugRepoServices (ApplicationDbContext context) : base (context)
        {
            Context = context;
        }

        // Return a random collection of drugs 
        public async Task<List<Drug>> GetBatchDrugs(int pageNumber)
        {
            int pageSize = 10;
            Random random = new Random();
            int RandomStart = random.Next(2000);
            return await Context.Drugs
                .Skip(((pageNumber - 1) * pageSize)+RandomStart)
                .Take(pageSize)
                .ToListAsync();
        }




        public async Task EditDrug(int id, Drug editedDrug)
        {
            Drug Updated = await Context.Drugs.FirstOrDefaultAsync(D => D.DrugID == id);
            if (Updated != null)
            {
                Updated.CommonName = editedDrug.CommonName;
                Updated.ActiveIngredient = editedDrug.ActiveIngredient;
                Updated.Category = editedDrug.Category;
                Updated.Indications_and_usage = editedDrug.Indications_and_usage;
                Updated.Drug_interactions = editedDrug.Drug_interactions;
                Updated.Alternatives_names = editedDrug.Alternatives_names;
                Updated.Dosage_forms_and_strengths = editedDrug.Dosage_forms_and_strengths;
                Updated.Contraindications = editedDrug.Contraindications;
                Updated.Warnings_and_cautions = editedDrug.Warnings_and_cautions;
                Updated.Drug_UrlImg = editedDrug.Drug_UrlImg;
                Updated.Description = editedDrug.Description;
                Updated.Storage_and_handling = editedDrug.Storage_and_handling;
                Updated.Adverse_reactions = editedDrug.Adverse_reactions;
                Updated.AlternativesGpID = editedDrug.AlternativesGpID;
                Updated.Dosage_and_administration = editedDrug.Dosage_and_administration;
                await Context.SaveChangesAsync();
            }
        }
    }
}
