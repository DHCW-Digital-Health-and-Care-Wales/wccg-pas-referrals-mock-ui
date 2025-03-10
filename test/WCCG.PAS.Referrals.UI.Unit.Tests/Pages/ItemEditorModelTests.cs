using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Pages;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Pages;

public class ItemEditorModelTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();
    private readonly ItemEditorModel _sut;

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private readonly ReferralDbModel _referralDbModel;

    public ItemEditorModelTests()
    {
        _referralDbModel = _fixture.Create<ReferralDbModel>();

        _sut = new ItemEditorModel(
            _fixture.Mock<IReferralService>().Object,
            _fixture.Mock<IValidator<ReferralDbModel>>().Object,
            _fixture.Mock<ILogger<ItemEditorModel>>().Object)
        {
            ReferralJson = JsonSerializer.Serialize(_referralDbModel, _jsonOptions),
        };
    }

    [Fact]
    public async Task OnGetShouldCallGetByIdAsync()
    {
        //Arrange
        var key = _fixture.Create<string>();
        SetupApimKey(key);

        var expectedJson = JsonSerializer.Serialize(_referralDbModel, _jsonOptions);

        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(_referralDbModel);

        //Act
        await _sut.OnGet(_referralDbModel.ReferralId!);

        //Assert
        _sut.ReferralJson.Should().Be(expectedJson);
        _fixture.Mock<IReferralService>().Verify(r => r.GetByIdAsync(key, _referralDbModel.ReferralId!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task OnGetShouldHandleEmptyApimKey(string? apimKey)
    {
        //Arrange
        SetupApimKey(apimKey);

        //Act
        await _sut.OnGet(_referralDbModel.ReferralId!);

        //Assert
        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().Be("APIM key value required");
        _fixture.Mock<IReferralService>().Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnGetShouldSetErrorMessageWhenException()
    {
        //Arrange
        var exception = _fixture.Create<Exception>();
        SetupApimKey(_fixture.Create<string>());

        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(exception);

        //Act
        await _sut.OnGet(_fixture.Create<string>());

        //Assert
        _sut.ErrorMessage.Should().Be(exception.Message);
    }

    [Fact]
    public async Task OnPostShouldCallUpsertAsyncWhenDeserializedAndValidatedSuccessfully()
    {
        //Act
        var key = _fixture.Create<string>();
        SetupApimKey(key);

        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();

        _sut.IsSaved.Should().BeTrue();
        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(key, It.Is<ReferralDbModel>(r => r.IsEquivalentTo(_referralDbModel))));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task OnPostShouldHandleEmptyApimKey(string? apimKey)
    {
        //Arrange
        SetupApimKey(apimKey);

        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();

        _sut.ErrorMessage.Should().Be("APIM key value required");
        _fixture.Mock<IReferralService>().Verify(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<ReferralDbModel>()), Times.Never);
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenDeserializationFailed()
    {
        //Arrange
        SetupApimKey(_fixture.Create<string>());

        var invalidReferral = _fixture.Create<string>();
        _sut.ReferralJson = JsonSerializer.Serialize(invalidReferral, _jsonOptions);

        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(_referralDbModel);

        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();

        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().NotBeEmpty();

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.IsAny<string>(), It.IsAny<ReferralDbModel>()), Times.Never());
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenValidationFailed()
    {
        //Arrange
        SetupApimKey(_fixture.Create<string>());

        var validationResult = _fixture.Build<ValidationResult>()
            .With(x => x.Errors, _fixture.CreateMany<ValidationFailure>().ToList)
            .Create();
        var expectedErrorMessage = validationResult.Errors.Select(x => x.ErrorMessage).Aggregate((f, s) => f + "<br/>" + s);

        _fixture.Mock<IValidator<ReferralDbModel>>().Setup(r => r.ValidateAsync(It.IsAny<ReferralDbModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();

        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().Be(expectedErrorMessage);

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.IsAny<string>(), It.IsAny<ReferralDbModel>()), Times.Never());
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenUpsertFailed()
    {
        //Arrange
        SetupApimKey(_fixture.Create<string>());

        var errorMessage = _fixture.Create<string>();

        _fixture.Mock<IReferralService>().Setup(s => s.UpsertAsync(It.IsAny<string>(), It.IsAny<ReferralDbModel>()))
            .ThrowsAsync(new ArgumentException(errorMessage));

        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();

        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().Be(errorMessage);

        _fixture.Mock<IReferralService>().Verify(s =>
            s.UpsertAsync(It.IsAny<string>(), It.Is<ReferralDbModel>(r => r.IsEquivalentTo(_referralDbModel))));
    }

    [Fact]
    public async Task OnPostShouldReturnPageResultWhenReferralJsonIsNull()
    {
        //Arrange
        SetupApimKey(_fixture.Create<string>());

        _sut.ReferralJson = null;

        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();
    }

    private void SetupApimKey(string? key)
    {
        _sut.PageContext = new PageContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new PageActionDescriptor(),
            new ModelStateDictionary()));
        var keys = _fixture.Mock<IRequestCookieCollection>();
        keys.Setup(x => x["ApimKey"]).Returns(key);
        _sut.HttpContext.Request.Cookies = keys.Object;
    }
}
