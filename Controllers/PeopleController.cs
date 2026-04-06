using Microsoft.AspNetCore.Mvc;
using PersonProfileAPI.Models;
using PersonProfileAPI.Repositories;

namespace PersonProfileAPI.Controllers;

/// <summary>
/// PART 1 — Standard CRUD endpoints.
/// Teach these first so students understand the request/response cycle in Swagger.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly IPersonRepository _repo;

    public PeopleController(IPersonRepository repo)
    {
        _repo = repo;
    }

    // GET api/people
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Person>>> GetAll()
    {
        return Ok(await _repo.GetAllAsync());
    }

    // GET api/people/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Person>> GetById(int id)
    {
        var person = await _repo.GetByIdAsync(id);
        return person is null ? NotFound() : Ok(person);
    }

    // POST api/people
    [HttpPost]
    public async Task<ActionResult<Person>> Create(Person person)
    {
        var created = await _repo.AddAsync(person);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/people/5
    [HttpPut("{id}")]
    public async Task<ActionResult<Person>> Update(int id, Person person)
    {
        var updated = await _repo.UpdateAsync(id, person);
        return updated is null ? NotFound() : Ok(updated);
    }

    // DELETE api/people/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _repo.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
