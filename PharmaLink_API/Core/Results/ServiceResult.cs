using PharmaLink_API.Core.Enums;

namespace PharmaLink_API.Core.Results
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public ErrorType? ErrorType { get; set; }

        public static ServiceResult SuccessResult()
        {
            return new ServiceResult { Success = true };
        }

        public static ServiceResult ErrorResult(string errorMessage, ErrorType errorType = Enums.ErrorType.Internal)
        {
            return new ServiceResult { Success = false, ErrorMessage = errorMessage, ErrorType = errorType };
        }

        public static ServiceResult ValidationErrorResult(List<string> validationErrors)
        {
            return new ServiceResult { Success = false, ValidationErrors = validationErrors, ErrorType = Enums.ErrorType.Validation };
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; set; }

        public static ServiceResult<T> SuccessResult(T data)
        {
            return new ServiceResult<T> { Success = true, Data = data };
        }

        public static new ServiceResult<T> ErrorResult(string errorMessage, ErrorType errorType = Enums.ErrorType.Internal)
        {
            return new ServiceResult<T> { Success = false, ErrorMessage = errorMessage, ErrorType = errorType };
        }

        public static new ServiceResult<T> ValidationErrorResult(List<string> validationErrors)
        {
            return new ServiceResult<T> { Success = false, ValidationErrors = validationErrors, ErrorType = Enums.ErrorType.Validation };
        }
    }
}