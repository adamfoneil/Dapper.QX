using Dapper.QX;
using Dapper.QX.Attributes;

namespace Testing.Queries
{
    public class PhraseQueryTest : Query<string>
    {
        public PhraseQueryTest() : base("SELECT * FROM [Employee] {where}")
        {
        }

        [Phrase("FirstName", "LastName", "Email", "Notes")]
        public string Search { get; set; }
    }

    public class PhraseQueryTestCe : Query<string>
    {
        public PhraseQueryTestCe() : base("SELECT * FROM [Employee] {where}")
        {
        }

        [Phrase("FirstName", "LastName", "Email", "Notes", ConcatenateWildcards = false)]
        public string Search { get; set; }
    }
}
