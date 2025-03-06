using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Pages;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Pages;

public class IndexModelTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();
    private readonly IndexModel _sut;

    public IndexModelTests()
    {
        _sut = new IndexModel(_fixture.Mock<IReferralService>().Object);
    }

    [Fact]
    public async Task OnGetShouldCallGetAllAsync()
    {
        //Arrange
        var key = _fixture.Create<string>();
        SetupApimKey(key);

        //Act
        await _sut.OnGet();

        //Assert
        _fixture.Mock<IReferralService>().Verify(r => r.GetAllAsync(key));
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
        await _sut.OnGet();

        //Assert
        _fixture.Mock<IReferralService>().Verify(r => r.GetAllAsync(It.IsAny<string>()), Times.Never);
        _sut.ErrorMessage.Should().Be("APIM key value required");
    }

    [Fact]
    public async Task OnGetShouldSetReferrals()
    {
        //Arrange
        SetupApimKey(_fixture.Create<string>());

        var allReferrals = _fixture.CreateMany<ReferralDbModel>().ToList();
        _fixture.Mock<IReferralService>().Setup(r => r.GetAllAsync(It.IsAny<string>()))
            .ReturnsAsync(allReferrals);

        //Act
        await _sut.OnGet();

        //Assert
        _sut.Referrals.Should().BeEquivalentTo(allReferrals);
    }

    [Fact]
    public async Task OnGetShouldSetErrorMessageWhenException()
    {
        //Arrange
        var exception = _fixture.Create<Exception>();
        SetupApimKey(_fixture.Create<string>());

        _fixture.Mock<IReferralService>().Setup(r => r.GetAllAsync(It.IsAny<string>()))
            .ThrowsAsync(exception);

        //Act
        await _sut.OnGet();

        //Assert
        _sut.ErrorMessage.Should().Be(exception.Message);
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
