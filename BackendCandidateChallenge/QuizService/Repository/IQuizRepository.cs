
using QuizService.Model;
using QuizService.Model.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizService.Repository
{
    public interface IQuizRepository
    {
        Task<IEnumerable<Quiz>> GetAsync();
    }
}
