using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;

namespace Public.IntegrationTests.SeatSelectorTests;

[LocalOnly]
[TestClass]
public class SeatFormTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement Section => _driver.FindElement(By.Id("reserve-seats"));
    private IList<SelectElement> Selects => [.. Section.FindElements(By.ClassName("form-select")).Select(x => new SelectElement(x))];
    private IWebElement Submit => Section.FindElement(By.ClassName("btn-primary"));

    [TestInitialize]
    public async Task Initialize()
    {
        _driver = new SeleniumWrapper();
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

        // Start with a clean slate.
        await TestDataSetup.DeleteAllReservations();
        await _mediator.Send(TestDataSetup.WorkingSaveConfigurationCommand);

        // Go to page
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void Select_HasOrderedAndUniqueItems()
    {
        // Arrange
        var expected = Enumerable.Range(1, 100).Select(x => x.ToString());

        // Act
        var actual = Selects[0].Options.Select(option => option.Text);

        // Assert
        Assert.AreEqual(101, actual.Count());
        Assert.AreEqual("", actual.First());
        Assert.IsTrue(Enumerable.SequenceEqual(expected, actual.Skip(1)));
    }

    [TestMethod]
    public void Select_WhenSelected_AlsoSelectsOnMap()
    {
        // Arrange
        var getMapSelections = () => Section.FindElements(By.CssSelector(".audience .selected"));

        // Act
        var count = 0;
        foreach (var select in Selects)
        {
            var seatNumber = count + 1;
            Selects[count++].SelectByValue(seatNumber.ToString());
        }

        // Assert
        var selectedSeats = getMapSelections().Select(x => x.Text);
        for (int i = 1; i <= count; ++i)
        {
            Assert.Contains(i.ToString(), selectedSeats);
        }
    }

    [TestMethod]
    public void Submit_WhenClicked_AndFormIsValid_GoesToReservationForm()
    {
        // Arrange
        const string reservationPath = "/reservation/new";

        // Act 1: Fill out and submit form
        Selects[0].SelectByIndex(1);
        _driver.ScrollTo(Submit);
        Submit.Click();

        // Act 2: Wait for page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == reservationPath);

        // Assert
        Assert.AreEqual(reservationPath, new UriBuilder(_driver.Url).Path);
    }

    [TestMethod]
    public void Submit_WhenClickedTwice_AndFormIsValid_GoesToReservationForm()
    {
        // Arrange
        const string reservationPath = "/reservation/new";

        // Act 1: Fill out form
        Selects[0].SelectByIndex(1);

        // Act 2: Submit the form twice...
        // The website is too fast to use `Submit.Click()` for this.
        var clickScript = "$('#reserve-seats .btn-primary').click();";
        _driver.ExecuteScript($"{clickScript}{clickScript}");

        // Act : Wait for page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == reservationPath);

        // Assert
        Assert.AreEqual(reservationPath, new UriBuilder(_driver.Url).Path);
    }
}
