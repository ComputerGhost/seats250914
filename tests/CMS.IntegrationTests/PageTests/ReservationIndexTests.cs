using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;

namespace CMS.IntegrationTests.PageTests;

[LocalOnly]
[TestClass]
public class ReservationIndexTests
{
    const string EMPTY_TEXT = "아직 예약된 좌석이 없습니다.";

    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement Approve => _driver.FindElement(By.Id("approve"));
    private IWebElement SelectAll => _driver.FindElement(By.ClassName("form-check-input"));
    private IWebElement Search => _driver.FindElement(By.Id("Search"));

    private List<IWebElement> Rows => [.. _driver.FindElements(By.TagName("tr")).Skip(2)];
    private IWebElement SeatNumber(int row) => Rows[row].FindElements(By.TagName("td"))[1];
    private IWebElement Select(int row) => Rows[row].FindElement(By.ClassName("form-check-input"));
    private IWebElement Status(int row) => Rows[row].FindElements(By.TagName("td"))[3];

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
    public async Task Approve_WhenClicked_OnlyApprovesSelected()
    {
        // Arrange
        var pageUrl = ConfigurationAccessor.Instance.TargetUrl + "/reservations";
        const int TARGET_INDEX1 = 0;
        const int TARGET_INDEX2 = 1;
        const int IGNORED_INDEX = 2;
        await CreateReservation(1);
        await CreateReservation(2);
        await CreateReservation(3);

        // Act: Go to page with random parameter so we can later detect navigation.
        _driver.Navigate().GoToUrl(pageUrl + "?test");

        // Act: Select seats then approve
        Select(TARGET_INDEX1).Click();
        Select(TARGET_INDEX2).Click();
        Approve.Click();

        // Act: Wait for navigation
        _driver.WaitUntil(d => new UriBuilder(d.Url).Query != "test");
        _driver.WaitUntil(d => Rows.Count == 3);

        // Assert
        Assert.AreEqual("승인됨", Status(TARGET_INDEX1).Text);
        Assert.AreEqual("승인됨", Status(TARGET_INDEX2).Text);
        Assert.AreEqual("승인 대기 중", Status(IGNORED_INDEX).Text);
    }

    [TestMethod]
    public async Task SelectAll_WhenClicked_OnlySelectsVisibleRows()
    {
        // Arrange
        await CreateReservation(1);
        await CreateReservation(2, "bob");
        await CreateReservation(3);
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "/reservations");

        // Act 1: Filter then select all
        Search.SendKeys("bob");
        SelectAll.Click();

        // Assert
        Assert.AreEqual(1, Rows.Count);
        Assert.AreNotEqual(EMPTY_TEXT, Rows[0].Text);
        Assert.IsTrue(Select(0).Selected);

        // Act 2: Unfilter
        Search.Clear();
        Search.SendKeys("보기"); // to trigger update

        // Assert
        Assert.AreEqual(3, Rows.Count);
        Assert.IsFalse(Select(0).Selected);
        Assert.IsTrue(Select(1).Selected);
        Assert.IsFalse(Select(2).Selected);
    }

    [TestMethod]
    public async Task Table_WhenEmpty_RendersEmptyInKorean()
    {
        // Arrange
        var pageUrl = ConfigurationAccessor.Instance.TargetUrl + "/reservations";

        // Act 1: Go to the page
        _driver.Navigate().GoToUrl(pageUrl + "?test");

        // Assert 1: Default empty text
        Assert.AreEqual(1, Rows.Count);
        Assert.AreEqual(EMPTY_TEXT, Rows[0].Text);

        // Arrange 2: Create some reservations
        await CreateReservation(1);
        await CreateReservation(2);
        await CreateReservation(3);

        // Act 2: Refresh then Filter to show nothing
        _driver.Navigate().GoToUrl(pageUrl);
        _driver.WaitUntil(d => new UriBuilder(d.Url).Query != "test");
        Search.SendKeys("--do-not-match-anything--");

        // Assert 2: Still displays correct text.
        Assert.AreEqual(1, Rows.Count);
        Assert.AreEqual("검색 결과가 없습니다", Rows[0].Text);
    }

    [TestMethod]
    public async Task Table_ByDefault_SortsByDateAscending()
    {
        // Arrange
        const int SEAT1 = 10;
        const int SEAT2 = 5;
        const int SEAT3 = 1;
        await CreateReservation(SEAT1);
        await CreateReservation(SEAT2);
        await CreateReservation(SEAT3);

        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "/reservations");

        // Assert
        Assert.AreEqual(3, Rows.Count);
        Assert.AreEqual(SEAT1.ToString(), SeatNumber(0).Text);
        Assert.AreEqual(SEAT2.ToString(), SeatNumber(1).Text);
        Assert.AreEqual(SEAT3.ToString(), SeatNumber(2).Text);
    }

    [TestMethod]
    public async Task Table_WhenFiltered_FiltersResults()
    {
        // Arrange
        const int PENDING_INDEX = 0;
        const int APPROVED_INDEX = 1;
        const int REJECTED_INDEX = 2;
        var pendingId = await CreateReservation(PENDING_INDEX + 1);
        var approvedId = await CreateReservation(APPROVED_INDEX + 1);
        var rejectedId = await CreateReservation(REJECTED_INDEX + 1);
        await _mediator.Send(new ApproveReservationCommand(approvedId));
        await _mediator.Send(new RejectReservationCommand(rejectedId));

        // Act 1: Navigate to page with filter URL
        var pageUrl = ConfigurationAccessor.Instance.TargetUrl + "/reservations";
        _driver.Navigate().GoToUrl(pageUrl + "?search=승인됨");

        // Assert
        Assert.AreEqual(1, Rows.Count);
        Assert.AreNotEqual(EMPTY_TEXT, Rows[0].Text);
        Assert.AreEqual("승인됨", Status(0).Text);

        // Act 2: Clear the search
        Search.Clear();
        Search.SendKeys("보기"); // to trigger update

        // Assert: All three rows are visible.
        Assert.IsTrue(Rows[PENDING_INDEX].Displayed);
        Assert.IsTrue(Rows[APPROVED_INDEX].Displayed);
        Assert.IsTrue(Rows[REJECTED_INDEX].Displayed);

        // Act 3: Search with something different
        Search.Clear();
        Search.SendKeys("승인 대기 중");

        // Assert
        Assert.AreEqual(1, Rows.Count);
        Assert.AreNotEqual(EMPTY_TEXT, Rows[0].Text);
        Assert.AreEqual("승인 대기 중", Status(0).Text);
    }

    private async Task<int> CreateReservation(int seatNumber, string name = "alice")
    {
        var lockResult = await _mediator.Send(new LockSeatCommand
        {
            IpAddress = "-",
            SeatNumber = seatNumber,
        });

        var result = await _mediator.Send(new ReserveSeatCommand
        {
            Email = "alice@example.com",
            Name = name,
            IpAddress = "-",
            PreferredLanguage = "English",
            SeatKey = lockResult.Value.SeatKey,
            SeatNumber = seatNumber,
        });

        return result.Value;
    }
}
