using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Data;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.IRepository;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DrugController : ControllerBase
    {
        private readonly IDrugRepository DrugRepo;
        public readonly ApplicationDbContext Context;

        public DrugController(IDrugRepository drugRepo, ApplicationDbContext _context)
        {
            this.DrugRepo = drugRepo;
            Context = _context;
        }


        [HttpGet("{PageIndex:int}")]

        public async Task<IEnumerable<Drug>> GetBatch(int PageIndex)
        {
            return await DrugRepo.GetBatchDrugs(PageIndex);
        }

        // GET api/<DrugController>/paracetamol
        [HttpGet("Drug_Name")]
        public async Task<List<Drug>> GetByName(string Dname)
        {
            return await DrugRepo.GetAllAsync(D => D.CommonName.ToLower().StartsWith(Dname.ToLower()));
        }

        [HttpGet("Category")]
        public async Task<List<Drug>> GetByCategory(string Cname)
        {
            return await DrugRepo.GetAllAsync(D => D.Category.ToLower().StartsWith(Cname.ToLower()));
        }

        [HttpGet("Active_Ingredient")]
        public async Task<List<Drug>> GetByActiveIngredient(string Active_Ingredient)
        {
            return await DrugRepo.GetAllAsync(D => D.Alternatives_names.ToLower().StartsWith(Active_Ingredient.ToLower()));
        }

        [HttpGet("q={SearchAnything}")]
        public async Task<List<Drug>> Search(string SearchAnything)
        { 
            List<Drug> SearchList = new List<Drug>();
            SearchList.AddRange(await DrugRepo.GetAllAsync(D => D.CommonName.ToLower().StartsWith(SearchAnything.ToLower())));
            SearchList.AddRange(await DrugRepo.GetAllAsync(D => D.Category.ToLower().StartsWith(SearchAnything.ToLower())));
            SearchList.AddRange(await DrugRepo.GetAllAsync(D => D.ActiveIngredient.ToLower().StartsWith(SearchAnything.ToLower())));
            return SearchList;
        }


        // POST api/<DrugController>
        [HttpPost]
        public async Task Post([FromBody] Drug NewDrug)
        {
            await DrugRepo.CreateAsync(NewDrug);
        }

        // PUT api/<DrugController>/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] Drug EditedDrug)
        {
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
