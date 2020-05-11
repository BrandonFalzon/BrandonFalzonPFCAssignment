using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using Google.Cloud.Storage.V1;
using Google.Apis.Storage.v1.Data;

namespace WebApplication1.Controllers
{
    public class ProductsController : Controller
    {
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Product p, HttpPostedFileBase file )
        {
            //upload image related to product on the bucket
            try
            {
                if (file != null)
                {
                    #region Uploading file on Cloud Storage
                    var storage = StorageClient.Create();
                    string link = "";

                    using (var f = file.InputStream)
                    {
                        var filename = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
                        var storageObject = storage.UploadObject("pfcbrandonbucket", filename, null, f);

                        link = "https://storage.cloud.google.com/pfcbrandonbucket/" + filename;

                        if (null == storageObject.Acl)
                        {
                            storageObject.Acl = new List<ObjectAccessControl>();
                        }


                        storageObject.Acl.Add(new ObjectAccessControl()
                        {
                            Bucket = "pfcbrandonbucket",
                            Entity = $"user-" + User.Identity.Name, //whereas ryanattard@gmail.com has to be replaced by a gmail email address who you want to have access granted
                            Role = "OWNER",
                        });

                        storageObject.Acl.Add(new ObjectAccessControl()
                        {
                            Bucket = "pfcbrandonbucket",
                            Entity = $"user-" + p.Shareuser, //whereas ryanattard@gmail.com has to be replaced by a gmail email address who you want to have access granted
                            Role = "READER", //READER
                        });

                        var updatedObject = storage.UpdateObject(storageObject, new UpdateObjectOptions()
                        {
                            // Avoid race conditions.
                            IfMetagenerationMatch = storageObject.Metageneration,
                        });
                    }
                    //store details in a relational db including the filename/link
                    List<Product> results = new List<Product>();
                    p.File = link;
                   // p.Name = System.IO.Path.GetExtension(model.Shareuser);
                    //p.OwnerFK = User.Identity.Name;
                    //p.Shareuser = Shareuser;
                   // results.Add(p);
                    #endregion



                }
                #region Storing details of product in db [INCOMPLETE]
                p.OwnerFK = User.Identity.Name;

                ProductsRepository pr = new ProductsRepository();
                
                
                pr.AddProduct(p);
                #endregion

                #region Updating Cache with latest list of Products from db

                //enable: after you switch on db
                try
                {
                    CacheRepository cr = new CacheRepository();
                    cr.UpdateCache(pr.GetProducts(User.Identity.Name));
                }
                catch (Exception ex)
                {
                    new LogsRepository().LogError(ex);
                }
                

                #endregion
                PubSubRepository psr = new PubSubRepository();
                psr.AddToEmailQueue(p); //adding it to queue to be sent as an email later on.
                ViewBag.Message = "Product created successfully";
                new LogsRepository().WriteLogEntry("Product created successfully for user: " + User.Identity.Name);

                
            }
            catch (Exception ex)
            {
                new LogsRepository().LogError(ex);
                ViewBag.Error = "Product failed to be created; " + ex.Message;
            }

            return View();
            ////upload image related to product on bucket
            //var storage = StorageClient.Create();
            //string link = "";
            //using (var f = file.InputStream)
            //{
            //    var filename = Guid.NewGuid() + System.IO.Path.GetExtension(file.FileName);
            //    var storageObject = storage.UploadObject("pfcbrandonbucket", filename, null, f);
            //    link = storageObject.MediaLink;

            //    if (null == storageObject.Acl)
            //    {
            //        storageObject.Acl = new List<ObjectAccessControl>();
            //    }
            //    storageObject.Acl.Add(new ObjectAccessControl()
            //    {
            //        Bucket = "pfcbrandonbucket",
            //        Entity = $"user-donut238@gmail.com",
            //        Role = "READER",
            //    });
            //    storageObject.Acl.Add(new ObjectAccessControl()
            //    {
            //        Bucket = "pfcbrandonbucket",
            //        Entity = $"user-donut238@gmail.com",
            //        Role = "OWNER",
            //    });
            //    var updatedObject = storage.UpdateObject(storageObject, new UpdateObjectOptions()
            //    {
            //        // Avoid race conditions.
            //        IfMetagenerationMatch = storageObject.Metageneration,
            //    });
            //}
            ////create p in db
            ////p.Link = link;
            //return View();
        }

        // GET: Products
        public ActionResult Index()
        {
            #region get products of db - removed....instead insert next region
            ProductsRepository pr = new ProductsRepository();
            //var products = pr.GetProducts(); //gets products from db
            #endregion

            #region instead of getting products from DB, to make your application faster , you load them from the cache
            CacheRepository cr = new CacheRepository();
            var products = cr.GetProductsFromCache();
            #endregion

            return View("Index", products);
        }

        [HttpPost]
        public ActionResult DeleteAll(int[] ids)
        {
            //1. requirement when opening a transaction: connection has to be opened

            ProductsRepository pr = new ProductsRepository();
            pr.MyConnection.Open();

            pr.MyTransaction = pr.MyConnection.BeginTransaction(); //from this point all code executed against the db will remail pending

            try
            {
                foreach(int id in ids)
                {
                    pr.DeleteProduct(id);
                }

                pr.MyTransaction.Commit(); //Commit: you are confirming the changes in the db
            }
            catch (Exception ex)
            {
                //log the exeption on the cloud
                new LogsRepository().LogError(ex);
                pr.MyTransaction.Rollback(); // Rollback: it will reverse all the changes done within the try-clause in the db
            }

            pr.MyConnection.Close();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            try
            {

                ProductsRepository pr = new ProductsRepository();
                pr.DeleteProduct(id);
            }
            catch (Exception ex)
            {
                new LogsRepository().LogError(ex);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Download(string file)
        {
            new LogsRepository().WriteLogEntry(User.Identity.Name + " downloaded file: " + file + " at " + DateTime.Now);

            return Redirect(file);
        }
    }
}