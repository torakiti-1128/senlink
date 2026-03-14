using Microsoft.AspNetCore.Mvc;
using SenLink.Api.Models;
using SenLink.Shared.Results;

namespace SenLink.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, string operation)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(new ApiResponse
            {
                Success = true,
                Code = (int)result.StatusCode,
                Message = result.Message,
                Operation = operation
            });
        }

        return new ObjectResult(new ApiErrorResponse
        {
            Success = false,
            Code = (int)result.StatusCode,
            Message = result.Message,
            Operation = operation,
            Error = new ErrorDetail
            {
                Type = result.ErrorType,
                Details = result.Errors
            }
        })
        {
            StatusCode = (int)result.StatusCode
        };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, string operation)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(new ApiResponse<T>
            {
                Success = true,
                Code = (int)result.StatusCode,
                Message = result.Message,
                Operation = operation,
                Data = result.Data
            });
        }

        return new ObjectResult(new ApiErrorResponse
        {
            Success = false,
            Code = (int)result.StatusCode,
            Message = result.Message,
            Operation = operation,
            Error = new ErrorDetail
            {
                Type = result.ErrorType,
                Details = result.Errors
            }
        })
        {
            StatusCode = (int)result.StatusCode
        };
    }
}
