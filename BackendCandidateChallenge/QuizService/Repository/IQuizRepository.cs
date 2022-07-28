
using QuizService.Model;
using QuizService.Model.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizService.Repository
{
    public interface IQuizRepository
    {
        Task<IEnumerable<Quiz>> GetAsync();
        Task<Quiz> GetAsync(int id);
        Task<IEnumerable<Question>> GetQuestionsAsync(int quizId);
        Task<Dictionary<int, IList<Answer>>> GetAnswersAsync(int quizId);
    }
}
