using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace CMS.IntegrationTests.PageTests;

[LocalOnly]
[TestClass]
public class ReservationEditTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement SeatNumber => _driver.FindElement(By.Id("SeatNumber"));
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
    public async Task Form_WhenLoaded_HasCorrectData()
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

        // Act 2: Navigate to edit page
        var editPage = $"{ConfigurationAccessor.Instance.TargetUrl}/reservations/{reservationId}/edit";
        _driver.Navigate().GoToUrl(editPage);

        // Assert
        Assert.AreEqual(expectedData.Email, Email.GetAttribute("value"));
        Assert.AreEqual(expectedData.Name, Name.GetAttribute("value"));
        Assert.AreEqual(expectedData.PhoneNumber, PhoneNumber.GetAttribute("value"));
        Assert.AreEqual(expectedData.PreferredLanguage, PreferredLanguage.GetAttribute("value"));
        Assert.AreEqual(expectedData.SeatNumber.ToString(), SeatNumber.GetAttribute("value"));
    }

    [TestMethod]
    public async Task Form_WhenSubmitted_RedirectsToDetails_AndSavesData()
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
        var detailsPath = $"/reservations/{reservationId}/details";

        // Act 2: Navigate to edit page
        var editUrl = $"{ConfigurationAccessor.Instance.TargetUrl}/reservations/{reservationId}/edit";
        _driver.Navigate().GoToUrl(editUrl);

        // Act 3: Fill out the form
        _driver.SetValue(Name, expectedData.Name);
        _driver.SetValue(Email, expectedData.Email);
        _driver.SetValue(PhoneNumber, expectedData.PhoneNumber);
        _driver.SetValue(PreferredLanguage, expectedData.PreferredLanguage);

        // Act 3: Submit and wait for page to load
        Submit.Click();
        _driver.WaitUntil(d => Regex.IsMatch(new UriBuilder(d.Url).Path, detailsPath));

        // Assert 1: We are on the details page
        Assert.IsTrue(Regex.IsMatch(new UriBuilder(_driver.Url).Path, detailsPath));

        // Assert 2: The saved data is accurate.
        var result = await _mediator.Send(new FetchReservationQuery(reservationId));
        Assert.AreEqual(expectedData.Email, result.Value.Email);
        Assert.AreEqual(expectedData.Name, result.Value.Name);
        Assert.AreEqual(expectedData.PhoneNumber, result.Value.PhoneNumber);
        Assert.AreEqual(expectedData.PreferredLanguage, result.Value.PreferredLanguage);
    }
}
