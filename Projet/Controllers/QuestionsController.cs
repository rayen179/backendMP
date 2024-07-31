using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Projet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly DataContext _context;

        public QuestionsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            return await _context.Questions
                                 .Include(q => q.Answers)
                                 .ToListAsync();
        }

        // POST: api/questions
        [HttpPost]
        public async Task<IActionResult> SubmitAnswers([FromBody] Question question)
        {
            if (question == null)
            {
                return BadRequest("Question cannot be null.");
            }

            // Ensure each Answer has a reference to the Question
            foreach (var answer in question.Answers)
            {
                answer.Question = question;
            }

            // Add the new question to the context, including its answers
            _context.Questions.Add(question);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Handle any potential errors during the save operation
                return StatusCode(500, "An error occurred while creating the question.");
            }

            // Return the created question with a 201 status code
            return CreatedAtAction(nameof(GetQuestions), new { id = question.Id }, question);
        }



        // PUT: api/questions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] Question question)
        {
            if (id != question.Id)
            {
                return BadRequest("Question ID mismatch.");
            }

            var existingQuestion = await _context.Questions
                                                 .Include(q => q.Answers)
                                                 .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuestion == null)
            {
                return NotFound("Question not found.");
            }

            // Update question properties
            existingQuestion.Text = question.Text;
            existingQuestion.Type = question.Type;
            existingQuestion.Options = question.Options;

            // Update answers
            _context.Answers.RemoveRange(existingQuestion.Answers);
            existingQuestion.Answers = question.Answers;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id))
                {
                    return NotFound("Question not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/questions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions
                                         .Include(q => q.Answers)
                                         .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound("Question not found.");
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}
