using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

#if NET6_0_OR_GREATER
namespace Ardalis.Result.AspNetCore;
public static partial class ResultExtensions
{
    /// <summary>
    /// Convert a <see cref="Result{T}"/> to an instance of <see cref="Microsoft.AspNetCore.Http.IResult"/>
    /// </summary>
    /// <typeparam name="T">The value being returned</typeparam>
    /// <param name="result">The Ardalis.Result to convert to an Microsoft.AspNetCore.Http.IResult</param>
    /// <returns></returns>
    public static Microsoft.AspNetCore.Http.IResult ToMinimalApiResult<T>(this Result<T> result)
    {
        return ToMinimalApiResult((IResult)result);
    }

    /// <summary>
    /// Convert a <see cref="Result"/> to an instance of <see cref="Microsoft.AspNetCore.Http.IResult"/>
    /// </summary>
    /// <param name="result">The Ardalis.Result to convert to an Microsoft.AspNetCore.Http.IResult</param>
    /// <returns></returns>
    public static Microsoft.AspNetCore.Http.IResult ToMinimalApiResult(this Result result)
    {
        return ToMinimalApiResult((IResult)result);
    }

    internal static Microsoft.AspNetCore.Http.IResult ToMinimalApiResult(this IResult result, HttpContext context = null)
    {
        if (!ResultStatusMap.Instance.ContainsKey(result.Status))
        {
            throw new NotSupportedException($"Result {result.Status} conversion is not supported.");
        }

        var method = context?.Request.Method;

        var resultStatusOptions = ResultStatusMap.Instance[result.Status];
        var statusCode = (int)resultStatusOptions.GetStatusCode(method);

        switch (result.Status)
        {
            case ResultStatus.Ok:
                return typeof(Result).IsInstanceOfType(result)
                    ? Results.Ok()
                    : Results.Ok(result.GetValue());
            default:
                return resultStatusOptions.ResponseType == null
                    ? Results.StatusCode(statusCode)
                    : Results.Problem(WithStatusCode(resultStatusOptions.GetResponseObject(result), statusCode));
        }
    }

    private static ProblemDetails WithStatusCode(ProblemDetails details, int statusCode)
    {
        details.Status = statusCode;
        return details;
    }
}
#endif
