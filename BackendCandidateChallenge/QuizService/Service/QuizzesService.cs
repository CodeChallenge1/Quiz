using QuizService.Model;
using QuizService.Model.Domain;
using QuizService.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizService.Service
{
    // TODO: Ideally this file would go to separate project for business logic (e.g. Quiz.Service or Quiz.Core)
    public class QuizzesService : IQuizzesService
    {
        private readonly IQuizRepository _quizRepository;
        public QuizzesService(IQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public async Task<IEnumerable<QuizResponseModel>> GetAsync()
        {
            var quizes = await _quizRepository.GetAsync();

            return quizes.Select(quiz => MapQuizToResponseModel(quiz));
        }

        private static QuizResponseModel MapQuizToResponseModel(Quiz quiz)
        {
            return new QuizResponseModel
            {
                Id = quiz.Id,
                Title = quiz.Title
            };
        }
    }
}
