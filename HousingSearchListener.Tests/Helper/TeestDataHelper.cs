using Nest;

namespace HousingSearchListener.Tests.Helper
{
    public static class TestDataHelper
    {
        public static string[] Alphabet = { "aa", "bb", "cc", "dd", "ee", "vv", "ww", "xx", "yy", "zz" };

        public static void InsertPersonInEs(IElasticClient elasticClient)
        {
            elasticClient?.Indices.Delete("persons");

            elasticClient?.Indices.Create("persons", s =>
                s.Map(x => x.AutoMap()
                    .Properties(prop =>
                        prop.Keyword(field => field.Name("surname"))
                            .Keyword(field => field.Name("firstname")))));
        }
    }
}
