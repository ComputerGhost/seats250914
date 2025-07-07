using Core.Application.System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;

namespace CMS.IntegrationTests.PageTests;

[LocalOnly]
[TestClass]
public class ConfigurationEditTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement ScheduledOpenDate => _driver.FindElement(By.Id("ScheduledOpenDate"));
    private IWebElement ScheduledOpenTime => _driver.FindElement(By.Id("ScheduledOpenTime"));
    private SelectElement ScheduledOpenTimeZone => new(_driver.FindElement(By.Id("ScheduledOpenTimeZone")));
    private IWebElement ScheduledCloseDate => _driver.FindElement(By.Id("ScheduledCloseDate"));
    private IWebElement ScheduledCloseTime => _driver.FindElement(By.Id("ScheduledCloseTime"));
    private SelectElement ScheduledCloseTimeZone => new(_driver.FindElement(By.Id("ScheduledCloseTimeZone")));
    private ReadOnlyCollection<IWebElement> ScheduleOverrides => _driver.FindElements(By.Id("ScheduleOverride"));
    private IWebElement MaxSeatsPerPerson => _driver.FindElement(By.Id("MaxSeatsPerPerson"));
    private IWebElement MaxSeatsPerIPAddress => _driver.FindElement(By.Id("MaxSeatsPerIPAddress"));
    private IWebElement MaxSecondsToConfirmSeat => _driver.FindElement(By.Id("MaxSecondsToConfirmSeat"));
    private IWebElement GracePeriodSeconds => _driver.FindElement(By.Id("GracePeriodSeconds"));
    private IWebElement SubmitButton => _driver.FindElement(By.ClassName("btn-primary"));

    private string ConfigurationPath => "/configuration";
    private string ConfigurationUrl => ConfigurationAccessor.Instance.TargetUrl + ConfigurationPath;

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper();
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

        // Sign in and go to config page.
        _driver.SignIn();
        _driver.Navigate().GoToUrl(ConfigurationUrl);
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == ConfigurationPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void ConfigForm_WhenSubmitted_ShowsConfirmationAlert()
    {
        // Act 1: Populate the form to minimal required values
        MaxSeatsPerPerson.SendKeys("1");
        MaxSeatsPerIPAddress.SendKeys("1");

        // Act 2: Submit
        SubmitButton.Submit();

        // Assert: Should not throw exception
        _driver.FindElement(By.ClassName("alert-success"));
    }

    [TestMethod]
    public async Task ConfigForm_WhenSubmitted_SavesConfig()
    {
        // Arrange
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        var targetCloseDateTime = DateTimeOffset.Now.AddMonths(1).AddMinutes(1);
        var targetOpenDateTime = DateTimeOffset.Now.AddDays(-1).AddMinutes(-1);
        var targetConfig = new SaveConfigurationCommand
        {
            ForceCloseReservations = true,
            ForceOpenReservations = false,
            GracePeriodSeconds = 2,
            MaxSeatsPerIPAddress = 20,
            MaxSeatsPerPerson = 10,
            MaxSecondsToConfirmSeat = 600,
            ScheduledCloseDateTime = TimeZoneInfo.ConvertTime(targetCloseDateTime, targetTimeZone),
            ScheduledCloseTimeZone = targetTimeZone.Id,
            ScheduledOpenDateTime = TimeZoneInfo.ConvertTime(targetOpenDateTime, targetTimeZone),
            ScheduledOpenTimeZone = targetTimeZone.Id,
        };

        // Act 1: Reset config and refresh
        await _mediator.Send(new SaveConfigurationCommand
        {
            ForceCloseReservations = false,
            ForceOpenReservations = false,
            GracePeriodSeconds = 0,
            MaxSeatsPerIPAddress = 0,
            MaxSeatsPerPerson = 0,
            MaxSecondsToConfirmSeat = 0,
            ScheduledCloseDateTime = DateTimeOffset.Now,
            ScheduledCloseTimeZone = "UTC",
            ScheduledOpenDateTime = DateTimeOffset.Now,
            ScheduledOpenTimeZone = "UTC",
        });
        _driver.Refresh();

        // Act 2: Populate form data and submit
        _driver.SetValue(ScheduledOpenDate, targetConfig.ScheduledOpenDateTime.ToString("yyyy-MM-dd"));
        _driver.SetValue(ScheduledOpenTime, targetConfig.ScheduledOpenDateTime.ToString("HH:mm:ss"));
        ScheduledOpenTimeZone.SelectByValue(targetConfig.ScheduledOpenTimeZone);
        _driver.SetValue(ScheduledCloseDate, targetConfig.ScheduledCloseDateTime.ToString("yyyy-MM-dd"));
        _driver.SetValue(ScheduledCloseTime, targetConfig.ScheduledCloseDateTime.ToString("HH:mm:ss"));
        ScheduledCloseTimeZone.SelectByValue(targetConfig.ScheduledCloseTimeZone);
        ScheduleOverrides[2].Click();
        _driver.SetValue(MaxSeatsPerPerson, targetConfig.MaxSeatsPerPerson.ToString());
        _driver.SetValue(MaxSeatsPerIPAddress, targetConfig.MaxSeatsPerIPAddress.ToString());
        _driver.SetValue(MaxSecondsToConfirmSeat, targetConfig.MaxSecondsToConfirmSeat.ToString());
        _driver.SetValue(GracePeriodSeconds, targetConfig.GracePeriodSeconds.ToString());
        _driver.ScrollTo(SubmitButton);
        SubmitButton.Click();

        // Assert save button is displayed
        _driver.WaitUntil(d => d.FindElements(By.ClassName("alert-success")).Any());
        Assert.AreEqual(1, _driver.FindElements(By.ClassName("alert-success")).Count);

        // Act 3: Reload the page
        _driver.Refresh();

        // Assert form is valid
        Assert.AreEqual(targetConfig.ScheduledOpenDateTime.ToString("yyyy-MM-dd"), ScheduledOpenDate.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledOpenDateTime.ToString("HH:mm:ss"), ScheduledOpenTime.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledOpenTimeZone, ScheduledOpenTimeZone.SelectedOption.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledCloseDateTime.ToString("yyyy-MM-dd"), ScheduledCloseDate.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledCloseDateTime.ToString("HH:mm:ss"), ScheduledCloseTime.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledCloseTimeZone, ScheduledCloseTimeZone.SelectedOption.GetAttribute("value"));
        Assert.IsTrue(bool.TryParse(ScheduleOverrides[2].GetAttribute("checked"), out bool value) && value);
        Assert.AreEqual(targetConfig.MaxSeatsPerPerson.ToString(), MaxSeatsPerPerson.GetAttribute("value"));
        Assert.AreEqual(targetConfig.MaxSeatsPerIPAddress.ToString(), MaxSeatsPerIPAddress.GetAttribute("value"));
        Assert.AreEqual(targetConfig.MaxSecondsToConfirmSeat.ToString(), MaxSecondsToConfirmSeat.GetAttribute("value"));
        Assert.AreEqual(targetConfig.GracePeriodSeconds.ToString(), GracePeriodSeconds.GetAttribute("value"));
    }
}
