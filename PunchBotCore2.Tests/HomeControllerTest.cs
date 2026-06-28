using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Controllers;
using PunchBotCore2.Tests.Mocks;

namespace PunchBotCore2.Tests;

[TestClass]
public sealed class HomeControllerTest
{
    private readonly HomeController _controller = new(new TestPunchContextFactory());

    [TestMethod]
    public async Task Index_WorksWithInitialDatabase()
    {
        var result = await _controller.Index();
        Assert.IsInstanceOfType<ViewResult>(result);
    }
}
