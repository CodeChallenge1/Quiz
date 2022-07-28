using QuizService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizService.Service
{
    public interface IQuizzesService
    {
        Task<IEnumerable<QuizResponseModel>> GetAsync();
        Task<QuizResponseModel> GetQuizAsync(int id);
        Task<int> TakeQuiz(int id, TakeQuizModel model);
    }
}
