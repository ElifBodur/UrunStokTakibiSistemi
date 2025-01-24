using System;
using System.Linq;
using System.Web.Mvc;
using UrunStokTakip.Models;
using PagedList;

namespace UrunStokTakip.Controllers
{
    public class SatisController : Controller
    {
        UrunTakipSistemiEntities db = new UrunTakipSistemiEntities();

        // Satış listesi (sayfalama ile)
        public ActionResult Index(int sayfa = 1)
        {
            if (User.Identity.IsAuthenticated)
            {
                var kullaniciadi = User.Identity.Name;
                var kullanici = db.Kullanici.FirstOrDefault(x => x.Email == kullaniciadi);
                if (kullanici == null) return HttpNotFound();

                var model = db.Satislar
                              .Where(x => x.KullaniciId == kullanici.Id)
                              .ToList()
                              .ToPagedList(sayfa, 5);
                return View(model);
            }
            return HttpNotFound();
        }

        // Tek ürün satın alma sayfası
        public ActionResult SatinAl(int id)
        {
            var model = db.Sepet.FirstOrDefault(x => x.Id == id);
            if (model == null) return HttpNotFound();

            return View(model);
        }

        // Tek ürün satın alma işlemi
        [HttpPost]
        public ActionResult SatinAl2(int id)
        {
            try
            {
                var model = db.Sepet.FirstOrDefault(x => x.Id == id);
                if (model == null)
                {
                    TempData["Message"] = "Ürün bulunamadı.";
                    return RedirectToAction("Index", "Sepet");
                }

                var satis = new Satislar
                {
                    KullaniciId = model.KullaniciId,
                    UrunId = model.UrunId,
                    Adet = model.Adet,
                    Resim = model.Resim,
                    Fiyat = model.Fiyat,
                    Tarih = DateTime.Now
                };

                db.Sepet.Remove(model);
                db.Satislar.Add(satis);
                db.SaveChanges();

                TempData["Message"] = "Satın alma işlemi başarılı bir şekilde gerçekleşmiştir.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Satın alma işlemi başarısız: " + ex.Message;
            }
            return RedirectToAction("Index", "Sepet");
        }

        // Tüm ürünleri satın alma sayfası
        public ActionResult HepsiniSatinAl()
        {
            if (User.Identity.IsAuthenticated)
            {
                var kullaniciadi = User.Identity.Name;
                var kullanici = db.Kullanici.FirstOrDefault(x => x.Email == kullaniciadi);
                if (kullanici == null) return HttpNotFound();

                var sepet = db.Sepet.Where(x => x.KullaniciId == kullanici.Id).ToList();
                if (!sepet.Any())
                {
                    ViewBag.Tutar = "Sepetinizde ürün bulunmamaktadır.";
                    return View(sepet);
                }

                var toplamTutar = sepet.Sum(x => x.Urun.Fiyat * x.Adet);
                ViewBag.Tutar = "Toplam Tutar: " + toplamTutar + " TL";
                return View(sepet);
            }
            return HttpNotFound();
        }

        // Tüm ürünleri satın alma işlemi
        [HttpPost]
        public ActionResult HepsiniSatinAl2()
        {
            try
            {
                var username = User.Identity.Name;
                var kullanici = db.Kullanici.FirstOrDefault(x => x.Email == username);
                if (kullanici == null) return HttpNotFound();

                var sepet = db.Sepet.Where(x => x.KullaniciId == kullanici.Id).ToList();
                if (!sepet.Any())
                {
                    TempData["Message"] = "Sepetinizde ürün bulunmamaktadır.";
                    return RedirectToAction("Index", "Sepet");
                }

                foreach (var item in sepet)
                {
                    var satis = new Satislar
                    {
                        KullaniciId = item.KullaniciId,
                        UrunId = item.UrunId,
                        Adet = item.Adet,
                        Fiyat = item.Fiyat,
                        Resim = item.Urun.Resim,
                        Tarih = DateTime.Now
                    };
                    db.Satislar.Add(satis);
                }

                db.Sepet.RemoveRange(sepet);
                db.SaveChanges();

                TempData["Message"] = "Tüm ürünler başarıyla satın alındı.";
                return RedirectToAction("Index", "Sepet");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Satın alma işlemi başarısız: " + ex.Message;
                return RedirectToAction("HepsiniSatinAl");
            }
        }
    }
}
