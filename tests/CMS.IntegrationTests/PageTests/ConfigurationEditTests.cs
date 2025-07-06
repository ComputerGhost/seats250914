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

        // Assert
        var alerts = _driver.FindElements(By.ClassName("alert-success"));
        Assert.AreEqual(1, alerts.Count);
    }

    [TestMethod]
    public async Task ConfigForm_WhenSubmitted_SavesConfig()
    {
        // Arrange
        var targetConfig = new SaveConfigurationCommand
        {
            ForceCloseReservations = true,
            ForceOpenReservations = false,
            GracePeriodSeconds = 2,
            MaxSeatsPerIPAddress = 100,
            MaxSeatsPerPerson = 100,
            MaxSecondsToConfirmSeat = 600,
            ScheduledCloseDateTime = DateTimeOffset.Now.AddMonths(1).AddMinutes(1),
            ScheduledCloseTimeZone = "Pacific Standard Time",
            ScheduledOpenDateTime = DateTimeOffset.Now.AddDays(-1).AddMinutes(-1),
            ScheduledOpenTimeZone = "Pacific Standard Time",
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
        _driver.Navigate().GoToUrl(ConfigurationUrl);
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == ConfigurationPath);

        // Act 2: Populate form data and submit
        _driver.SetDateField(ScheduledOpenDate, targetConfig.ScheduledOpenDateTime);
        ScheduledOpenTime.SendKeys(targetConfig.ScheduledOpenDateTime.ToString("HH:mm:ss"));
        ScheduledOpenTimeZone.SelectByValue(targetConfig.ScheduledOpenTimeZone);
        _driver.SetDateField(ScheduledCloseDate, targetConfig.ScheduledCloseDateTime);
        ScheduledCloseTime.SendKeys(targetConfig.ScheduledCloseDateTime.ToString("HH:mm:ss"));
        ScheduledCloseTimeZone.SelectByValue(targetConfig.ScheduledCloseTimeZone);
        ScheduleOverrides[2].Click();
        MaxSeatsPerPerson.SendKeys(targetConfig.MaxSeatsPerPerson.ToString());
        MaxSeatsPerIPAddress.SendKeys(targetConfig.MaxSeatsPerIPAddress.ToString());
        MaxSecondsToConfirmSeat.SendKeys(targetConfig.MaxSecondsToConfirmSeat.ToString());
        GracePeriodSeconds.SendKeys(targetConfig.GracePeriodSeconds.ToString());
        _driver.ScrollTo(SubmitButton);
        SubmitButton.Click();

        // Assert save button is displayed
        _driver.WaitUntil(d => d.FindElements(By.ClassName("alert-success")).Any());
        Assert.AreEqual(1, _driver.FindElements(By.ClassName("alert-success")).Count);

        // Act 3: Reload the page
        _driver.Navigate().GoToUrl(ConfigurationUrl);
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == ConfigurationPath);

        // Assert form is valid
        Assert.AreEqual(targetConfig.ScheduledOpenDateTime.ToString("yyyy-MM-dd"), ScheduledOpenDate.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledOpenDateTime.ToString("HH:mm:ss"), ScheduledOpenTime.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledOpenTimeZone, ScheduledOpenTimeZone.SelectedOption.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledCloseDateTime.ToString("yyyy-MM-dd"), ScheduledCloseDate.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledCloseDateTime.ToString("HH:mm:ss"), ScheduledCloseTime.GetAttribute("value"));
        Assert.AreEqual(targetConfig.ScheduledCloseTimeZone, ScheduledCloseTimeZone.SelectedOption.GetAttribute("value"));
        Assert.AreEqual("checked", ScheduleOverrides[2].GetAttribute("checked"));
        Assert.AreEqual(targetConfig.MaxSeatsPerPerson.ToString(), MaxSeatsPerPerson.Text);
        Assert.AreEqual(targetConfig.MaxSeatsPerIPAddress.ToString(), MaxSeatsPerIPAddress.Text);
        Assert.AreEqual(targetConfig.MaxSecondsToConfirmSeat.ToString(), MaxSecondsToConfirmSeat.Text);
        Assert.AreEqual(targetConfig.GracePeriodSeconds.ToString(), GracePeriodSeconds.Text);
    }
}
