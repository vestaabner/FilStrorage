using AutoMapper;
using Infrastructure.Dto;
using Infrastructure.Utility;
using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly OnlineShopDbContext dbContext;
    private readonly IMapper mapper;
    private readonly MyFileUtility fileUtility;

    //repository ??
    //unit of work ??

    public ProductService(OnlineShopDbContext dbContext, IMapper mapper,
    MyFileUtility fileUtility)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
        this.fileUtility = fileUtility;
    }
    //DTO => Data Transfer Object
    public async Task<ProductDto> Add(ProductDto model)
    {
        //file => FileStream

        //var product = mapper.Map<Product>(model);
        var product = new Product
        {
            Price = model.Price,
            ProductName = model.ProductName,
            //save in folder
            ThumbnailFileName = fileUtility.SaveFileInFolder(model.Thumbnail, nameof(Product), true),
            //db => byte[]
            Thumbnail = fileUtility.EncryptFile(fileUtility.ConvertToByteArray(model.Thumbnail)),
            ThumbnailFileExtenstion = fileUtility.GetFileExtension(model.Thumbnail.FileName),
            ThumbnailFileSize = model.Thumbnail.Length,
        };

        
       

        await dbContext.AddAsync(product);
        await dbContext.SaveChangesAsync();

        model.Id = product.Id;

        return model;
    }

    public async Task<ProductDto> Get(int id)
    {
        var product = await dbContext.Products.FindAsync(id);
        //var model = mapper.Map<ProductDto>(product);

        var model = new ProductDto
        {
            Id = product.Id,
            Price = product.Price,
            ProductName = product.ProductName,
            PriceWithComma = product.Price.ToString("###.###"),
            ThumbnailBase64 = fileUtility.ConvertToBase64(fileUtility.DecryptFile(product.Thumbnail)),
            //ThumbnailPath = "E:\\DevTube" => "localhost:1220/Media/Attachment/Product/654654654.txt"
            //ThumbnailUrl = fileUtility.GetFileUrl(product.ThumbnailFileName, nameof(Product)),
            ThumbnailUrl = fileUtility.GetEncryptedFileActionUrl(product.ThumbnailFileName, nameof(Product)),
        };
        return model;
    }

    public async Task<List<ProductDto>> GetAll()
    {
        //    var result = await dbContext.Products.Select(product => new ProductDto{
        //        Id = product.Id,
        //         Price = product.Price,
        //         ProductName = product.ProductName,
        //         PriceWithComma = product.Price.ToString("###.###"),
        //    }).ToListAsync();

        var products = await dbContext.Products.ToListAsync();

        var result = mapper.Map<List<ProductDto>>(products);

        return result;
    }
}