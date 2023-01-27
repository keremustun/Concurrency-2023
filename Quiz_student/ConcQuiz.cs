using System;
using Quiz;

namespace ConcQuiz
{
    public class ConcAnswer : Answer
    {
        public ConcAnswer(ConcStudent std, string txt = "") : base(std, txt) { }
    }
    public class ConcQuestion : Question
    {
        private static Mutex mutex = new Mutex();

        public ConcQuestion(string txt, string tcode) : base(txt, tcode) { }

        public override void AddAnswer(Answer a)
        {
            mutex.WaitOne();
            this.Answers.AddLast(a);
            mutex.ReleaseMutex();
        }
    }

    public class ConcStudent : Student
    {
        private ConcExam? ConcExam;
        public LinkedListNode<ConcQuestion>? ConCurrent;
        public ConcStudent(int num, string name) : base(num, name) { }
        public override void AssignExam(Exam e)
        {
            ConcExam = (ConcExam)e;
            this.Log("[Exam is Assigned]");
        }

        public override void StartExam()
        {
            if (this.ConcExam is not null)
                this.ConCurrent = this.ConcExam.Questions.First;
            for (int i = 0; i < this.ConcExam.Questions.Count; i++)
            {
                this.Think();
                this.ProposeAnswer();
            }
        }

        public override void Think()
        {
            Thread.Sleep(new Random().Next(FixedParams.minThinkingTimeStudent, FixedParams.maxThinkingTimeStudent));
        }

        public override void ProposeAnswer()
        {
            if (this.ConCurrent is not null)
            {
                this.Log("\n[Proposing Answer]\n");
                this.ConCurrent.Value.AddAnswer(new Answer(this));
                this.ConCurrent = this.ConCurrent.Next;
                this.CurrentQuestionNumber++;
            }
        }


        public override void Log(string logText = "")
        {
            string delim = " : ", nl = "\n";
            string ToString = "Student " + delim + this.Number.ToString() + nl + "Exam: " + this.ConcExam.Number.ToString() + " Total Num Questions: " + this.ConcExam.Questions.Count.ToString() + delim + "Current Question: " + this.CurrentQuestionNumber.ToString();
            Console.WriteLine(logText + nl + ToString);
        }

    }
    public class ConcTeacher : Teacher
    {
        public ConcExam? ConcExam;
        public ConcTeacher(string code, string name) : base(code, name) { }

        public override void AssignExam(Exam e)
        {
            this.ConcExam = (ConcExam)e;
            this.Log("[Exam is Assigned]");
        }
        public override void Think()
        {
            Thread.Sleep(new Random().Next(FixedParams.minThinkingTimeTeacher, FixedParams.maxThinkingTimeTeacher));
        }
        public override void ProposeQuestion()
        {
            this.Log("[Proposing Question]");

            string qtext = " [This is the text for Question] ";
            if (this.ConcExam is not null)

                this.ConcExam.AddQuestion(this, qtext);
        }
        public override void PrepareExam(int maxNumOfQuestions)
        {
            for (int i = 0; i < maxNumOfQuestions; i++)
            {
                this.Think();
                this.ProposeQuestion();
            }
        }
        public override void Log(string logText = "")
        {
            string nl = "\n";
            Console.WriteLine(this.ToString() + nl + logText);
        }
    }
    public class ConcExam : Exam
    {
        private int QuestionNumber;
        private string Name;
        public int Number;
        public LinkedList<ConcQuestion> Questions;
        private static Mutex mutex = new Mutex();
        public ConcExam(int number, string name = "") : base(number, name)
        {
            this.Questions = new LinkedList<ConcQuestion>();
            this.QuestionNumber = 0;
            this.Name = name;
            this.Number = number;
        }

        public override void AddQuestion(Teacher teacher, string text)
        {
            this.QuestionNumber++;
            ConcQuestion q = new ConcQuestion(text, teacher.Code);
            mutex.WaitOne();
            this.Questions.AddLast(q);
            mutex.ReleaseMutex();
            this.Log("[Question is added]" + q.ToString());
        }


        public override void Log(string logText = "")
        {
            string delim = " : ", nl = "\n";
            string ToString = "Exam " + delim + this.Number.ToString() + delim + " Total Num Questions: " + this.QuestionNumber.ToString();
            Console.WriteLine(ToString + nl + logText);
        }
    }

    public class ConcClassroom : Classroom
    {
        public ConcExam Exam;
        public LinkedList<ConcStudent> Students;
        public LinkedList<ConcTeacher> Teachers;


        public ConcClassroom(int examNumber = 1, string examName = "Programming") : base(examNumber, examName)
        {
            this.Students = new LinkedList<ConcStudent>();
            this.Teachers = new LinkedList<ConcTeacher>();
            this.Exam = new ConcExam(examNumber, examName);
        }

        public static string GenerateName(int length)
        {
            string[] vwls = { "a", "e", "i", "o", "u", "y", "ae" };
            string[] cnsnts = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            
            Random r = new Random();

            string studentName = "";
            studentName += cnsnts[r.Next(cnsnts.Length)].ToUpper();
            studentName += vwls[r.Next(vwls.Length)];
         
            int x = 2; 

            while (x < length)
            {
                studentName += cnsnts[r.Next(cnsnts.Length)];
                x++;
                studentName += vwls[r.Next(vwls.Length)];
                x++;
            }

            return studentName;
        }

        public override void SetUp()
        {
            for (int i = 0; i < FixedParams.maxNumOfStudents; i++)
            {
                string std_name = GenerateName(6); //todo: to be generated later
                this.Students.AddLast(new ConcStudent(i + 1, std_name));
            }
            for (int i = 0; i < FixedParams.maxNumOfTeachers; i++)
            {
                string teacher_name = GenerateName(7); //todo: to be generated later
                this.Teachers.AddLast(new ConcTeacher((i + 1).ToString(), teacher_name));
            }
            foreach (ConcTeacher t in this.Teachers)
                t.AssignExam(this.Exam);
        }

        public override void PrepareExam(int maxNumOfQuestion)
        {
            List<Thread> threads = new();

            foreach (ConcTeacher t in this.Teachers)
            {
                Thread thread = new(() => t.PrepareExam(maxNumOfQuestion));
                thread.Start();
                threads.Add(thread);
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }
        public override void DistributeExam()
        {
            foreach (ConcStudent s in this.Students)
                s.AssignExam(this.Exam);
        }
        public override void StartExams()
        {
            List<Thread> threads = new();

            foreach (ConcStudent s in this.Students)
            {
                Thread thread = new(() => s.StartExam());
                thread.Start();
                threads.Add(thread);

            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }


        }
        public string GetStatistics()
        {
            string result = "", nl = "\n";
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

