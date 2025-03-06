using AutoFixture;
using FluentAssertions;
using Moq;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Repositories;
using WCCG.PAS.Referrals.UI.Services;
using WCCG.PAS.Referrals.UI.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.UI.Unit.Tests.Services;

public class ReferralServiceTests
{
    private readonly IFixture _fixture = new Fixture().WithCustomizations();
    private readonly ReferralService _sut;

    public ReferralServiceTests()
    {
        _sut = _fixture.CreateWithFrozen<ReferralService>();
    }

    [Fact]
    public async Task UpsertAsyncShouldCallRepoMethod()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var referral = _fixture.Create<ReferralDbModel>();
        var upsertResult = _fixture.Create<bool>();

        _fixture.Mock<ICosmosRepository<ReferralDbModel>>().Setup(r => r.UpsertAsync(It.IsAny<string>(), It.IsAny<ReferralDbModel>()))
            .ReturnsAsync(upsertResult);

        //Act
        var result = await _sut.UpsertAsync(apimKey, referral);

        //Assert
        result.Should().Be(upsertResult);
        _fixture.Mock<ICosmosRepository<ReferralDbModel>>().Verify(r => r.UpsertAsync(apimKey, referral));
    }

    [Fact]
    public async Task GetAllAsyncShouldCallRepoMethod()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var allReferrals = _fixture.CreateMany<ReferralDbModel>().ToList();

        _fixture.Mock<ICosmosRepository<ReferralDbModel>>().Setup(r => r.GetAllAsync(It.IsAny<string>()))
            .ReturnsAsync(allReferrals);

        //Act
        var result = await _sut.GetAllAsync(apimKey);

        //Assert
        result.Should().BeEquivalentTo(allReferrals);
        _fixture.Mock<ICosmosRepository<ReferralDbModel>>().Verify(r => r.GetAllAsync(apimKey));
    }

    [Fact]
    public async Task GetByIdAsyncShouldCallRepoMethod()
    {
        //Arrange
        var apimKey = _fixture.Create<string>();
        var id = _fixture.Create<string>();
        var referral = _fixture.Create<ReferralDbModel>();

        _fixture.Mock<ICosmosRepository<ReferralDbModel>>().Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(referral);

        //Act
        var result = await _sut.GetByIdAsync(apimKey, id);

        //Assert
        result.Should().BeEquivalentTo(referral);
        _fixture.Mock<ICosmosRepository<ReferralDbModel>>().Verify(r => r.GetByIdAsync(apimKey, id));
    }
}
