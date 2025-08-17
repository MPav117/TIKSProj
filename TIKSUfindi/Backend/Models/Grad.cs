
//using OfficeOpenXml;
namespace Models;
public class Grad
{
    //https://simplemaps.com/data/world-cities

    [Key]
    public int ID { get; set; }

    public required string City { get; set; }
    public required string City_ascii { get; set; }
    public required string Country { get; set; }
    public required string Admin_name { get; set; }

}


