using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.UI.Configs;
using WCCG.PAS.Referrals.UI.Configs.OptionValidators;
using WCCG.PAS.Referrals.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddOptions<CosmosConfig>().Bind(builder.Configuration.GetSection(CosmosConfig.SectionName));
builder.Services.AddSingleton<IValidateOptions<CosmosConfig>, ValidateCosmosConfigOptions>();

builder.Services.AddApplicationInsights(builder.Environment.IsDevelopment(), builder.Configuration);

builder.Services.AddCosmosClient();
builder.Services.AddCosmosRepositories();
builder.Services.AddValidators();
builder.Services.AddServices();

var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
