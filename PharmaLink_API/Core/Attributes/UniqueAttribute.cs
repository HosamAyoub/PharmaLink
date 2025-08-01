using PharmaLink_API.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PharmaLink_API.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UniqueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dbContext = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext))!;

            var entityType = validationContext.ObjectInstance.GetType();
            var propertyName = validationContext.MemberName!;

            var existingEntity = dbContext.Model.FindEntityType(entityType);
            var tableName = existingEntity?.GetTableName();

            if (tableName == null)
                return ValidationResult.Success;

            var query = $"SELECT 1 FROM {tableName} WHERE {propertyName} = {{0}} AND Id <> {{1}}";
            var valueParam = value?.ToString();
            var entityId = (validationContext.ObjectInstance as dynamic)?.Id?.ToString() ?? "0";

            var exists = dbContext.Database.ExecuteSqlRaw(query, new object[] { valueParam, entityId }) > 0;

            return exists
                ? new ValidationResult(GetErrorMessage(propertyName, value))
                : ValidationResult.Success;
        }

        private string GetErrorMessage(string propertyName, object value)
        {
            return $"{propertyName} '{value}' already exists. Please choose a different value.";
        }
    }
}