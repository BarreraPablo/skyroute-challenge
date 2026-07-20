using SkyRoute.Core.Services;
using SkyRoute.Core.Services.Airports;

namespace SkyRoute.Core.Tests.Services.Airports;

[TestFixture]
public class AirportReferenceServiceTests
{
    private AirportReferenceService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new AirportReferenceService();
    }

    [Test]
    public void GetAirports_ReturnsAllKnownAirports()
    {
        // Act
        var airports = _sut.GetAirports();

        // Assert
        Assert.That(airports, Has.Count.EqualTo(6));
        Assert.That(airports.Select(airport => airport.Code), Does.Contain("JFK"));
        Assert.That(airports.Select(airport => airport.Code), Does.Contain("LHR"));
    }

    [Test]
    public void IsValidAirportCode_WithKnownCode_ReturnsTrue()
    {
        // Act
        var isValid = _sut.IsValidAirportCode("MAD");

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValidAirportCode_WithUnknownCode_ReturnsFalse()
    {
        // Act
        var isValid = _sut.IsValidAirportCode("XYZ");

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValidAirportCode_IsCaseInsensitive()
    {
        // Act
        var isValid = _sut.IsValidAirportCode("jfk");

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void GetCountryCodeByAirportCode_WithKnownCode_ReturnsCountryCode()
    {
        // Act
        var countryCode = _sut.GetCountryCodeByAirportCode("BCN");

        // Assert
        Assert.That(countryCode, Is.EqualTo("ES"));
    }

    [Test]
    public void GetCountryCodeByAirportCode_WithUnknownCode_ReturnsNull()
    {
        // Act
        var countryCode = _sut.GetCountryCodeByAirportCode("XYZ");

        // Assert
        Assert.That(countryCode, Is.Null);
    }
}
