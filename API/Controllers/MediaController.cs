using Infrastructure.Utility;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class MediaController:ControllerBase
{
    private readonly MyFileUtility myFileUtility;

    public MediaController(MyFileUtility myFileUtility)
    {
        this.myFileUtility = myFileUtility;
    }
    [HttpGet("/Media/{entity}/{fileName}")]
    public async Task<IActionResult> Get(string fileName, string entity)
    {
        var filePath = myFileUtility.GetFileFullPath(fileName, entity);
        byte[] encryptedData = await System.IO.File.ReadAllBytesAsync(filePath);
        var decryptedData = myFileUtility.DecryptFile(encryptedData);

        return new FileContentResult(decryptedData, "application/txt");
    }
}