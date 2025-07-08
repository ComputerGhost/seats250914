using Core.Application.Reservations;
using Core.Domain.Common.Enumerations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CMS.IntegrationTests.PageTests;

[LocalOnly]
[TestClass]
public class ReservationCreateTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private SelectElement SeatNumber => new(_driver.FindElement(By.Id("SeatNumber")));
    private IWebElement Name => _driver.FindElement(By.Id("Name"));
    private IWebElement Email => _driver.FindElement(By.Id("Email"));
    private IWebElement PhoneNumber => _driver.FindElement(By.Id("PhoneNumber"));
    private IWebElement PreferredLanguage => _driver.FindElement(By.Id("PreferredLanguage"));
    private IWebElement Submit => _driver.FindElement(By.ClassName("btn-primary"));

    [TestInitialize]
    public async Task Initialize()
    {
        _driver = new SeleniumWrapper(languageId: "en");
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

        // Start with a clean slate.
        await TestDataSetup.DeleteAllReservations();

        // Sign in  and go to page
        _driver.SignIn();
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "/reservations/new");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public async Task Form_WhenSubmitted_RedirectsToDetails_AndSavesData()
    {
        // Arrange
        var detailsPathPattern = @"/reservations/(\d+)/details";
        var expectedData = new FetchReservationQueryResponse
        {
            Email = "alice@example.com",
            Name = "Alice",
            PhoneNumber = "555-0000",
            PreferredLanguage = "English",
            ReservedAt = DateTimeOffset.Now,
            SeatNumber = 1,
            Status = ReservationStatus.ReservationConfirmed
        };

        // Act: Fill out the form
        SeatNumber.SelectByValue(expectedData.SeatNumber.ToString());
        Name.SendKeys(expectedData.Name);
        Email.SendKeys(expectedData.Email);
        PhoneNumber.SendKeys(expectedData.PhoneNumber);
        PreferredLanguage.SendKeys(expectedData.PreferredLanguage);

        // Act: Submit and wait for page to load
        Submit.Click();
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path != "/reservations/new");

        // Assert 1: We are on the details page so we can pull the id.
        var currentPath = new UriBuilder(_driver.Url).Path;
        Assert.IsTrue(Regex.IsMatch(currentPath, detailsPathPattern));
        var idParameter = Regex.Match(currentPath, detailsPathPattern).Groups[1].Value;
        Assert.IsTrue(int.TryParse(idParameter, out int reservationId));

        // Assert 2: The saved data is accurate.
        var result = await _mediator.Send(new FetchReservationQuery(reservationId));
        Assert.AreEqual(expectedData.Email, result.Value.Email);
        Assert.AreEqual(expectedData.Name, result.Value.Name);
        Assert.AreEqual(expectedData.PhoneNumber, result.Value.PhoneNumber);
        Assert.AreEqual(expectedData.PreferredLanguage, result.Value.PreferredLanguage);
        Assert.AreEqual(expectedData.ReservedAt, result.Value.ReservedAt, new CloseEnough());
        Assert.AreEqual(expectedData.SeatNumber, result.Value.SeatNumber);
        Assert.AreEqual(expectedData.Status, result.Value.Status);
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
