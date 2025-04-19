using Microsoft.AspNetCore.Authentication;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string Role { get; set; }
}