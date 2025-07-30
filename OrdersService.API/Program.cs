using eCommerce.OrdersService.API.Middleware;
using eCommerce.OrdersService.BusinessLogicLayer;
using eCommerce.OrdersService.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersService.BusinessLogicLayer.Policies;
using eCommerce.OrdersService.BusinessLogicLayer.PolicyContracts;
using eCommerce.OrdersService.DataAccessLayer;
using Polly;
using Polly.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Add BLLayer and DAL services.
builder.Services.AddBusinessLogicLayer(builder.Configuration);
builder.Services.AddDataAccessLayer(builder.Configuration);

builder.Services.AddControllers();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CORS
builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(builder =>
  {
    builder.WithOrigins("http://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader();
  });
});

builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();

builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

//HTTP Clients
builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
  client.BaseAddress = new Uri($"http://{builder.Configuration["USERS_MICROSERVICE_NAME"]}:{builder.Configuration["USERS_MICROSERVICE_PORT"]}");
}).AddPolicyHandler(
    builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy()
  )
  .AddPolicyHandler(
      builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCircuitBreakerPolicy()
    );

builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
  string host = builder.Configuration["PRODUCTS_MICROSERVICE_NAME"]!;
  string port = builder.Configuration["PRODUCTS_MICROSERVICE_PORT"]!;
  client.BaseAddress = new Uri($"http://{host}:{port}");

}).AddResilienceHandler("Products.FallbackPolicy", (pipelineBuilder, context) =>
{
  IProductsMicroservicePolicies policies = context.ServiceProvider.GetRequiredService<IProductsMicroservicePolicies>();

  ResiliencePipeline<HttpResponseMessage>? fallbackPipeline = policies.GetFallbackPolicy();

  pipelineBuilder.AddPipeline(fallbackPipeline);
});


var app = builder.Build();
app.UseExceptionHandlingMiddleware();
app.UseRouting();

//CORS
app.UseCors();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Auth
if(app.Environment.EnvironmentName != "Development")
{
  app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

//Endpoints
app.MapControllers();

app.Run();
