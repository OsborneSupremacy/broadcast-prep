namespace Broadcast.Prep.Service.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var input = "/Users/ben-osborne/Library/CloudStorage/Dropbox/AV/Bulletins/Current.pdf";

        BroadCast.Prep.Service.PagesToPngService.InvertImage(input);



    }
}