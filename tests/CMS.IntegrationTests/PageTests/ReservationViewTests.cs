using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using System.Diagnostics.CodeAnalysis;

namespace CMS.IntegrationTests.PageTests;

[LocalOnly]
[TestClass]
public class ReservationViewTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement SeatNumbers => _driver.FindElement(By.Id("SeatNumbers"));
    private IWebElement Name => _driver.FindElement(By.Id("Name"));
    private IWebElement Email => _driver.FindElement(By.Id("Email"));
    private IWebElement PhoneNumber => _driver.FindElement(By.Id("PhoneNumber"));
    private IWebElement PreferredLanguage => _driver.FindElement(By.Id("PreferredLanguage"));
    private IWebElement ReservedAt => _driver.FindElement(By.Id("ReservedAt"));

    [TestInitialize]
    public async Task Initialize()
    {
        _driver = new SeleniumWrapper(languageId: "en");
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

        // Start with a clean slate.
        await TestDataSetup.DeleteAllReservations();

        // Sign in 
        _driver.SignIn();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public async Task Page_WhenLoaded_HasCorrectData()
    {
        // Arrange
        var expectedData = new AdminReserveSeatCommand
        {
            Email = "alice@example.com",
            Name = "Alice",
            PhoneNumber = "555-0000",
            PreferredLanguage = "English",
            SeatNumber = 1,
        };

        // Act 1: Create reservation
        var reservationId = (await _mediator.Send(expectedData)).Value;

        // Act 2: Navigate to details page
        var detailsPage = $"{ConfigurationAccessor.Instance.TargetUrl}/reservations/{reservationId}/details";
        _driver.Navigate().GoToUrl(detailsPage);

        // Assert
        Assert.AreEqual(expectedData.Email, Email.Text);
        Assert.AreEqual(expectedData.Name, Name.Text);
        Assert.AreEqual(expectedData.PhoneNumber, PhoneNumber.Text);
        Assert.AreEqual(expectedData.PreferredLanguage, PreferredLanguage.Text);
        Assert.AreEqual(expectedData.SeatNumber.ToString(), SeatNumbers.Text);
        var renderedTime = DateTimeOffset.Parse(ReservedAt.GetAttribute("datetime") + "z");
        Assert.AreEqual(DateTimeOffset.UtcNow, renderedTime, new CloseEnough());
    }

    private class CloseEnough : IEqualityComparer<DateTimeOffset>
    {
        public bool Equals(DateTimeOffset x, DateTimeOffset y)
        {
            var diff = Math.Abs(x.UtcTicks - y.UtcTicks);
            return TimeSpan.FromTicks(diff) < TimeSpan.FromMinutes(1);
        }

        public int GetHashCode([DisallowNull] DateTimeOffset obj) => throw new NotImplementedException();
    }
}
