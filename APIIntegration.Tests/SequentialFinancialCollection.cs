using APIIntegration.Tests.Helper;

namespace APIIntegration.Tests;

[CollectionDefinition("Sequential Financial Tests", DisableParallelization = false)]
public class SequentialFinancialCollection : ICollectionFixture<ApiTestSetup>
{
}