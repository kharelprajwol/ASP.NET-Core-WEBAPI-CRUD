using MagicVilla_VillaAPI.Data;
//using MagicVilla_VillaAPI.Logging; // Custom Logging
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [ApiController] // Enables api-specific behaviours.
    [Route("api/VillaApi")]  // Path for this controller. [Route("api/[controller]")] can also be used.

    // [Route("api/[controller]")] - Automatically maps the controller name.
    // [Route("api/VillaApi")] - Harcoded but better because route name wont be changed for client even if controller name is changed by developer.

    public class VillaAPIController : ControllerBase // Class must derive ControllerBase Class to be a Controller Class
    {

        private readonly ApplicationDbContext _db;

        public VillaAPIController(ApplicationDbContext db)
        {
            _db = db;
        }

        // DEPENDENCY INJECTION FOR CUSTOM LOGGING
        //private readonly ILogging _logger;
        //public VillaAPIController(ILogging logger)
        //{
        //    _logger = logger;
        //}

        // DEPENDENCY INJECTION FOR BUILT-IN LOGGING
        //private readonly ILogger<VillaAPIController> _logger;
        //public VillaAPIController(ILogger<VillaAPIController> logger)
        //{
        //    _logger = logger;
        //}



        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Documenting possible response type
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            //_logger.Log("Getting all villas","");  // Custom Logging
            //_logger.LogInformation("Getting all villas");  // Built-In Logging
            return Ok(_db.Villas.ToList());
        }


        // [HttpGet("id")]
        [HttpGet("{id:int}", Name = "GetVilla")] // passed id must be of type integer
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200, Type=typeof(VillaDTO))]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if (id == 0)
            {
                //_logger.Log("Get Villa Error with Id" + id,"error"); // Custom Logging
                //_logger.LogError("Getting all villas");  // Built-In Logging
                return BadRequest();
            }

            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

            if (villa == null)
            {
                return NotFound();
            }

            return Ok(villa);
        }

        [HttpPost]
        // Documenting possible response types
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villaDTO)   // [FromBody] meaning - from response body 
        {
            // DEFAULT MODEL STATE VALIDATION
            //if (!ModelState.IsValid) 
            //{
            //    return BadRequest(ModelState);
            //}

            // CUSTOM MODEL STATE VALIDATION
            if (_db.Villas.FirstOrDefault(u => u.Name.ToLower() == villaDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("Custom Error", "Villa already exists"); //AddModelError (string key, string errorMessage);
                return BadRequest(ModelState); // ModelState must be returned for error to be displayed.
            }

            if (villaDTO == null)
            {
                return BadRequest(villaDTO);
            }

            if (villaDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            Villa model = new()
            {
                Amenity = villaDTO.Amenity,
                Details = villaDTO.Details,
                Id = villaDTO.Id,
                ImageUrl = villaDTO.ImageUrl,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft
            };

            _db.Villas.Add(model);
            _db.SaveChanges();

            return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);

        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);

            if (villa == null)
            {
                return NotFound();
            }

            _db.Villas.Remove(villa);
            _db.SaveChanges();
            return NoContent();

        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO) 
        {
            if (villaDTO == null || id != villaDTO.Id) 
            {
                return BadRequest();
            }

            //var villa = VillaStore.VillaList.FirstOrDefault(u => u.Id == id);
            //villa.Name = villaDTO.Name;
            //villa.Occupancy = villaDTO.Occupancy;
            //villa.Sqft = villaDTO.Sqft;

            Villa model = new()
            {
                Amenity = villaDTO.Amenity,
                Details = villaDTO.Details,
                Id = villaDTO.Id,
                ImageUrl = villaDTO.ImageUrl,
                Name = villaDTO.Name,
                Occupancy = villaDTO.Occupancy,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft
            };

            _db.Villas.Update(model);
            _db.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO) 
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }

            var villa = VillaStore.VillaList.FirstOrDefault(u => u.Id == id);

            if (villa == null) 
            {
                return BadRequest();
            }

            patchDTO.ApplyTo(villa, ModelState);

            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            return NoContent();

        }

    }
}
