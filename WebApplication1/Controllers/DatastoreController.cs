﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Cloud.Datastore.V1;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class DatastoreController : Controller
    {
        // GET: Datastore
        public ActionResult Index()
        {
            DatastoreDb db = DatastoreDb.Create("programmingforthecloudbf");

            Query query = new Query("users");

            List<Log> myLogs = new List<Log>();
            foreach (Entity entity in db.RunQuery(query).Entities)
            {
                Log myLog = new Log();
                myLog.Email = entity["email"].StringValue;
                myLog.LastLoggedIn = entity["lastloggedin"].TimestampValue.ToDateTime();
                myLog.Id = entity.Key.ToString();

                myLogs.Add(myLog);
            }

            return View(myLogs);
        }

        public ActionResult Create()
        {
            DatastoreDb db = DatastoreDb.Create("programmingforthecloudbf");

            Entity log = new Entity()
            {
                Key = db.CreateKeyFactory("users").CreateIncompleteKey(), //incompletekey: auto generated id
                ["email"] = User.Identity.Name,
                ["lastloggedin"] = DateTime.UtcNow,
                ["active"] = true
            };

            db.Insert(log);

            return Content("Opereation done!");
        }
    }
}