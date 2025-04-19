using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class SeleniumUITests : IClassFixture<CustomWebApplicationFactorySelenium>, IDisposable
{
    private readonly CustomWebApplicationFactorySelenium _factory;
    private readonly IWebDriver _driver;
    private readonly string _baseUrl = "http://localhost:80";

    public SeleniumUITests(CustomWebApplicationFactorySelenium factory)
    {
        _factory = new CustomWebApplicationFactorySelenium();

        // Trigger app startup
        var httpClient = _factory.CreateClient();

        EnsureAppIsRunning(httpClient);
        
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("test-type");
        options.AddArgument("start-maximized");
        options.AddArgument("disable-infobars");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--ignore-certificate-errors");

        _driver = new ChromeDriver(options);
    }

    private void EnsureAppIsRunning(HttpClient httpClient)
    {
        Thread.Sleep(1000);

        for (int i = 0; i < 10; i++)
        {
            try
            {
                var response = httpClient.GetAsync($"{_baseUrl}/").Result;
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // wait and retry
            }

            Thread.Sleep(1000);
        }

        throw new InvalidOperationException("The test server failed to start.");
    }

    [Fact]
    public void Index_DisplaysUsersList_WhenAdminIsLoggedIn()
    {
        LoginAsAdmin();

        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/Users/Index");

        // Assert
        var tableRows = _driver.FindElements(By.CssSelector("table tbody tr"));
        Assert.NotEmpty(tableRows);

        var createButton = _driver.FindElement(By.LinkText("➕ Create a user"));
        Assert.NotNull(createButton);
    }

    [Fact]
    public void Create_CreatesUser_WhenValidDataIsEntered()
    {
        LoginAsAdmin();

        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/Users/Create");

        _driver.FindElement(By.Id("FirstName")).SendKeys("Pepi");
        _driver.FindElement(By.Id("LastName")).SendKeys("Pepo");
        _driver.FindElement(By.Id("Email")).SendKeys("pp@hcm.com");
        _driver.FindElement(By.Id("JobTitle")).SendKeys("Software Developer");
        _driver.FindElement(By.Id("Salary")).SendKeys("50000");
        _driver.FindElement(By.CssSelector("select[name='Role']")).SendKeys("employee");
        _driver.FindElement(By.Id("Password")).SendKeys("password123");
        _driver.FindElement(By.CssSelector("select[name='DepartmentId']")).SendKeys("1");

        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        var userListPageUrl = "https://localhost:7072/Users/Index";
        Assert.Equal(userListPageUrl, _driver.Url);

        var userRow = _driver.FindElement(By.XPath("//table//tr[contains(., 'Pepi Pepo')]"));
        Assert.NotNull(userRow);
    }

    [Fact]
    public void Edit_UpdatesUser_WhenValidDataIsEntered()
    {
        LoginAsAdmin();

        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/Users/Edit/1");

        _driver.FindElement(By.Id("FirstName")).Clear();
        _driver.FindElement(By.Id("FirstName")).SendKeys("Updated Pepi");

        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        var userListPageUrl = $"{_baseUrl}/Users/Index";
        Assert.Equal(userListPageUrl, _driver.Url);

        var updatedUserRow = _driver.FindElement(By.XPath("//table//tr[contains(., 'Updated John')]"));
        Assert.NotNull(updatedUserRow);
    }

    [Fact]
    public void Delete_DeletesUser_WhenConfirmed()
    {
        LoginAsAdmin();

        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/Users/Delete/1");

        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Assert
        var userListPageUrl = $"{_baseUrl}/Users/Index";
        Assert.Equal(userListPageUrl, _driver.Url);

        var deletedUserRow = _driver.FindElements(By.XPath("//table//tr[contains(., 'John Doe')]"));
        Assert.Empty(deletedUserRow);
    }

    [Fact]
    private void LoginAsAdmin()
    {
        // Act
        try
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/Account/Login");
        }
        catch (WebDriverException ex)
        {
            throw new InvalidOperationException("Failed to navigate to the login page.", ex);
        }

        var usernameField = _driver.FindElement(By.Id("Input_Email"));
        var passwordField = _driver.FindElement(By.Id("Input_Password"));
        var submitButton = _driver.FindElement(By.CssSelector("button[type='submit']"));

        usernameField.SendKeys("admin@hcm.com");
        passwordField.SendKeys("AdminPassword123!");
        submitButton.Click();
    }

    public void Dispose()
    {
        _driver.Quit();
        _factory.Dispose();

    }
}
