using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

public class CustomWebApplicationFactorySelenium : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseTestServer();
        });

        return base.CreateHost(builder);
    }
}
