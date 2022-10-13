using System.Diagnostics;
using System.Globalization;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using oneHut.ConnectDB;
using oneHut.Models;
using System.Threading;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace oneHut.Controllers;

public class BookingController : Controller
{
    private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;

    private readonly ILogger<BookingController> _logger;

    public BookingController(ILogger<BookingController> logger, Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
    {
        Environment = _environment;
        _logger = logger;
    }

    private string validateDate(string strDate)
    {
        DateTimeStyles styles;
        DateTime dateResult;

        Regex regex = new Regex(@"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d))$");

        if (strDate.Trim().Length < 10) { throw new Exception(); }

        //Verify whether date entered in dd/MM/yyyy format.
        bool isValid = regex.IsMatch(strDate.Trim().Substring(0, 10));
        if (!isValid) { throw new Exception(); }

        // Parse a date and time with no styles.
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        styles = DateTimeStyles.None;

        if (strDate.Contains("00:00 00")) { if (!DateTime.TryParseExact(strDate.Substring(0, 10), "dd/MM/yyyy", Thread.CurrentThread.CurrentCulture, DateTimeStyles.None, out dateResult)) { throw new Exception(); } }
        else { if (!DateTime.TryParse(strDate, Thread.CurrentThread.CurrentCulture, styles, out dateResult)) { throw new Exception(); } }

        if (strDate.Length == 10) { return strDate = strDate + " 00:00 00"; }
        return strDate;

    }

    [HttpGet]
    public IActionResult Booking(string pageNo)
    {
        //new OneHutData().AzureUploadFile();
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }
        ViewBag.pageName = "Booking";
        return View(new OneHutData().GetBookings(
             new BookingModel()
             {
                 CurrentPage = string.IsNullOrEmpty(pageNo) ? 1 : Convert.ToInt32(pageNo),
                 Guest = HttpContext.Session.GetString("_Guest"),
                 CheckIn = HttpContext.Session.GetString("_CheckIn"),
                 CheckOut = HttpContext.Session.GetString("_CheckOut"),
                 IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
             },
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") }));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult Booking(List<IFormFile> postedFiles, BookingModel bookingModel)
    {


        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }
        // add booking
        bool newBooking = true;
        OneHutData oneHutData = new OneHutData();
        String id = bookingModel.Book._id;
        String guestname = bookingModel.Book.GuestName;
        Regex regex = new Regex(@"\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}\s+(AM|PM)");

        if (String.IsNullOrEmpty(id))
        {
            HttpContext.Session.SetString("_Guest", string.Empty);
            HttpContext.Session.SetString("_CheckIn", string.Empty);
            HttpContext.Session.SetString("_CheckOut", string.Empty);
            HttpContext.Session.SetString("_IsToday", "false");
            HttpContext.Session.SetString("_CurrentPage", "1");
            bookingModel.Book.UserID = HttpContext.Session.GetString("_UserID");
            bookingModel.Book.UserGroupID = HttpContext.Session.GetString("_UserGroupID");
            bookingModel.Book.Status = "Booked";
        }
        try
        {
            if (!String.IsNullOrEmpty(id))
            {
                bookingModel.Book.CheckIn = validateDate(bookingModel.Book.CheckIn.ToUpper());
                bookingModel.Book.CheckOut = validateDate(bookingModel.Book.CheckOut.ToUpper());
                newBooking = false;
            }

            // bookingModel.Book.Rooms = bookingModel.Book.Rooms.Replace(",", ", ");
            bookingModel.Book.BillAmount = CovertToCurrency(bookingModel.Book.BillAmount);
            bookingModel.Book.AmountPaid = CovertToCurrency(bookingModel.Book.AmountPaid);

            bookingModel = oneHutData.AddBooking(bookingModel, new Models.User()
            {
                UserID = HttpContext.Session.GetString("_UserID"),
                UserGroupID = HttpContext.Session.GetString("_UserGroupID")
            });

            if (postedFiles.Count != 0)
            {
                // upload file 
                if (string.IsNullOrEmpty(id)) { id = bookingModel.Book._id; }

                //upload files to WWW root
                UploadFiles(postedFiles, id);
                bookingModel.PostedFiles = GetUploadedFiles(id);

                if (HttpContext.Session.GetString("_isAzureStorage").Equals("Y"))
                {             //upload file to Azure Storage
                    string path = Path.Combine(this.Environment.WebRootPath, "Uploads\\" + HttpContext.Session.GetString("_UserGroupID").Trim() + "\\" + id);
                    string strdirectory = @"Uploads/" + HttpContext.Session.GetString("_UserGroupID") + @"/" + id;
                    oneHutData.AzureUploadFile(bookingModel.PostedFiles, strdirectory, path);

                    //Delete wwwroot files - Uncomment when qzure storage enabled
                    if (Directory.Exists(path)) { Directory.Delete(path, true); }
                }
            }
        }
        catch (Exception e)
        {

            //Retrive Booking
            BookingModel _exbookingModel = new BookingModel();

            _exbookingModel.Guest = HttpContext.Session.GetString("_Guest");
            _exbookingModel.CheckIn = HttpContext.Session.GetString("_CheckIn");
            _exbookingModel.CheckOut = HttpContext.Session.GetString("_CheckOut");
            _exbookingModel.CurrentPage = Convert.ToInt32(HttpContext.Session.GetString("_CurrentPage"));
            _exbookingModel.IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"));

            //Retrive Booking
            bookingModel = oneHutData.GetBookings(
            _exbookingModel,
                new Models.User()
                {
                    UserID = HttpContext.Session.GetString("_UserID"),
                    UserGroupID = HttpContext.Session.GetString("_UserGroupID")
                });

            bookingModel.Book._id = id;
            bookingModel.Book.GuestName = guestname;
            bookingModel.Message = "Error saving data!";

            if (HttpContext.Session.GetString("_isAzureStorage").Equals("Y"))
            {
                bookingModel.PostedFiles = oneHutData.AzureUploadedFileAccess(@"Uploads/" + HttpContext.Session.GetString("_UserGroupID") + @"/" + id, "https://onehut.file.core.windows.net/onehutfileshare/");
            }
            else
            {
                bookingModel.PostedFiles = GetUploadedFiles(id);
            }

            ViewBag.pageName = "Booking";
            return View(bookingModel);

        }
        ModelState.Clear();

        BookingModel _bookingModel = new BookingModel();

        _bookingModel.Guest = HttpContext.Session.GetString("_Guest");
        _bookingModel.CheckIn = HttpContext.Session.GetString("_CheckIn");
        _bookingModel.CheckOut = HttpContext.Session.GetString("_CheckOut");
        _bookingModel.CurrentPage = Convert.ToInt32(HttpContext.Session.GetString("_CurrentPage"));
        _bookingModel.IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"));

        //Retrive Booking
        bookingModel = oneHutData.GetBookings(
           _bookingModel,
            new Models.User()
            {
                UserID = HttpContext.Session.GetString("_UserID"),
                UserGroupID = HttpContext.Session.GetString("_UserGroupID")
            });
        bookingModel.Book = new Booking();
        if (!newBooking) { bookingModel.Message = guestname.Split(' ').FirstOrDefault() + "'s booking updated successfully!"; }
        else { bookingModel.Message = guestname.Split(' ').FirstOrDefault() + "'s booking successful!"; }
        ViewBag.pageName = "Booking";

        return View(bookingModel);

    }

    [HttpGet]
    public IActionResult UpdateBooking(string useraction, string id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        OneHutData oneHutData = new OneHutData();

        oneHutData.UpdateBooking(useraction, id, new Models.User());

        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel()
            {
                Guest = HttpContext.Session.GetString("_Guest"),
                CheckIn = HttpContext.Session.GetString("_CheckIn"),
                CheckOut = HttpContext.Session.GetString("_CheckOut"),
                CurrentPage = Convert.ToInt32(HttpContext.Session.GetString("_CurrentPage")),
                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
            },
            new Models.User()
            {
                UserID = HttpContext.Session.GetString("_UserID"),
                UserGroupID = HttpContext.Session.GetString("_UserGroupID")
            });
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpGet]
    public IActionResult EditBooking(string id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        ModelState.Clear();
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel()
            {
                Guest = HttpContext.Session.GetString("_Guest"),
                CheckIn = HttpContext.Session.GetString("_CheckIn"),
                CheckOut = HttpContext.Session.GetString("_CheckOut"),
                CurrentPage = Convert.ToInt32(HttpContext.Session.GetString("_CurrentPage")),
                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
            },
            new Models.User() { UserID = HttpContext.Session.GetString("_UserID") });
        bookingModel.Book = bookingModel.Bookings.Find(it => it._id.Equals(id));
        ViewBag.pageName = "Booking";

        if (HttpContext.Session.GetString("_isAzureStorage").Equals("Y"))
        {
            //Get files from Azure Storage 
            bookingModel.PostedFiles = oneHutData.AzureUploadedFileAccess(@"Uploads/" + HttpContext.Session.GetString("_UserGroupID") + @"/" + id, "https://onehut.file.core.windows.net/onehutfileshare/");

        }
        else
        {
            //wwwroot files 
            bookingModel.PostedFiles = GetUploadedFiles(bookingModel.Book._id);
        }



        return View("Booking", bookingModel);
    }

    [HttpGet]
    public IActionResult ClearBooking()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        ModelState.Clear();
        HttpContext.Session.SetString("_Guest", string.Empty);
        HttpContext.Session.SetString("_CheckIn", string.Empty);
        HttpContext.Session.SetString("_CheckOut", string.Empty);
        HttpContext.Session.SetString("_CurrentPage", "1");
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel()
            {
                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
            },
            new Models.User()
            {
                UserID = HttpContext.Session.GetString("_UserID"),
                UserGroupID = HttpContext.Session.GetString("_UserGroupID")
            });
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpPost]
    public IActionResult SearchBooking(BookingModel bookingModel)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }
        HttpContext.Session.SetString("_Guest", string.Empty);
        HttpContext.Session.SetString("_CheckIn", string.Empty);
        HttpContext.Session.SetString("_CheckOut", string.Empty);
        HttpContext.Session.SetString("_CurrentPage", "1");
        bookingModel.CurrentPage = 1;
        if (!string.IsNullOrEmpty(bookingModel.CheckIn))
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            bookingModel.CheckIn = Convert.ToDateTime(bookingModel.CheckIn.Trim()).ToString("dd/MM/yyyy").ToUpper();
            HttpContext.Session.SetString("_CheckIn", bookingModel.CheckIn.ToString());
        }

        if (!string.IsNullOrEmpty(bookingModel.CheckOut))
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            bookingModel.CheckOut = Convert.ToDateTime(bookingModel.CheckOut.Trim()).ToString("dd/MM/yyyy").ToUpper();
            HttpContext.Session.SetString("_CheckOut", bookingModel.CheckOut);
        }

        if (!string.IsNullOrEmpty(bookingModel.Guest))
        {
            HttpContext.Session.SetString("_Guest", Convert.ToString(bookingModel.Guest));
        }
        HttpContext.Session.SetString("_IsToday", "false");
        bookingModel.IsToday = false;

        OneHutData oneHutData = new OneHutData();
        BookingModel _bookingModel = new BookingModel();
        _bookingModel = oneHutData.GetBookings(
            bookingModel,
            new Models.User()
            {
                UserID = HttpContext.Session.GetString("_UserID"),
                UserGroupID = HttpContext.Session.GetString("_UserGroupID")
            });
        ViewBag.pageName = "Booking";
        return View("Booking", _bookingModel);
    }

    [HttpGet]
    public IActionResult TodayBooking()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        if (HttpContext.Session.GetString("_IsToday") == "true")
        {
            HttpContext.Session.SetString("_IsToday", "false");
            HttpContext.Session.SetString("_Guest", string.Empty);
            HttpContext.Session.SetString("_CheckIn", string.Empty);
            HttpContext.Session.SetString("_CheckOut", string.Empty);
        }
        else
        {
            HttpContext.Session.SetString("_IsToday", "true");
            HttpContext.Session.SetString("_Guest", string.Empty);
            HttpContext.Session.SetString("_CheckIn", string.Empty);
            HttpContext.Session.SetString("_CheckOut", string.Empty);
        }

        ModelState.Clear();
        OneHutData oneHutData = new OneHutData();
        BookingModel bookingModel = oneHutData.GetBookings(
            new BookingModel()
            {
                IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"))
            },
            new Models.User()
            {
                UserID = HttpContext.Session.GetString("_UserID"),
                UserGroupID = HttpContext.Session.GetString("_UserGroupID")
            });
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    [HttpPost]
    public IActionResult NextPageBooking(BookingModel bookingModel)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("_UserID")))
        {
            ViewBag.pageName = "";
            return RedirectToAction("Index", "Home");
        }

        HttpContext.Session.SetString("_CurrentPage", bookingModel.CurrentPage.ToString());
        bookingModel.Guest = HttpContext.Session.GetString("_Guest");
        bookingModel.CheckIn = HttpContext.Session.GetString("_CheckIn");
        bookingModel.CheckOut = HttpContext.Session.GetString("_CheckOut");
        bookingModel.IsToday = Convert.ToBoolean(HttpContext.Session.GetString("_IsToday"));

        OneHutData oneHutData = new OneHutData();
        BookingModel _bookingModel = new BookingModel();
        bookingModel = oneHutData.GetBookings(
            bookingModel,
            new Models.User()
            {
                UserID = HttpContext.Session.GetString("_UserID"),
                UserGroupID = HttpContext.Session.GetString("_UserGroupID")
            });
        ViewBag.pageName = "Booking";
        return View("Booking", bookingModel);
    }

    private void UploadFiles(List<IFormFile> postedFiles, string _id)
    {
        string wwwPath = this.Environment.WebRootPath;
        string contentPath = this.Environment.ContentRootPath;

        string path = Path.Combine(this.Environment.WebRootPath, "Uploads\\" + HttpContext.Session.GetString("_UserGroupID").Trim() + "\\" + _id);

        // if (Directory.Exists(path)) { Directory.Delete(path, true); }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        List<string> uploadedFiles = new List<string>();
        int fileCount = 1;
        foreach (IFormFile postedFile in postedFiles)
        {
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + fileCount.ToString() + Path.GetFileName(postedFile.FileName);
            using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                postedFile.CopyTo(stream);
                uploadedFiles.Add(fileName);
                //ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", fileName);
            }
            fileCount += 1;
        }
    }

    private List<string> GetUploadedFiles(string _id)
    {
        string wwwPath = this.Environment.WebRootPath;
        string contentPath = this.Environment.ContentRootPath;

        string path = Path.Combine(this.Environment.WebRootPath, "Uploads\\" + HttpContext.Session.GetString("_UserGroupID").Trim() + "\\" + _id);

        // if (Directory.Exists(path)) { Directory.Delete(path, true); }

        List<string> uploadedFiles = new List<string>();

        if (Directory.Exists(path))
        {

            foreach (string fileName in Directory.GetFiles(path).ToArray().OrderBy(it => it))
            {
                uploadedFiles.Add("/Uploads" + fileName.Split("Uploads").LastOrDefault().ToString().Replace("\\", "/"));
            }
        }

        //uploadedFiles.Add("/Uploads/USR0001/632f16625f51c805bb23f952/HeartBox.jpg");
        return uploadedFiles;
    }

    #region Validation

    private string CovertToCurrency(string Amount)
    {
        if (string.IsNullOrEmpty(Amount)) { return string.Empty; }

        return string.IsNullOrEmpty(Amount.Replace("₹", string.Empty)) ? string.Empty :
         string.Format("{0:#.00}", Convert.ToDecimal(Amount.Replace("₹", string.Empty))).Trim();
    }


    #endregion




}
