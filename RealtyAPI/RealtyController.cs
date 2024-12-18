using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RealtyAPI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RealtyContext>(options =>
                options.UseNpgsql("Host=localhost;Port=5440;Username=user;Password=user;Database=pish_db;SslMode=Disable"));
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class RealtyContext : DbContext
    {
        public DbSet<Realtor> Realtors { get; set; }
        public DbSet<Realty> Properties { get; set; }
        public DbSet<Sale> Sales { get; set; }

        public RealtyContext(DbContextOptions<RealtyContext> options) : base(options)
        {
        }
    }

    [Table("realtors")]
    public class Realtor
    {
        [Column("realtor_id", TypeName = "int")]
        public int Id { get; set; }

        [Column("first_name", TypeName = "varchar")]
        public string FirstName { get; set; }

        [Column("last_name", TypeName = "varchar")]
        public string LastName { get; set; }

        [Column("middle_name", TypeName = "varchar")]
        public string MiddleName { get; set; }

        [Column("contact_phone", TypeName = "varchar")]
        public string ContactPhone { get; set; }
    }

    [Table("realty_objects")]
    public class Realty
    {
        [Column("realty_object_id", TypeName = "serial")]
        public int Id { get; set; }

        [Column("district_id", TypeName = "int")]
        public int DistrictID { get; set; }

        [Column("address", TypeName = "varchar")]
        public string Address { get; set; }

        [Column("floorlevel", TypeName = "int")]
        public int FloorLevel { get; set; }

        [Column("type_id", TypeName = "int")]
        public int TypeID { get; set; }

        [Column("status", TypeName = "int")]
        public int Status { get; set; }

        [Column("price", TypeName = "int")]
        public int Price { get; set; }

        [Column("object_desc", TypeName = "text")]
        public string ObjectDesc { get; set; }

        [Column("material_id", TypeName = "int")]
        public int MaterialID { get; set; }

        [Column("area", TypeName = "int")]
        public int Area { get; set; }

        [Column("announcement_dt", TypeName = "timestamp")]
        public DateTime AnnouncmentDT { get; set; }
    }

    [Table("sales")]
    public class Sale
    {
        [Key]
        [Column("sale_id", TypeName = "serial4")]
        public int SaleId { get; set; }

        [ForeignKey("Realty")]
        [Column("realty_object_id", TypeName = "int8")]
        public int RealtyObjectId { get; set; }

        [ForeignKey("Realtor")]
        [Column("realtor_id", TypeName = "int8")]
        public int RealtorId { get; set; }

        [Column("sale_dt", TypeName = "timestamp")]
        public DateTime SaleDate { get; set; }

        [Column("sale_price", TypeName = "float8")]
        public double SalePrice { get; set; }

        // Навигационные свойства
        public Realty Realty { get; set; }
        public Realtor Realtor { get; set; }
    }


    [ApiController]
    [Route("api/[controller]")]
    public class RealtyController : ControllerBase
    {
        private readonly RealtyContext _context;

        public RealtyController(RealtyContext context)
        {
            _context = context;
        }

        // GET: api/Realty
        [HttpGet]
        public IActionResult GetRealties()
        {
            var realties = _context.Properties.ToList();
            return Ok(realties);
        }

        // GET: api/Realty/{id}
        [HttpGet("{id}")]
        public IActionResult GetRealty(int id)
        {
            var realty = _context.Properties.FirstOrDefault(r => r.Id == id);

            if (realty == null)
            {
                return NotFound();
            }

            return Ok(realty);
        }

        // POST: api/Realty
        [HttpPost]
        public IActionResult AddRealty([FromBody] Realty newRealty)
        {
            if (newRealty == null)
            {
                return BadRequest("Realty object is null");
            }

            _context.Properties.Add(newRealty);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetRealty), new { id = newRealty.Id }, newRealty);
        }

        // PUT: api/Realty/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateRealty(int id, [FromBody] Realty updatedRealty)
        {
            if (updatedRealty == null)
            {
                return BadRequest("Realty object is null");
            }

            var existingRealty = _context.Properties.FirstOrDefault(r => r.Id == id);

            if (existingRealty == null)
            {
                return NotFound();
            }

            existingRealty.DistrictID = updatedRealty.DistrictID;
            existingRealty.Address = updatedRealty.Address;
            existingRealty.FloorLevel = updatedRealty.FloorLevel;
            existingRealty.TypeID = updatedRealty.TypeID;
            existingRealty.Status = updatedRealty.Status;
            existingRealty.Price = updatedRealty.Price;
            existingRealty.ObjectDesc = updatedRealty.ObjectDesc;
            existingRealty.MaterialID = updatedRealty.MaterialID;
            existingRealty.Area = updatedRealty.Area;
            existingRealty.AnnouncmentDT = updatedRealty.AnnouncmentDT;

            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/Realty/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteRealty(int id)
        {
            var realty = _context.Properties.FirstOrDefault(r => r.Id == id);

            if (realty == null)
            {
                return NotFound();
            }

            _context.Properties.Remove(realty);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet("AveragePriceByRealtor")]
        public IActionResult GetAveragePriceByRealtor(int year)
        {
            var result = _context.Sales
                .Where(s => s.SaleDate.Year == year) 
                .GroupBy(s => new { s.RealtorId, s.Realtor.FirstName, s.Realtor.LastName }) // Группируем по риэлтору
                .Select(group => new
                {
                    RealtorName = group.Key.FirstName + " " + group.Key.LastName,
                    AveragePrice = group.Average(s => s.SalePrice)
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("LowPricePerSquareMeter")]
        public IActionResult GetLowPricePerSquareMeter()
        {
            // Сначала вычислим среднюю стоимость за м² по каждому району
            var avgPricePerDistrict = _context.Properties
                .GroupBy(p => p.DistrictID) 
                .Select(g => new
                {
                    DistrictID = g.Key,
                    AveragePricePerMeter = g.Average(p => (double)p.Price / p.Area) // Средняя стоимость 1 м² в районе
                })
                .ToList(); 

            // Теперь найдем объекты, у которых стоимость 1 м² меньше средней по району
            var result = _context.Properties
                .Where(p => p.Area > 0)
                .Select(p => new
                {
                    p.Address, // Адрес объекта
                    PricePerMeter = (double)p.Price / p.Area, // Стоимость за м²
                    p.DistrictID
                })
                .ToList() 
                .Where(p => avgPricePerDistrict
                    .Any(d => d.DistrictID == p.DistrictID && p.PricePerMeter < d.AveragePricePerMeter)) // Сравниваем с средней стоимостью 1 м² в районе
                .Select(p => p.Address) 
                .ToList();

            return Ok(result);
        }

        [HttpGet("RealtorsWithFewSales")]
        public IActionResult GetRealtorsWithFewSales()
        {
            var result = _context.Sales
                .GroupBy(s => new { s.RealtorId, s.Realtor.FirstName, s.Realtor.LastName })
                .Where(group => group.Count() < 5) // Фильтруем риэлторов, которые продали меньше 5 объектов
                .Select(group => new
                {
                    RealtorName = group.Key.FirstName + " " + group.Key.LastName 
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("RealtorsWithNoSalesThisYear")]
        public IActionResult GetRealtorsWithNoSalesThisYear()
        {
            int currentYear = System.DateTime.Now.Year;

            // Выбираем риэлторов, которые не имеют продаж в текущем году
            var result = _context.Realtors
                .Where(r => !_context.Sales
                    .Any(s => s.RealtorId == r.Id && s.SaleDate.Year == currentYear)) // Проверяем, есть ли у риэлтора продажи в текущем году
                .Select(r => new
                {
                    FullName = r.FirstName + " " + r.LastName 
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet("YearsWithSpecificRealtyCount")]
        public IActionResult GetYearsWithSpecificRealtyCount()
        {
            // Группируем объекты недвижимости по году размещения
            var result = _context.Properties
                .GroupBy(p => p.AnnouncmentDT.Year) 
                .Where(g => g.Count() >= 2 && g.Count() <= 3) // Фильтруем годы с количеством объектов от 2 до 3
                .Select(g => g.Key) 
                .ToList();

            return Ok(result);
        }

        [HttpGet("MostExpensiveRealtyByDistrict")]
        public IActionResult GetMostExpensiveRealtyByDistrict()
        {
            var result = _context.Properties
                .GroupBy(p => p.DistrictID) 
                .Select(g => new
                {
                    DistrictID = g.Key, // ID района
                    MostExpensiveRealty = g.OrderByDescending(p => p.Price).FirstOrDefault() // Самый дорогой объект в районе
                })
                .ToList(); 

            // Проекция данных в удобный формат для ответа
            var formattedResult = result.Select(g => new
            {
                DistrictID = g.DistrictID,
                Address = g.MostExpensiveRealty?.Address,
                Price = g.MostExpensiveRealty?.Price
            }).ToList();

            return Ok(formattedResult);
        }
    }
}
