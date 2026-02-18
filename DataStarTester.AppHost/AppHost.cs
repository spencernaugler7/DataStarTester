var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DataStarTester>("datastartester");

builder.Build().Run();
