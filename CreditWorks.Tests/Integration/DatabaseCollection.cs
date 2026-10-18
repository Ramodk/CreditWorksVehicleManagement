namespace CreditWorks.Tests.Integration;

// Tests that read or change the shared test database run one
// at a time, so a category change in one test can't affect
// assertions in another.
[CollectionDefinition("Database", DisableParallelization = true)]
public class DatabaseCollection
{
}
