using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using QuizService.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace QuizService.Tests;

public class QuizzesControllerTest
{
    const string QuizApiEndPoint = "/api/quizzes/";

    [Fact]
    public async Task PostNewQuizAddsQuiz()
    {
        var quiz = new QuizCreateModel("Test title");
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(quiz));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),
                content);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }
    }

    [Fact]
    public async Task AQuizExistGetReturnsQuiz()
    {
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            const long quizId = 1;
            var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var quiz = JsonConvert.DeserializeObject<QuizResponseModel>(await response.Content.ReadAsStringAsync());
            Assert.Equal(quizId, quiz.Id);
            Assert.Equal("My first quiz", quiz.Title);
        }
    }

    [Fact]
    public async Task AQuizDoesNotExistGetFails()
    {
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            const long quizId = 999;
            var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
        
    public async Task AQuizDoesNotExists_WhenPostingAQuestion_ReturnsNotFound()
    {
        const string QuizApiEndPoint = "/api/quizzes/999/questions";

        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            const long quizId = 999;
            var question = new QuestionCreateModel("The answer to everything is what?");
            var content = new StringContent(JsonConvert.SerializeObject(question));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),content);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Theory]
    [InlineData("Red", "Yellow", 2)]
    [InlineData("Red", "Blue", 1)]
    public async Task GivenQuizExists_WhenQuizIsTaken_NumberOfQuestionsAnsweredShouldBeAsExpected(string firstAnswer, string secondAnswer, int expectedNumberOfCorrectAnswers)
    {
        var quizModel = new QuizCreateModel("Fruit Color Guessing");

        const string question1 = "What color is a strawberry ?";
        const string correctAnswer1 = "Red";
        const string question2 = "What color is a banana ?";
        const string correctAnswer2 = "Yellow";

        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            var (quizId, questionId1, questionId2) = await SetupQuizData(quizModel, 
                question1, 
                correctAnswer1,
                question2, 
                correctAnswer2,
                client, 
                testHost.BaseAddress);

            //take the quiz
            var takeQuizContent = HttpClientHelpers.CreateStringContent(new TakeQuizModel()
            {
                Answers = new List<TakeQuizQuestionModel>()
                {
                    new TakeQuizQuestionModel() { QuestionId = questionId1, Answer = firstAnswer },
                    new TakeQuizQuestionModel() { QuestionId =  questionId2, Answer = secondAnswer }
                }
            });

            var takeQuizResponse = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/take"), takeQuizContent);
            var takeQuizSuccessCount = JsonConvert.DeserializeObject<int>(await takeQuizResponse.Content.ReadAsStringAsync());
            Assert.Equal(expectedNumberOfCorrectAnswers, takeQuizSuccessCount);
        }
    }

    private async Task<(int quizId, int questionId1, int questionId2)> SetupQuizData(QuizCreateModel quizModel, 
        string question1, 
        string correctAnswer1,
        string question2,
        string correctAnswer2, 
        HttpClient client,
        Uri baseAddress)
    {
        var content = HttpClientHelpers.CreateStringContent(quizModel);

        var response = await client.PostAsync(new Uri(baseAddress, $"{QuizApiEndPoint}"), content);
        var quizId = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.NotEqual(0, quizId);

        //question 1 
        var contentQuestion1 = HttpClientHelpers.CreateStringContent(new QuestionCreateModel(question1));
        var questionResponse1 = await client.PostAsync(new Uri(baseAddress,
            $"{QuizApiEndPoint}{quizId}/questions"), contentQuestion1);
        var questionId1 = JsonConvert.DeserializeObject<int>(await questionResponse1.Content.ReadAsStringAsync());
        Assert.NotEqual(0, questionId1);

        //answer for question 1
        var contentAnswer1 = HttpClientHelpers.CreateStringContent(new AnswerCreateModel(correctAnswer1));
        var answerResponse1 = await client.PostAsync(new Uri(baseAddress,
            $"{QuizApiEndPoint}{quizId}/questions/{questionId1}/answers"), contentAnswer1);
        var answerId1 = JsonConvert.DeserializeObject<int>(await answerResponse1.Content.ReadAsStringAsync());
        Assert.NotEqual(0, answerId1);

        //update question 1 with answer 
        var contentPutQuestion1 = HttpClientHelpers.CreateStringContent(new QuestionUpdateModel() { Text = question1, CorrectAnswerId = answerId1 });
        await client.PutAsync(new Uri(baseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questionId1}"), contentPutQuestion1);

        //question 2
        var contentQuestion2 = HttpClientHelpers.CreateStringContent(new QuestionCreateModel(question2));
        var questionResponse2 = await client.PostAsync(new Uri(baseAddress,
            $"{QuizApiEndPoint}{quizId}/questions"), contentQuestion2);
        var questionId2 = JsonConvert.DeserializeObject<int>(await questionResponse2.Content.ReadAsStringAsync());
        Assert.NotEqual(0, questionId2);

        //answer for question 2
        var contentAnswer2 = HttpClientHelpers.CreateStringContent(new AnswerCreateModel(correctAnswer2));
        var answerResponse2 = await client.PostAsync(new Uri(baseAddress,
            $"{QuizApiEndPoint}{quizId}/questions/{questionId2}/answers"), contentAnswer2);
        var answerId2 = JsonConvert.DeserializeObject<int>(await answerResponse2.Content.ReadAsStringAsync());
        Assert.NotEqual(0, answerId2);

        //update question 2 with answer
        var contentPutQuestion2 = HttpClientHelpers.CreateStringContent(
            new QuestionUpdateModel() { Text = question2, CorrectAnswerId = answerId2 });
        await client.PutAsync(new Uri(baseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questionId2}"), contentPutQuestion2);

        return (quizId, questionId1, questionId2);
    }
}