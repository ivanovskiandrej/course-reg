using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CourseAdvisor.Models;
using Microsoft.AspNet.Identity;


namespace CourseAdvisor.Controllers
{
    public class EnrollmentsController : Controller
    {
        private DbModelEntities db = new DbModelEntities();

        //GET: Remaining
        public ActionResult Remaining()
        {
            var userId = User.Identity.GetUserId();
            var enrollments = db.Enrollments.Where(e => e.StudentId == userId).ToList();
            //.Include(e => e.User);
            var possibleCourses = db.Courses.ToList();
            IList<Course> remainingCourses = new List<Course>();

            IList<int> enrollmentIdList = new List<int>();
            for (int i = 0; i < enrollments.Count; i++)
                enrollmentIdList.Add(enrollments[i].CourseId);

            IList<int> courseIdList = new List<int>();
            for (int i = 0; i < possibleCourses.Count; i++)
                courseIdList.Add(possibleCourses[i].CourseId);

            for (int i = 0; i < enrollmentIdList.Count; i++)
            {
                if (courseIdList.Contains(enrollmentIdList[i]))
                    courseIdList.Remove(enrollmentIdList[i]);
                // remove the course that has enrollmentid[i]
            }

            for (int i = 0; i < possibleCourses.Count; i++)
            {
                for (int j = 0; j < courseIdList.Count; j++)
                    if (possibleCourses[i].CourseId == courseIdList[j])
                        remainingCourses.Add(possibleCourses[i]);
            }

            List<Course> requiredCourses = new List<Course>();
            List<Course> electiveCourses = new List<Course>();
            List<Course> genEdCourses = new List<Course>();
            for (int i = 0; i < remainingCourses.Count; i++)
            {
                if (remainingCourses[i].Required == true)
                    requiredCourses.Add(remainingCourses[i]);
                if (remainingCourses[i].Elective == true)
                    electiveCourses.Add(remainingCourses[i]);
                if (remainingCourses[i].GenEd == true)
                    genEdCourses.Add(remainingCourses[i]);
            }
            
            IList<List<Course>> courses = new List<List<Course>>();
            courses.Add(requiredCourses);
            courses.Add(electiveCourses);
            courses.Add(genEdCourses);
            return View(courses);

        }

        // GET: Advise
        public ActionResult Advise()
        {
            var userId = User.Identity.GetUserId();
            var enrollments = db.Enrollments.Where(e => e.StudentId == userId).ToList();
                //.Include(e => e.User);
            var possibleCourses = db.Courses.ToList();
            IList<Course> remainingCourses = new List<Course>();

            IList<int> enrollmentIdList = new List<int>();
            for(int i =0; i< enrollments.Count; i++)
                enrollmentIdList.Add(enrollments[i].CourseId);

            IList<int> courseIdList = new List<int>();
            for (int i = 0; i < possibleCourses.Count; i++)
                courseIdList.Add(possibleCourses[i].CourseId);

            for(int i = 0; i< enrollmentIdList.Count;i++)
            {
                if (courseIdList.Contains(enrollmentIdList[i]))
                    courseIdList.Remove(enrollmentIdList[i]);
                    // remove the course that has enrollmentid[i]
            }

            for (int i = 0; i < possibleCourses.Count; i++)
            {
                for(int j = 0; j < courseIdList.Count; j++)
                if (possibleCourses[i].CourseId == courseIdList[j])
                    remainingCourses.Add(possibleCourses[i]);
            }
            if (remainingCourses.Count >=5)
            {
                List<Course> requiredCourses = new List<Course>();
                List<Course> electiveCourses = new List<Course>();
                List<Course> genEdCourses = new List<Course>();
                for (int i = 0; i < remainingCourses.Count; i++)
                {
                    if (remainingCourses[i].Required == true)
                        requiredCourses.Add(remainingCourses[i]);
                    if (remainingCourses[i].Elective == true)
                        electiveCourses.Add(remainingCourses[i]);
                    if (remainingCourses[i].GenEd == true)
                        genEdCourses.Add(remainingCourses[i]);
                }

                requiredCourses.Sort((x, y) => x.Level.CompareTo(y.Level));
                electiveCourses.Sort((x, y) => x.Level.CompareTo(y.Level));
                genEdCourses.Sort((x, y) => x.Level.CompareTo(y.Level));

                Stack<Course> requiredStack = new Stack<Course>();
                Stack<Course> electiveStack = new Stack<Course>();
                Stack<Course> genEdStack = new Stack<Course>();
                for (int i = requiredCourses.Count-1; i >= 0; i--)
                {
                    requiredStack.Push(requiredCourses[i]);
                }
                for (int i = electiveCourses.Count-1; i >= 0; i--)
                {
                    electiveStack.Push(electiveCourses[i]);
                }
                for (int i = genEdCourses.Count-1; i >= 0; i--)
                {
                    genEdStack.Push(genEdCourses[i]);
                }

                List<Course> advisedCourses = new List<Course>();
                
                while (advisedCourses.Count < 5)
                {
                    if (requiredStack.Count != 0)
                    {
                        advisedCourses.Add(requiredStack.Pop());
                    }
                    if (genEdStack.Count != 0)
                    {
                        advisedCourses.Add(genEdStack.Pop());
                    }
                    if (electiveStack.Count != 0)
                    {
                        advisedCourses.Add(electiveStack.Pop());
                    }
                }
                return View(advisedCourses);
            }
            else
            {
                return View(remainingCourses);
            }
        }

        // GET: Enrollments
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var enrollments = db.Enrollments.Include(e => e.Course).Include(e => e.User).Where(e=>e.StudentId == userId);
            return View(enrollments.ToList());
        }
        // GET: Enrollments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // GET: Enrollments/Create
        [Authorize]
        public ActionResult Create()
        {
           
           var pKey = User.Identity.GetUserId();
           

            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "Title");
            ViewBag.StudentId = new SelectList(db.Users.Where(P => P.Id == pKey), "Id", "Email");

            /*Original function: 
                ViewBag.StudentId = new SelectList(db.Users, "Id", "Email");*/
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EnrollmentId,Grade,CourseId,StudentId")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                db.Enrollments.Add(enrollment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            
            var pKey = User.Identity.GetUserId();
            
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "Title", enrollment.CourseId);
            ViewBag.StudentId = new SelectList(db.Users.Where(P => P.Id == pKey), "Id", "Email", enrollment.StudentId);

            /*Original function: 
                ViewBag.StudentId = new SelectList(db.Users, "Id", "Email", enrollment.StudentId); */
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5


        public ActionResult Edit(int? id)
        {
            var userId = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (userId != enrollment.StudentId)
            {
                return RedirectToAction("Index");
            }
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "Title", enrollment.CourseId);
            ViewBag.StudentId = new SelectList(db.Users, "Id", "Email", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EnrollmentId,Grade,CourseId,StudentId")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(enrollment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "CourseId", "Title", enrollment.CourseId);
            ViewBag.StudentId = new SelectList(db.Users, "Id", "Email", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public ActionResult Delete(int? id)
        {
            var userId = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if(userId != enrollment.StudentId)
            {
                return RedirectToAction("Index");
            }
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Enrollment enrollment = db.Enrollments.Find(id);
            db.Enrollments.Remove(enrollment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
