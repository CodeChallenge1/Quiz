using QuizService.Model;
using QuizService.Model.Domain;
using QuizService.Repository;
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

        public async Task<QuizResponseModel> GetQuizAsync(int id)
        {
            var quiz = await _quizRepository.GetAsync(id);

            if (quiz == null)
                return await Task.FromResult<QuizResponseModel>(null);

            var questions = await _quizRepository.GetQuestionsAsync(id);
            var answers = await _quizRepository.GetAnswersAsync(id);
            return new QuizResponseModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Questions = questions.Select(question => new QuizResponseModel.QuestionItem
                {
                    Id = question.Id,
                    Text = question.Text,
                    Answers = answers.ContainsKey(question.Id)
                        ? answers[question.Id].Select(answer => new QuizResponseModel.AnswerItem
                        {
                            Id = answer.Id,
                            Text = answer.Text
                        })
                        : new QuizResponseModel.AnswerItem[0],
                    CorrectAnswerId = question.CorrectAnswerId
                }),
                Links = new Dictionary<string, string>
            {
                {"self", $"/api/quizzes/{id}"},
                {"questions", $"/api/quizzes/{id}/questions"}
            }
            };
        }

        public async Task<int> TakeQuiz(int id, TakeQuizModel model)
        {
            var quiz = await GetQuizAsync(id);
            return 
                model.Answers.Where(answer => 
                quiz.Questions.Any(q => q.Id == answer.QuestionId &&
                q.Answers.First(a => a.Id == q.CorrectAnswerId).Text == answer.Answer))
                .Count();
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
