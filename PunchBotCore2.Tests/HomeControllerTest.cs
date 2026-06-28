using Microsoft.AspNetCore.Mvc;
using PunchBotCore2.Controllers;
using PunchBotCore2.Models;
using PunchBotCore2.Tests.Mocks;

namespace PunchBotCore2.Tests;

[TestClass]
public sealed class HomeControllerTest
{

    [TestMethod]
    public async Task Index_WorksWithInitialDatabase()
    {
        HomeController controller = new(new TestPunchContextFactory());
        ActionResult result = await controller.Index();
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IndexData>(viewResult.Model);
        IndexData model = (IndexData)viewResult.Model;
        Assert.AreEqual(model.DaySum, TimeSpan.Zero);
        Assert.AreEqual(model.WeekSum, TimeSpan.Zero);
        Assert.AreEqual(model.DayBreakSum, TimeSpan.Zero);
        Assert.IsNull(model.LastEntry);
        Assert.AreEqual(model.RemainingTime, TimeSpan.FromHours(7));
    }
}
