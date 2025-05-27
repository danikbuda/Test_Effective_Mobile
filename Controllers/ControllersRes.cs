using Microsoft.AspNetCore.Mvc;
using System.Collections.Frozen;
using Test_Effective_Mobile.Models;

namespace Test_Effective_Mobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllersRes : ControllerBase
    {
        private static FrozenDictionary<string, List<ModelNameRes>> ModelNameResData;



        [HttpGet("load")]
        public async Task<IActionResult> LoadFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                return BadRequest("Файл не найден.");

            try
            {
                var tempData = new Dictionary<string, List<ModelNameRes>>();

                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(stream);

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(":"))
                        continue;

                    var parts = line.Split(':', 2);
                    if (parts.Length != 2)
                        continue;

                    var name = parts[0].Trim();
                    var locations = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


                    foreach (var loc in locations)
                    {
                        if (!tempData.ContainsKey(loc))
                            tempData[loc] = new List<ModelNameRes>();

                        tempData[loc].Add(new ModelNameRes { Name = name });
                       
                    }
                }
                foreach (var loc in tempData.Keys)
                {


                    var location = loc;
                    while (!string.IsNullOrEmpty(location))
                    {
                        if (tempData.TryGetValue(location, out var places))
                        {
                            var splitList = new List<ModelNameRes>();
                            foreach (var place in places)
                            {
                                
                                if (tempData[loc].Any(p => p.Name == place.Name) == false)
                                {
                                    splitList.Add(new ModelNameRes { Name = place.Name });
                                }
                                
                            }
                            tempData[loc].AddRange(splitList);

                        }
                        var lastSlash = location.LastIndexOf('/');
                        if (lastSlash <= 0)
                            break;

                        location = location[..lastSlash];
                    }
                }
                ModelNameResData = tempData.ToFrozenDictionary() ;

                return Ok("Данные загружены.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка загрузки файла: {ex.Message}");
            }
        }
       

        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync([FromQuery] string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return BadRequest("Не указана локация.");

            if (ModelNameResData.TryGetValue(location, out var places))
            {
                return Ok(places);
            }

            return BadRequest("Локация не найдена");

        }
    }
}
