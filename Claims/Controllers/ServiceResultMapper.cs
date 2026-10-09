using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

internal static class ServiceResultMapper
{
    public static ActionResult ToActionResult<T>(this ControllerBase controller, ServiceResult<T> result)
    {
        return result switch
        {
            ServiceResult<T>.Success success => controller.Ok(success.Value),
            ServiceResult<T>.NotFound => controller.NotFound(),
            ServiceResult<T>.Invalid invalid => controller.ValidationProblem(new ValidationProblemDetails(invalid.Errors)),
            _ => throw new InvalidOperationException($"Unexpected result {result.GetType().Name}.")
        };
    }

    public static ActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
    {
        return result switch
        {
            ServiceResult.Success => controller.Ok(),
            ServiceResult.NotFound => controller.NotFound(),
            ServiceResult.Invalid invalid => controller.ValidationProblem(new ValidationProblemDetails(invalid.Errors)),
            _ => throw new InvalidOperationException($"Unexpected result {result.GetType().Name}.")
        };
    }
}
