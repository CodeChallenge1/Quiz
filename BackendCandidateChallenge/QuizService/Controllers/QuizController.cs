using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using QuizService.Model.Domain;
using QuizService.Service;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace QuizService.Controllers;

[Route("api/quizzes")]
public class QuizController : Controller
{
    //TODO Remove Data access logic from controller methods bellow (e.g. move into Repository)
    //TODO Remove business logic from controller methods bellow (e.g. move into Service)
    //TODO Consider making controller methods async
    private readonly IDbConnection _connection;
    private readonly IQuizzesService _quizesService;
    public QuizController(IDbConnection connection, IQuizzesService quizesService)
    {
        _connection = connection;
        _quizesService = quizesService;
    }

    // GET api/quizzes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizResponseModel>>> GetAsync()
    {
        return Ok(await _quizesService.GetAsync());
    }

    // GET api/quizzes/5
    [HttpGet("{id}")]
    public async Task<object> Get(int id)
    {
        var quiz = await _quizesService.GetQuizAsync(id);
        return quiz == null ? NotFound() : quiz;
    }

    // POST api/quizzes
    [HttpPost]
    public IActionResult Post([FromBody]QuizCreateModel value)
    {
        var sql = $"INSERT INTO Quiz (Title) VALUES('{value.Title}'); SELECT LAST_INSERT_ROWID();";
        var id = _connection.ExecuteScalar(sql);
        return Created($"/api/quizzes/{id}", id);
    }

    // PUT api/quizzes/5
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody]QuizUpdateModel value)
    {
        const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
        int rowsUpdated = _connection.Execute(sql, new {Id = id, Title = value.Title});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        const string sql = "DELETE FROM Quiz WHERE Id = @Id";
        int rowsDeleted = _connection.Execute(sql, new {Id = id});
        if (rowsDeleted == 0)
            return NotFound();
        return NoContent();
    }

    // POST api/quizzes/5/questions
    [HttpPost]
    [Route("{id}/questions")]
    public IActionResult PostQuestion(int id, [FromBody]QuestionCreateModel value)
    {
        var quiz = GetQuizById(id);
        if (quiz == null)
            return NotFound();
        const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";
        var questionId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuizId = id});
        return Created($"/api/quizzes/{id}/questions/{questionId}", questionId);
    }

    // PUT api/quizzes/5/questions/6
    [HttpPut("{id}/questions/{qid}")]
    public IActionResult PutQuestion(int id, int qid, [FromBody]QuestionUpdateModel value)
    {
        const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
        int rowsUpdated = _connection.Execute(sql, new {QuestionId = qid, Text = value.Text, CorrectAnswerId = value.CorrectAnswerId});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6
    [HttpDelete]
    [Route("{id}/questions/{qid}")]
    public IActionResult DeleteQuestion(int id, int qid)
    {
        const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
        _connection.ExecuteScalar(sql, new {QuestionId = qid});
        return NoContent();
    }

    // POST api/quizzes/5/questions/6/answers
    [HttpPost]
    [Route("{id}/questions/{qid}/answers")]
    public IActionResult PostAnswer(int id, int qid, [FromBody]AnswerCreateModel value)
    {
        const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
        var answerId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuestionId = qid});
        return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", answerId);
    }

    // PUT api/quizzes/5/questions/6/answers/7
    [HttpPut("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult PutAnswer(int id, int qid, int aid, [FromBody]AnswerUpdateModel value)
    {
        const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
        int rowsUpdated = _connection.Execute(sql, new {AnswerId = qid, Text = value.Text});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6/answers/7
    [HttpDelete]
    [Route("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult DeleteAnswer(int id, int qid, int aid)
    {
        const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
        _connection.ExecuteScalar(sql, new {AnswerId = aid});
        return NoContent();
    }

    //POST api/quizzes/5/take
   [HttpPost]
   [Route("{id}/take")]
    public async Task<ActionResult<int>> TakeQuiz(int id, [FromBody] TakeQuizModel value)
    {
        return await _quizesService.TakeQuiz(id, value);
    }

    private Quiz GetQuizById(int id)
    {
        const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
        return _connection.QuerySingleOrDefault<Quiz>(quizSql, new { Id = id });
    }
}