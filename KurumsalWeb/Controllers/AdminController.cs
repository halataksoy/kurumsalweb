
using KurumsalWeb.Models;
using KurumsalWeb.Models.DataContext;
using KurumsalWeb.Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace KurumsalWeb.Controllers
{
    public class AdminController : Controller
    {
        KurumsalDBContext db = new KurumsalDBContext();
        // GET: Admin
        [Route("yonetimpaneli")]
        public ActionResult Index()
        {
            ViewBag.BlogSay = db.Blog.Count();
            ViewBag.KategoriSay = db.Kategori.Count();
            ViewBag.HizmetSay = db.Hizmet.Count();
            ViewBag.YorumSay = db.Yorum.Count();
            ViewBag.YorumOnay = db.Yorum.Where(x => x.Onay == false).Count();//sayısını al bize getir
            var sorgu = db.Kategori.ToList();
            return View(sorgu);
        }
        [Route("yonetimpaneli/giris")]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Admin admin)
        {
            //login sayfasındaki şifremizi alıp md5 olarak vericek bize;
            var login = db.Admin.Where(x => x.Eposta == admin.Eposta).SingleOrDefault();
            if (login.Eposta == admin.Eposta && login.Sifre == Crypto.Hash(admin.Sifre, "MD5"));
            {
                Session["adminid"] = login.AdminId;
                Session["eposta"] = login.Eposta;
                Session["yetki"] = login.Yetki;
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.Uyari = "Kullanıcı adı veya şifre yanlış";
            return View(admin);
        }
        public ActionResult Logout()
        {
            Session["adminid"] = null;
            Session["eposta"] = null;
            Session.Abandon();
            return RedirectToAction("Login", "Admin");
        }
        public ActionResult SifremiUnuttum()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SifremiUnuttum(string eposta)
        {
            var mail = db.Admin.Where(x => x.Eposta == eposta).SingleOrDefault();
            if (mail!=null)
            {
                Random rmd = new Random();//rastgele sayı üretir random
                int yenisifre = rmd.Next();//üretilen randomu yeni şifreye ata

                Admin admin= new Admin();//admine yeni bir şifre üreteceğimizi belirrtil
                mail.Sifre = Crypto.Hash(Convert.ToString(yenisifre), "MD5");//gelen kaydı int stringe çeviirp sonra da md5 ile şifrelemesini sağladık 
                db.SaveChanges();//veritabnına kaydet

                WebMail.SmtpServer = "smtp.gmail.com";
                WebMail.EnableSsl = true;//güvenli bağlantı oluştur
                WebMail.UserName = "kurumsalweb1@gmail.com";
                WebMail.Password = "Kurumsalweb5148";
                WebMail.SmtpPort = 587;
                WebMail.Send(eposta, "Admin panel giriş şifreniz" ,"Şifreniz : " +yenisifre);//bizim mail prosedürümüzü uygulayarak girilen e postyaa gönder
                ViewBag.Uyari = "Şifreniz başarı ile gönderilmiştir.";

            }
            else
            {
                ViewBag.Uyari = "Hata oluştu.Tekrar deneyiniz.";
            }
            return View();
        }
        public ActionResult Adminler()
        {
            return View(db.Admin.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Admin admin,string sifre,string eposta)
        {
            if (ModelState.IsValid)
            {
                admin.Sifre = Crypto.Hash(sifre, "MD5");
                db.Admin.Add(admin);
                db.SaveChanges();

                return RedirectToAction("Adminler");
            }
            return View();
        }
        public ActionResult Edit(int id)
        {
            var a = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();
            return View(a);
        }
        [HttpPost]
        public ActionResult Edit(int id,Admin admin,string sifre,string eposta)
        {
            if (ModelState.IsValid)
            {
                var a = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();
                a.Sifre = Crypto.Hash(sifre, "MD5");
                a.Eposta = admin.Eposta;
                a.Yetki = admin.Yetki;
                db.SaveChanges();
                RedirectToAction("Adminler");
            }
            return View(admin);
        }
        public ActionResult Delete(int id)
        {
            var a = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();
            if (a!=null)
            {
                db.Admin.Remove(a);//sil  
                db.SaveChanges();
                return RedirectToAction("Adminler");
            }
            return View();
        }
    }
}