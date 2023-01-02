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
        private static Mutex mutex = new Mutex(); 

        public ConcQuestion(string txt, string tcode) : base(txt, tcode){}

        public override void AddAnswer(Answer a)
        {
            mutex.WaitOne();
            //todo: implement the body 
            this.Answers.AddLast(a);
            mutex.ReleaseMutex();
        }
    }

    public class ConcStudent: Student
    {
        // todo: add required fields
        private ConcExam? ConcExam;
        public LinkedListNode<ConcQuestion>? ConCurrent;
        public ConcStudent(int num, string name): base(num,name){}
        public override void AssignExam(Exam e)
        {
            //todo: implement the body
            ConcExam = (ConcExam) e;
            this.Log("[Exam is Assigned]");
         }

        public override void StartExam()
        {
            //todo: implement the body
            if(this.ConcExam is not null)
				this.ConCurrent = this.ConcExam.ConcQuestions.First;
			for(int i=0;i<this.ConcExam.ConcQuestions.Count;i++)
			{
                this.Think();
				this.ProposeAnswer();
            }
        }

        public override void Think()
        {
            //todo: implement the body
        }

        public override void ProposeAnswer()
        {
            //todo: implement the body
                        if (this.ConCurrent is not null)
            {
                this.Log("\n[Proposing Answer]\n");
				// add your answer
                this.ConCurrent.Value.AddAnswer(new Answer(this));
				// go for the next question
				this.ConCurrent = this.ConCurrent.Next;
                this.CurrentQuestionNumber++;
            }
        }


        public override void Log(string logText = "")
        {
            			string delim = " : " , nl = "\n";
            string ToString =  "Student "+delim+this.Number.ToString()+nl+"Exam: "+this.ConcExam.Number.ToString()+" Total Num Questions: "+this.ConcExam.ConcQuestions.Count.ToString()+delim+"Current Question: "+this.CurrentQuestionNumber.ToString();
			Console.WriteLine(logText+nl+ToString);
        }

    }
    public class ConcTeacher: Teacher
    {
        //todo: add required fields, if necessary
        public ConcExam? ConcExam;
        public ConcTeacher(string code, string name) : base(code,name){}

        public override void AssignExam(Exam e)
        {   
            this.ConcExam = (ConcExam) e;
            this.Log("[Exam is Assigned]");
        }
        public override void Think()
        {
            //todod: implement the body
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
    public class ConcExam: Exam
    {
        //todo: add required fields, if necessary
		private int QuestionNumber;
        private string Name;
        public int Number;
        public LinkedList<ConcQuestion> ConcQuestions;
        private static Mutex mutex = new Mutex(); 
        public ConcExam(int number, string name = "") : base(number,name){
            this.ConcQuestions = new LinkedList<ConcQuestion>();
			this.QuestionNumber = 0;
            this.Name = name;
            this.Number = number;
        }

        public override void AddQuestion(Teacher teacher, string text)
        {
            this.QuestionNumber++;
			ConcQuestion q = new ConcQuestion(text, teacher.Code);
			this.ConcQuestions.AddLast(q);
            Console.WriteLine(this.Number);
            this.Log("[Question is added]"+q.ToString());
        }

        //might have to be removed
    
        public override void Log(string logText = "")
        {
            string delim = " : ", nl = "\n";
            string ToString =  "Exam "+delim+this.Number.ToString()+delim+" Total Num Questions: "+this.QuestionNumber.ToString();
            Console.WriteLine(ToString + nl + logText);
        }
    }

    public class ConcClassroom : Classroom
    {
        //todo: add required fields, if necessary
        public ConcExam ConcExam;
        public LinkedList<ConcStudent> ConcStudents;
        public LinkedList<ConcTeacher> ConcTeachers;


        public ConcClassroom(int examNumber = 1, string examName = "Programming") : base(examNumber, examName)
        {
            //todo: implement the body
            this.ConcStudents = new LinkedList<ConcStudent>();
			this.ConcTeachers = new LinkedList<ConcTeacher>();
            this.ConcExam = new ConcExam(examNumber, examName);
        }

        public override void SetUp()
        {
            //todo: implement the body
            for(int i = 0; i<FixedParams.maxNumOfStudents; i++)
			{
				string std_name = " STUDENT NAME"; //todo: to be generated later
				this.ConcStudents.AddLast(new ConcStudent(i + 1, std_name));
			}
			for(int i=0; i<FixedParams.maxNumOfTeachers; i++)
            {
                string teacher_name = " TEACHER NAME"; //todo: to be generated later
                this.ConcTeachers.AddLast(new ConcTeacher((i + 1).ToString(), teacher_name));
			}
			// assign exams
			foreach (ConcTeacher t in this.ConcTeachers)
				t.AssignExam(this.ConcExam);
        }
        public override void PrepareExam(int maxNumOfQuestion)
        {
            //todo: implement the body
            List<Thread> threads = new();

            foreach (ConcTeacher t in this.ConcTeachers)
            {
                Thread thread = new(() => t.PrepareExam(maxNumOfQuestion));
                thread.Start();
                threads.Add(thread);

            }

            foreach(Thread thread in threads)
            {
                thread.Join();
            }
        }
        public override void DistributeExam()
        {
            //todo: implement the body
            foreach (ConcStudent s in this.ConcStudents)
				s.AssignExam(this.ConcExam);
        }
        public override void StartExams()
        {
            //todo: implement the body
            List<Thread> threads = new();

            foreach (ConcStudent s in this.ConcStudents)
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
            result = "#Students: " + this.ConcStudents.Count.ToString() + nl +
                "#Teachers: " + this.Teachers.Count.ToString() + nl +
                "#Questions: " + this.ConcExam.ConcQuestions.Count.ToString() + nl +
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

