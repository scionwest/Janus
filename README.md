[![Downloads](https://img.shields.io/nuget/dt/Janus.svg)](https://www.nuget.org/packages/Janus/)

# Janus Database Seeder for .NET

## Project Description

HelloJanus is a simple database seeding API for seeding databases used with [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/). It is a NetStandard 2.0 library that can be used to provision a new database for all tests in a test-suite or for each individual test executed. Janus can be used to seed the database for your API integration tests, simplifying your API testing significantly.

## Download & Install

**NuGet Package [Janus](https://www.nuget.org/packages/Janus)**

You can install it using the Visual Studio Package Manager
> Install-Package Janus

Or via the dotnet core CLI
> dotnet add package Janus

Minimum Requirements is .Net Standard 2.0.

## Usage
### Setting the stage for our example

In order to utilize the Jane API, you need to have a DbContext with some Entities and a API Controller created.

```c#
public class UserEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }

    public List<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}
public class TaskEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public DateTime DueDate { get; set; }
}
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
}
```

With the data model out of the way, we'll assume next that we have a Controller representing an API endpoint that will provide us our User entities.

```c#
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    public UsersController(AppDbContext context) => Context = context;

    public AppDbContext Context { get; }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var entities = await this.Context
            .Users
            .Include(user => user.Tasks)
            .ToArrayAsync();
        return base.Ok(entities);
    }
}
```

### Provisioning a database
Our first Integration test will provision the database and make an Http Request to our API. The test will make sure we received a http status code of 200 OK.

```c#
[TestClass]
public class CreateDbContextTests
{
    private ApiIntegrationTestFactory<Startup> testFactory;

    [TestInitialize]
    public void Initialize()
        => this.testFactory = new ApiIntegrationTestFactory<Startup>();

    [TestCleanup]
    public void Cleanup() => this.testFactory.Dispose();

    [TestMethod]
    public async Task GetUsers_ReturnsOkStatusCode()
    {
        // Arrange - Sets up and creates the database.
        this.testFactory.WithDataContext<AppDbContext>("Default");
        var client = this.testFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("api/users");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task PostUsers_ReturnsNotFound()
    {
        // Arrange - Sets up and creates the database.
        this.testFactory.WithDataContext<AppDbContext>("Default");
        var client = this.testFactory.CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsync("api/users");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

This integration test will provision two databases using the default naming convention. Assuming that our `Default` connection string pointed to a database called `UsersDb` then our tests will provision the following two databases.

- Test-GetUsers_ReturnsOkStatusCode_{timestamp}
- Test-PostUsers_ReturnsNotFound_{timestamp}

The {timestamp} value is replaced with an actual time stamp for when the database was provisioned. When the `Cleanup` method is called the databases will be destroyed.

### Seeding the database
We can seed the database inline, within each test. This is the quickest and easiest approach.

```
[TestMethod]
public async Task GetUsers_ReturnsFakeUsers()
{
    // Arrange
    var users = new UserEntity[]
    {
        new UserEntity
        {
            Address = this.dataFaker.Address.FullAddress(),
            Email = this.dataFaker.Internet.Email(),
            Username = this.dataFaker.Internet.UserName()
        },
        new UserEntity
        {
            Address = this.dataFaker.Address.FullAddress(),
            Email = this.dataFaker.Internet.Email(),
            Username = this.dataFaker.Internet.UserName()
        },
    };

    this.testFactory.WithDataContext<AppDbContext>("Default")
        .WithSeedData(context =>
        {
            context.Users.AddRange(users);
            context.SaveChanges();
        });

    var client = this.testFactory.CreateClient();

    // Act
    HttpResponseMessage response = await client.GetAsync("api/users");
    string responseBody = await response.Content.ReadAsStringAsync();
    UserEntity[] responseData = JsonConvert.DeserializeObject<UserEntity[]>(responseBody);


    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    Assert.AreEqual(users.Length, responseData.Length);
}
```

This uses the [Bogus](https://github.com/bchavez/Bogus) framework to produce fake data for each user, then the test inserts them into the `DbContext` for saving. When the Integration Test runs, we can now verify that our API is returning the data we just inserted into the database.

### Entity Seeder

There are use-cases were you have more complex relationships though. Building those out inline within each test can be cumbersome. You can use the `EntitySeeder` API to make your seeding re-usable across tests and move the complex relationship building to live outside of your tests.

We'll first build two different `EntitySeeder` classes. One to seed our users and one for seeding the Task entity each User has a collection of.

```c#
public class UserEntitySeeder : EntitySeeder<UserEntity>
{
    protected override bool MapEntities(UserEntity[] seededEntities, ISeedReader seedReader) => true;

    protected override IList<UserEntity> Seed(SeedOptions options)
    {
        IList<UserEntity> users = new Faker().Make(10, count =>
        {
            UserEntity user = new Faker<UserEntity>()
            .RuleFor(entity => entity.Address, faker => faker.Address.FullAddress())
            .RuleFor(entity => entity.Email, faker => faker.Internet.Email())
            .RuleFor(entity => entity.Id, Guid.NewGuid())
            .RuleFor(entity => entity.Username, faker => faker.Internet.UserName())
            .Ignore(entity => entity.Tasks);

            return user;
        });

        return users;
    }
}

public class TaskEntitySeeder : EntitySeeder<TaskEntity>
{
    protected override bool MapEntities(TaskEntity[] seededEntities, ISeedReader seedReader)
    {
        UserEntity[] users = seedReader.GetDataForEntity<UserEntity>();
        var faker = new Faker();

        foreach(TaskEntity task in seededEntities)
        {
            UserEntity user = faker.PickRandom(users);
            user.Tasks.Add(task);
            task.UserId = user.Id;
        }

        return true;
    }

    protected override IList<TaskEntity> Seed(SeedOptions options)
    {
        IList<TaskEntity> tasks = new Faker().Make(100, count =>
        {
            TaskEntity task = new Faker<TaskEntity>()
                .RuleFor(entity => entity.DueDate, faker => faker.Date.Soon())
                .RuleFor(entity => entity.Id, Guid.NewGuid())
                .RuleFor(entity => entity.Title, faker => faker.Random.String())
                .Ignore(entity => entity.UserId);

            return task;
        });

        return tasks;
    }
}
```

You can see in the `TaskEntitySeeder` that we ask the `ISeedReader` to provide us all of the seeded Users so we can build our relationships. This allows Entity Framework to save the seeded data easily, relationships and all. You can see from our example code that the `TaskEntitySeeder` will insert 100 records while the `UserEntitySeeder` will insert 10 records.

Your integration tests don't change very much. Instead of passing a callback to the `WithSeedData` method, you provide a generic Type representing your seed data.

```c#
[TestMethod]
public async Task GetUsers_ReturnsSeederData()
{
    // Arrange
    this.testFactory.WithDataContext<AppDbContext>("Default")
        .WithSeedData<UserEntitySeeder>()
        .WithSeedData<TaskEntitySeeder>();

    var client = testFactory.CreateClient();
    IEntitySeeder userSeeder = testFactory.GetDataContextSeedData<AppDbContext, UserEntitySeeder>();

    // Act
    HttpResponseMessage response = await client.GetAsync("api/users");
    string responseBody = await response.Content.ReadAsStringAsync();
    UserEntity[] responseData = JsonConvert.DeserializeObject<UserEntity[]>(responseBody);


    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    Assert.AreEqual(userSeeder.GetSeedData().Length, responseData.Length);
    Assert.IsTrue(responseData.Any(user => user.Tasks.Count > 0));
}
```

We can fetch the data we seeded, after the database has been created, and use that to verify that our API is still sending us back the right data. 

## Configuring the Factory
### Root Content Path
The factory is configured by default to use your solutions root directory as the source for where it can find your content, such as appsettings.json. If you have a solution structure that doesn't directly expose the root content to your solution file then you need to configure the Factory for it.

```c#
this.testFactory = new ApiIntegrationTestFactory<Startup>()
    .SetSolutionRelativeContentRoot("samples\\ApiSample\\src");
```

### Database key within connection strings
Each database provider uses a different key in their connection strings to represent the database name. The table below gives some examples

| Provider | Database Key | Example |
|----------|-|-|
| Sql Server | Initial Catalog | Server=localhost;Initial Catalog=MyDatabase;|
| MySql | Database | Server=localhost;Database=MyDatabase;|
| Sqlite | Data Source | Data Source=MyDatabase; |

Janus defaults to looking for `Initial Catalog` for Sql Server connection strings. However, if you use a different provider such as Sqlite or MySql, then you need to specify the Database Key used in the providers connection string for determining the name of the database.

You can specify that key when you instantiate the `ApiIntegrationTestFactory`

```
// Sqlite
var factory = new ApiIntegrationTestFactory<Startup>("Data Source");

// MySql
var factory = new ApiIntegrationTestFactory<Startup>("Database");
```

You can use any provider as long as they support a connection string that is simi-colon delimited with each element being a key/value pair separated by an `=` equals sign as shown in the examples in the above table.

### Finding the Connection String
In order for the factory to find the connection string, you have to tell it where it lives within the `IConfiguration` instance. By default, Janus will look inside of the `ConnectionStrings:` Section, meaning you only have to provide the sub-section.

If you have the following configuration in `appsettings.json`, then this is how you would tell Janus where to find your connection string.

```json
{
    "ConnectionStrings": {
        "Default": "Server=localhost;Database=MyDatabase;"
    }
}
```
```c#
testFactory.WithDataContext<AppDbContext>("Default")
    .WithSeedData<UserEntitySeeder>()
    .WithSeedData<TaskEntitySeeder>();
```

If you do not use the standard convention, storing your connection strings differently, then you need to provide Janus with the fully qualified path.

```json
{
    "DataStores":{
        "UserContext": {
            "ConnectionString": "Server=localhost;Database=MyDatabase;"
        }
    }
}
```
```c#
testFactory.WithDataContext<AppDbContext>("DataStores:UserContext:ConnectionString")
    .WithSeedData<UserEntitySeeder>()
    .WithSeedData<TaskEntitySeeder>();
```

### Retaining databases
If you have an integration test that fails and you want to look at the database to understand what the data looked like that caused the failure, you can tell Janus to retain your database.

```c#
this.testFactory.WithDataContext<AppDbContext>("Default")
    .RetainDatabase()
    .WithSeedData<UserEntitySeeder>();
```

## Extending ApiIntegrationTestFactory
The factory can be extended to fit the needs of the developers in most cases.

### CreateDatabase
You can override the `CreateDatabase` method and do work before and after your database is provisioned.

> This is not the place to do data seeding/querying.

```c#
protected virtual void CreateDatabase(TestDatabaseConfiguration configuration, DbContext context)
{
    context.Database.EnsureCreated();
}
```

### SeedDatabase
The `SeedDatabase` method is responsible for seeding the created database. This will get called after a successful call to `CreateDatabase`.

The Janus implementation sets up the `IEntitySeeder` and `IDataContextSeeder` implementations to perform database seeding. It will also invoke the callback delegate provided when using the inline test seeding API.

If you override and replace this functionality, you will want to make sure you honor both the `WithSeedData<T>()` and `WithSeedData(Action<TContext>)` API needs. Janus chooses to seed the database with the data seeders first, followed by the inline delegate callbacks. This allows each integration test to override a seeders data, deleting it, mutating it or adding to it as needed per test.

```c#
protected virtual void SeedDatabase(TestServer server, TestDatabaseConfiguration databaseConfiguration, DbContext dbContext)
{
    // Resolve a context seeder and seed the database with the individual seeders
    IDataContextSeeder contextSeeder = server.Host.Services.GetRequiredService<IDataContextSeeder>();
    IEntitySeeder[] entitySeeders = databaseConfiguration.SeedBuilder.GetEntitySeeders();
    contextSeeder.SeedDataContext(dbContext, entitySeeders);

    // Always run the callback seeder last so that individual tests have the ability to replace data
    // inserted via seeders.
    databaseConfiguration.DatabaseSeeder?.DynamicInvoke(dbContext);
}
```

### Database Name for Test
You can override the `CreateDatabaseNameForTest` method and use your own naming convention for your databases.

The default implementation looks like the following.

```c#
protected virtual string CreateDatabaseNameForTest(string initialDatabaseName)
{
    string timeStamp = DateTime.Now.ToString("HHmmss.fff");
    string newDbName = $"Tests-{initialDatabaseName}-{timeStamp}";
    return newDbName;
}
```