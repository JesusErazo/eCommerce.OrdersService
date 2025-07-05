using eCommerce.OrdersService.API.Middleware;
using eCommerce.OrdersService.BusinessLogicLayer;
using eCommerce.OrdersService.DataAccessLayer;

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


var app = builder.Build();
app.UseExceptionHandlingMiddleware();
app.UseRouting();

//CORS
app.UseCors();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//Endpoints
app.MapControllers();

app.Run();
