using ComplectGroup.Application.DTOs;
using ComplectGroup.Web.Controllers;
using System.ComponentModel.DataAnnotations;

namespace ComplectGroup.Tests.Validation;

public class DtoValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void CreateChapterRequest_EmptyName_IsInvalid()
    {
        var model = new CreateChapterRequest { Name = "" };

        var results = Validate(model);

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateChapterRequest.Name)));
    }

    [Fact]
    public void CreateChapterRequest_ValidName_IsValid()
    {
        var model = new CreateChapterRequest { Name = "Раздел" };

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void CreatePartRequest_MissingNameOrChapter_IsInvalid()
    {
        var model1 = new CreatePartRequest { Name = "", ChapterId = 1 };
        var model2 = new CreatePartRequest { Name = "Деталь", ChapterId = 0 };

        Assert.NotEmpty(Validate(model1));
        Assert.NotEmpty(Validate(model2));
    }

    [Fact]
    public void CreatePartRequest_Valid_IsValid()
    {
        var model = new CreatePartRequest { Name = "Деталь", ChapterId = 1 };

        Assert.Empty(Validate(model));
    }

    [Fact]
    public void UpdatePartRequest_MissingNameOrChapter_IsInvalid()
    {
        var model1 = new UpdatePartRequest { Name = "", ChapterId = 1 };
        var model2 = new UpdatePartRequest { Name = "Деталь", ChapterId = 0 };

        Assert.NotEmpty(Validate(model1));
        Assert.NotEmpty(Validate(model2));
    }

    [Fact]
    public void CreateComplectationRequest_EmptyNumber_IsInvalid()
    {
        var model = new CreateComplectationRequest { Number = "" };

        var results = Validate(model);

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateComplectationRequest.Number)));
    }

    [Fact]
    public void CreateComplectationRequest_NegativeWeightOrVolume_IsInvalid()
    {
        var model1 = new CreateComplectationRequest { Number = "C1", TotalWeight = -1 };
        var model2 = new CreateComplectationRequest { Number = "C1", TotalVolume = -1 };

        Assert.NotEmpty(Validate(model1));
        Assert.NotEmpty(Validate(model2));
    }

    [Fact]
    public void CreateComplectationRequest_Valid_IsValid()
    {
        var model = new CreateComplectationRequest { Number = "C1", TotalWeight = 10, TotalVolume = 2 };

        Assert.Empty(Validate(model));
    }
}
