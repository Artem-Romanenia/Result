using Microsoft.AspNetCore.Mvc;
using System.Net;
using Xunit;

namespace Ardalis.Result.AspNetCore.UnitTests;

[Collection("ResultStatusMapTest")]
public class ResultConventionDefaultResultStatusMapModified : BaseResultConventionTest
{
    [Fact]
    public void RemoveResultStatus()
    {
        ResultStatusMap.Initialize(map => map
            .AddDefaultMap()
            .Remove(ResultStatus.Unauthorized)
            .Remove(ResultStatus.Forbidden));

        var convention = new ResultConvention();

        var actionModelBuilder = new ActionModelBuilder()
            .AddActionFilter(new TranslateResultToActionResultAttribute());

        var actionModel = actionModelBuilder.GetActionModel();

        convention.Apply(actionModel);

        Assert.Equal(4, actionModel.Filters.Where(f => f is ProducesResponseTypeAttribute).Count());

        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 204, typeof(void)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 404, typeof(ProblemDetails)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 400, typeof(ValidationProblemDetails)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 422, typeof(ProblemDetails)));
    }

    [Fact]
    public void ChangeResultStatus()
    {
        ResultStatusMap.Initialize(map => map
            .AddDefaultMap()
            .For(ResultStatus.Error, HttpStatusCode.InternalServerError));

        var convention = new ResultConvention();

        var actionModelBuilder = new ActionModelBuilder()
            .AddActionFilter(new TranslateResultToActionResultAttribute());

        var actionModel = actionModelBuilder.GetActionModel();

        convention.Apply(actionModel);

        Assert.Equal(6, actionModel.Filters.Where(f => f is ProducesResponseTypeAttribute).Count());

        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 204, typeof(void)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 404, typeof(ProblemDetails)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 400, typeof(ValidationProblemDetails)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 401, typeof(void)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 403, typeof(void)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 500, typeof(void)));
    }

    [Theory]
    [InlineData("Index", typeof(void), typeof(HttpPostAttribute), 204)]
    [InlineData("Index", typeof(void), typeof(HttpDeleteAttribute), 204)]
    [InlineData("Index", typeof(void), typeof(HttpGetAttribute), 204)]
    [InlineData(nameof(TestController.ResultString), typeof(string), typeof(HttpPostAttribute), 201)]
    [InlineData(nameof(TestController.ResultString), typeof(string), typeof(HttpDeleteAttribute), 204)]
    [InlineData(nameof(TestController.ResultString), typeof(string), typeof(HttpGetAttribute), 200)]
    [InlineData(nameof(TestController.ResultEnumerableString), typeof(IEnumerable<string>), typeof(HttpPostAttribute), 201)]
    [InlineData(nameof(TestController.ResultEnumerableString), typeof(IEnumerable<string>), typeof(HttpDeleteAttribute), 204)]
    [InlineData(nameof(TestController.ResultEnumerableString), typeof(IEnumerable<string>), typeof(HttpGetAttribute), 200)]
    public void ChangeResultStatus_ForSpecificMethods(string actionName, Type expectedType, Type attributeType, int expectedStatusCode)
    {
        ResultStatusMap.Initialize(map => map
            .AddDefaultMap()
            .For(ResultStatus.Ok, HttpStatusCode.OK, opts => opts
                .For("POST", HttpStatusCode.Created)
                .For("DELETE", HttpStatusCode.NoContent)));

        var convention = new ResultConvention();

        var actionModelBuilder = new ActionModelBuilder()
            .AddActionFilter(new TranslateResultToActionResultAttribute())
            .AddActionAttribute((Attribute)Activator.CreateInstance(attributeType)!);

        var actionModel = actionModelBuilder.GetActionModel(actionName);

        convention.Apply(actionModel);

        Assert.Equal(6, actionModel.Filters.Where(f => f is ProducesResponseTypeAttribute).Count());

        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, expectedStatusCode, expectedType));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 404, typeof(ProblemDetails)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 400, typeof(ValidationProblemDetails)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 401, typeof(void)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 403, typeof(void)));
        Assert.Contains(actionModel.Filters, f => IsProducesResponseTypeAttribute(f, 422, typeof(ProblemDetails)));
    }
}
