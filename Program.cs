using Microsoft.EntityFrameworkCore;
using PunchBotCore2.Data;

namespace PunchBotCore;

public class Program
{
    private const string dbFilename = "times.db";

    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton(new LiteDB.LiteDatabase(dbFilename));
        builder.Services.AddDbContextFactory<PunchContext>(
            options => options.UseSqlite(builder.Configuration.GetConnectionString("PunchContextSQLite")));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }
        
        using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            PunchContext context = services.GetRequiredService<PunchContext>();
            context.Database.EnsureCreated();
            LiteDB.LiteDatabase liteDB = services.GetRequiredService<LiteDB.LiteDatabase>();
            await context.Migrate(liteDB);
        }

        // app.UseHttpsRedirection();

        app.UseRouting();
        app.UsePathBase(new PathString("/punch"));
        app.UseStaticFiles();

        // app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
