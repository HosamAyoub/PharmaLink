using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;
using PharmaLink_API.Models.Validators;
using PharmaLink_API.Repository.IRepository;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmacyStockController : ControllerBase
    {
        private readonly IPharmacyStockRepository pharmacyStockRepository;
        private readonly IMapper mapper;
        private readonly IValidator<pharmacyProductDTO> validator;

        public PharmacyStockController(IPharmacyStockRepository pharmacyStockRepository 
            , IMapper mapper
            ,IValidator<pharmacyProductDTO> validator
            )
        {
            this.pharmacyStockRepository = pharmacyStockRepository;
            this.mapper = mapper;
            this.validator = validator;
        }
        [HttpGet]
        public IActionResult GetPharmacyStock(int pharmacyId, int pageNumber, int pageSize)
        {
            var pharmacyStocks = pharmacyStockRepository.GetPharmacyStock(pharmacyId, pageNumber, pageSize).ToList();
            List<PharmacyProductDetailsDTO> pharmacyStockDetailsDTOs = new List<PharmacyProductDetailsDTO>();
            foreach (var stock in pharmacyStocks)
            {
                pharmacyStockDetailsDTOs.Add(new PharmacyProductDetailsDTO()
                {
                    DrugId = stock.DrugId,
                    DrugName = stock.Drug?.CommonName,
                    DrugDescription = stock.Drug?.Description,
                    DrugImageUrl = stock.Drug?.Drug_UrlImg,
                    PharmacyId = stock.PharmacyId,
                    PharmacyName = stock.Pharmacy?.Name,
                    Price = stock.Price,
                    QuantityAvailable = stock.QuantityAvailable

                });
            }
            if (pharmacyStocks == null || !pharmacyStocks.Any())
            {
                return NotFound(new { Message = "No pharmacy stock found for the given pharmacy ID." });
            }
            return Ok(pharmacyStockDetailsDTOs);
        }

        [HttpPost]
        public IActionResult AddProductsToPharmacyStock(List<pharmacyProductDTO> pharmacyStockDTO)
        {

           var MappedpharmacyStock =  mapper.Map<List<PharmacyProduct>>(pharmacyStockDTO);
            if (MappedpharmacyStock == null || !MappedpharmacyStock.Any())
            {
                return BadRequest(new { Message = "Invalid pharmacy stock data." });
            }

            pharmacyStockRepository.AddProductsToPharmacyStock(MappedpharmacyStock);
            return Created();


        }
        [HttpPut]
        public IActionResult UpdatePharmacyProduct(pharmacyProductDTO pharmacyStockDTO)
        {
            var validationResult = validator.Validate(pharmacyStockDTO);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Message = "Validation failed.", Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var existingStock = pharmacyStockRepository.GetPharmacyProduct(pharmacyStockDTO.PharmacyId, pharmacyStockDTO.DrugId);
            if (existingStock == null)
            {
                return NotFound(new { Message = "Pharmacy or Product not found." });
            }
            var pharmacyStock = mapper.Map<PharmacyProduct>(pharmacyStockDTO);

            pharmacyStockRepository.UpdatePharmacyProduct(pharmacyStock);
            return NoContent();
        }

        [HttpDelete]
        public IActionResult DeletePharmacyProduct(int pharmacyId, int productId) 
        {
            var existingPharamcyProduct = pharmacyStockRepository.GetPharmacyProduct(pharmacyId,productId);
            if(existingPharamcyProduct == null)
            {
                return NotFound(new { Message = "Pharmacy or Product not found." });
            }
            pharmacyStockRepository.DeletePharmacyProduct(existingPharamcyProduct);
            return NoContent();

        }
    }
}
