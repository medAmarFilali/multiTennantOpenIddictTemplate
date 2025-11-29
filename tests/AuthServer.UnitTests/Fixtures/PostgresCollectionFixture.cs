namespace AuthServer.UnitTests.Fixtures;

/// <summary>
/// Collection definition for sharing PostgresFixture across multiple test classes.
/// This allows multiple test classes to share the same database container instance.
/// </summary>
[CollectionDefinition("Postgres collection")]
public class PostgresCollectionFixture : ICollectionFixture<PostgresFixture>
{
    // This class has no code, and is never created.
    // Its purpose is simply to be the place to apply [CollectionDefinition]
    // and the ICollectionFixture<> interface.
}
