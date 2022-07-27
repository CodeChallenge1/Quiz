using Dapper;
using QuizService.Model.Domain;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace QuizService.Repository
{
    // TODO: Ideally this file would go to separate data access project (e.g. Quiz.Database or Quiz.DAL)
    public class QuizRepository : IQuizRepository
    {
        private readonly IDbConnection _connection;

        public QuizRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Quiz>> GetAsync()
        {
            const string sql = "SELECT * FROM Quiz;";
            return await _connection.QueryAsync<Quiz>(sql);
        }
    }
}
