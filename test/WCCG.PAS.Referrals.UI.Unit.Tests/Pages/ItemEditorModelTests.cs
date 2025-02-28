using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        var expectedJson = JsonSerializer.Serialize(_referralDbModel, _jsonOptions);

        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(_referralDbModel);
        //Act
        await _sut.OnGet(_referralDbModel.ReferralId!);

        //Assert
        _sut.ReferralJson.Should().Be(expectedJson);

        _fixture.Mock<IReferralService>().Verify(r => r.GetByIdAsync(_referralDbModel.ReferralId!));
    }

    [Fact]
    public async Task OnPostShouldCallUpsertAsyncWhenDeserializedAndValidatedSuccessfully()
    {
        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();

        _sut.IsSaved.Should().BeTrue();
        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.Is<ReferralDbModel>(r => r.IsEquivalentTo(_referralDbModel))));
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenDeserializationFailed()
    {
        //Arrange
        var invalidReferral = _fixture.Create<string>();
        _sut.ReferralJson = JsonSerializer.Serialize(invalidReferral, _jsonOptions);

        _fixture.Mock<IReferralService>().Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(_referralDbModel);

        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();

        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().NotBeEmpty();

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.IsAny<ReferralDbModel>()), Times.Never());
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenValidationFailed()
    {
        //Arrange
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

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.IsAny<ReferralDbModel>()), Times.Never());
    }

    [Fact]
    public async Task OnPostShouldHandleErrorsWhenUpsertFailed()
    {
        //Arrange
        var errorMessage = _fixture.Create<string>();

        _fixture.Mock<IReferralService>().Setup(s => s.UpsertAsync(It.IsAny<ReferralDbModel>()))
            .ThrowsAsync(new ArgumentException(errorMessage));

        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<PageResult>();

        _sut.IsSaved.Should().BeFalse();
        _sut.ErrorMessage.Should().Be(errorMessage);

        _fixture.Mock<IReferralService>().Verify(s => s.UpsertAsync(It.Is<ReferralDbModel>(r => r.IsEquivalentTo(_referralDbModel))));
    }

    [Fact]
    public async Task OnPostShouldReturnBadRequestWhenReferralJsonIsNull()
    {
        //Arrange
        _sut.ReferralJson = null;

        //Act
        var result = await _sut.OnPost();

        //Assert
        result.Should().BeOfType<BadRequestResult>();
    }
}
