using System;
using Quiz;

// todo: Complete the implementation

/// Concurrent version of the Quiz
namespace ConcQuiz
{
    public class ConcAnswer: Answer
    {
        public ConcAnswer(ConcStudent std, string txt = ""): base(std,txt){}
    }
    public class ConcQuestion : Question
    {
        //todo: add required fields, if necessary

        public ConcQuestion(string txt, string tcode) : base(txt, tcode){}

        public override void AddAnswer(Answer a)
        {
            //todo: implement the body 
        }
    }

    public class ConcStudent: Student
    {
        // todo: add required fields

        public ConcStudent(int num, string name): base(num,name){}

        public override void AssignExam(Exam e)
        {
            //todo: implement the body
        }

        public override void StartExam()
        {
            //todo: implement the body
        }

        public override void Think()
        {
            //todo: implement the body
        }

        public override void ProposeAnswer()
        {
            //todo: implement the body
        }

        public override void Log(string logText = "")
        {
            base.Log();
        }

    }
    public class ConcTeacher: Teacher
    {
        //todo: add required fields, if necessary

        public ConcTeacher(string code, string name) : base(code,name){}

        public override void AssignExam(Exam e)
        {
            //todo: implement the body
        }
        public override void Think()
        {
            //todo: implement the body
        }
        public override void ProposeQuestion()
        {
            //todo: implement the body
        }
        public override void PrepareExam(int maxNumOfQuestions)
        {
            //todo: implement the body
        }
        public override void Log(string logText = "")
        {
            base.Log();
        }
    }
    public class ConcExam: Exam
    {
        //todo: add required fields, if necessary

        public ConcExam(int number, string name = "") : base(number,name){}

        public override void AddQuestion(Teacher teacher, string text)
        {
            //todo: implement the body
        }
        public override void Log(string logText = "")
        {
            base.Log();
        }
    }

    public class ConcClassroom : Classroom
    {
        //todo: add required fields, if necessary

        public ConcClassroom(int examNumber = 1, string examName = "Programming") : base(examNumber, examName)
        {
            //todo: implement the body
        }

        public override void SetUp()
        {
            //todo: implement the body
            for(int i = 0; i<FixedParams.maxNumOfStudents; i++)
			{
				string std_name = " STUDENT NAME"; //todo: to be generated later
				this.Students.AddLast(new Student(i + 1, std_name));
			}
			for(int i=0; i<FixedParams.maxNumOfTeachers; i++)
            {
                string teacher_name = " TEACHER NAME"; //todo: to be generated later
                this.Teachers.AddLast(new Teacher((i + 1).ToString(), teacher_name));
			}
			// assign exams
			foreach (Teacher t in this.Teachers)
				t.AssignExam(this.Exam);
        }

        public override void PrepareExam(int maxNumOfQuestion)
        {
            //todo: implement the body
            foreach (Teacher t in this.Teachers)
				t.PrepareExam(maxNumOfQuestion);
        }
        public override void DistributeExam()
        {
            //todo: implement the body
            foreach (Student s in this.Students)
				s.AssignExam(this.Exam);
        }
        public override void StartExams()
        {
            //todo: implement the body
            List<Thread> threads = new();

            foreach (Student s in this.Students)
            {
                Thread thread = new(() => s.StartExam());
                thread.Start();
                threads.Add(thread);

            }

            foreach(Thread thread in threads)
            {
                thread.Join();
            }
        
            
        }

        public string GetStatistics()
        {
            string result = "" , nl = "\n";
            int totalNumOfAnswers = 0;
            foreach (Question q in this.Exam.Questions)
                totalNumOfAnswers += q.Answers.Count;
            result = "#Students: " + this.Students.Count.ToString() + nl +
                "#Teachers: " + this.Teachers.Count.ToString() + nl +
                "#Questions: " + this.Exam.Questions.Count.ToString() + nl +
                "#Answers: " + totalNumOfAnswers.ToString();
            return result;
        }
    }
    //THIS CLASS (QUIZCONCURRENT) SHOULD NOT BE CHANGED
    public class QuizConcurrent
    {
        ConcClassroom classroom;

        public QuizConcurrent()
        {
            this.classroom = new ConcClassroom();
        }
        public void RunExams()
        {
            classroom.SetUp();
            classroom.PrepareExam(Quiz.FixedParams.maxNumOfQuestions);
            classroom.DistributeExam();
            classroom.StartExams();
        }
        public string FinalResult()
        {
            return classroom.GetStatistics();
        }

    }
}

