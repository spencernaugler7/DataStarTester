using DataStarTester;
using StarFederation.Datastar.DependencyInjection;
using StarFederation.Datastar.ModelBinding;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDatastar();
builder.Services.AddDatastarMvc();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

MethodInfo[] endpointRegisterMethods = Assembly.GetEntryAssembly()
    .GetTypes()
    .SelectMany(t => t.GetMethods())
    .Where(w => w.GetCustomAttributes(typeof(RegisterEndpointAttribute), false).Length > 0)
    .ToArray();

foreach(var method in endpointRegisterMethods)
{
    method.Invoke(null, [app]);
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();