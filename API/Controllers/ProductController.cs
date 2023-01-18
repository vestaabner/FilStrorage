using API.CustomAttributes;
using API.Hubs;
using Infrastructure.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.AccessControl;
using System.Security.Cryptography;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[EnableCors("MyAPI")]
//[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService productService;
    private readonly IWebHostEnvironment environment;
    private readonly IConfiguration configuration;
    private readonly IHubContext<ChatHub> hubContext;

    public ProductController(IProductService productService, IWebHostEnvironment environment,
        IConfiguration configuration, IHubContext<ChatHub> hubContext)
    {
        this.productService = productService;
        this.environment = environment;
        this.configuration = configuration;
        this.hubContext = hubContext;
    }

    [HttpGet("{id}")]
    [SwaggerOperation(
      Summary = "Get a Product",
      Description = "Get a Product with id",
      OperationId = "Products.Get",
      Tags = new[] { "ProductController" })
  ]
    public async Task<IActionResult> Get(int id)
    {
        var result = await productService.Get(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        //call signalR hub from controller
        await hubContext.Clients.All.SendAsync("Notify", $"Call Product GetAll at: {DateTime.Now}");

        var result = await productService.GetAll();
        return Ok(result);
    }

    [HttpPost]
    //[AccessControl(typeof(IPermissionService), Permission = "product-add")]
    //[AccessControl(Permission = "product-add")]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromForm] ProductDto model)
    {
        var result = await productService.Add(model);
        return Ok(result);
    }

    [HttpGet("GetFileContent")]
    [AllowAnonymous]
    public async Task<FileContentResult> GetFileContent(string fileUrl)
    {
        var urlSections = fileUrl.Split("/");
        //read file and decrypt content
        byte[] encryptedData = await System.IO.File.ReadAllBytesAsync("");
        var decryptedData = Decrypt(encryptedData);

        return new FileContentResult(decryptedData, "application/txt");
    }

    [HttpPost("upload")]
    [AllowAnonymous]
    public async Task<IActionResult> Upload(IFormFile thumbnail)
    {
        //1-save to byte[]
        using (var target = new MemoryStream())
        {
            thumbnail.CopyTo(target);
            var thumbnailByteArray = target.ToArray();
        }

        //2-save in folders
        //string filePath = @"E:\"; //???
        var wwwRootPath = environment.WebRootPath;
        var contentRootPath = environment.ContentRootPath;
        var mediaPath = configuration.GetValue<string>("MediaPath");
        var productFolder = "Product";

        //check mediaPath Directory Exists
        if (!Directory.Exists(Path.Combine(wwwRootPath, mediaPath)))
        {
            Directory.CreateDirectory(Path.Combine(wwwRootPath, mediaPath));
        }

        //check productFolder Directory Exists
        if (!Directory.Exists(Path.Combine(wwwRootPath, mediaPath, productFolder)))
        {
            Directory.CreateDirectory(Path.Combine(wwwRootPath, mediaPath, productFolder));
        }

        FileInfo fileInfo = new FileInfo(thumbnail.FileName);
        string fileName = thumbnail.FileName + fileInfo.Extension;


        var uniqueId = DateTime.Now.Ticks;
        var newFileName = $"{uniqueId}{fileInfo.Extension}";

        string fileNameWithPath = Path.Combine(wwwRootPath, mediaPath, productFolder, newFileName);

        //convert IFormFile to byte[]
        byte[] encryptedData;
        using (var ms = new MemoryStream())
        {
            thumbnail.CopyTo(ms);
            var fileBytes = ms.ToArray();
            encryptedData = Encrypt(fileBytes);
        }

        
        //write byte[] to file
        using var writer = new BinaryWriter(System.IO.File.OpenWrite(fileNameWithPath));
        writer.Write(encryptedData);

        return Ok(fileNameWithPath);
    }

    //[HttpGet("GetFile")]
    //[AllowAnonymous]
    //public FileResult GetFile(string filePath)
    //{
    //    return File(filePath, "application/pdf");
    //}

    // [HttpGet("GetFileContent")]
    // [AllowAnonymous]
    // public async Task<FileContentResult> GetFileContent(string filePath)
    // {
    //     //read file and decrypt content
    //     byte[] encryptedData = await System.IO.File.ReadAllBytesAsync(filePath);
    //     var decryptedData = Decrypt(encryptedData);

    //     return new FileContentResult(decryptedData, "application/txt");
    // }

    private byte[] Encrypt(byte[] fileContent)
    {
        string EncryptionKey = "MAKV2SPBNI54324";
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(fileContent, 0, fileContent.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }

    private byte[] Decrypt(byte[] fileContent)
    {
        string EncryptionKey = "MAKV2SPBNI54324";
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);


            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(fileContent, 0, fileContent.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }
}