using Dapper;
using QuizService.Model.Domain;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuizService.Repository
{
    // TODO Ideally this file would go to separate data access project (e.g. Quiz.Database or Quiz.DAL)
    // TODO Consider not using plain sql (e.g. use EF core)
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

        public async Task<Quiz> GetAsync(int id)
        {
            const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
            return await _connection.QuerySingleOrDefaultAsync<Quiz>(quizSql, new { Id = id });
        }

        public async Task<IEnumerable<Question>> GetQuestionsAsync(int quizId)
        {
            const string questionsSql = "SELECT * FROM Question WHERE QuizId = @QuizId;";
            return await _connection.QueryAsync<Question>(questionsSql, new { QuizId = quizId });
        }

        public async Task<Dictionary<int, IList<Answer>>> GetAnswersAsync(int quizId)
        {
            //TODO Consider using EF core to simplify this query
            const string answersSql = "SELECT a.Id, a.Text, a.QuestionId FROM Answer a INNER JOIN Question q ON a.QuestionId = q.Id WHERE q.QuizId = @QuizId;";
            return (await _connection.QueryAsync<Answer>(answersSql, new { QuizId = quizId }))
                .Aggregate(new Dictionary<int, IList<Answer>>(), (dict, answer) => {
                    if (!dict.ContainsKey(answer.QuestionId))
                        dict.Add(answer.QuestionId, new List<Answer>());
                    dict[answer.QuestionId].Add(answer);
                    return dict;
                });
        }
    }
}
