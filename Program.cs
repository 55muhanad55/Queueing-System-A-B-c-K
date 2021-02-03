using System;
using System.Linq;
using System.Collections.Generic;

namespace Simulation_Coursework
{
    class Program
    {
        static void Main(string[] args)
        {
       
            job nextjob = new job ();

            while (QueueingSystem.CurrentTime < QueueingSystem.SimulationTime)
            {
                if (QueueingSystem.servers.Count > 0)
                {
                    QueueingSystem.nextdepartur = QueueingSystem.servers.Min(job => job.inServeJob.DepartureTime);
                }

                if ((QueueingSystem.Uc ==0) || nextjob.ArrivalTime <= QueueingSystem.nextdepartur)
                {
                    QueueingSystem.CurrentTime = nextjob.ArrivalTime;
                    if (!QueueingSystem.BoundedQueue || QueueingSystem.Maximumlength >= QueueingSystem.q.Count)
                        QueueingSystem.q.Enqueue(nextjob);
                    nextjob = new job();

                    if (QueueingSystem.Uc < QueueingSystem.c)
                    {
                        QueueingSystem.Uc++;
                        QueueingSystem.servers.Add(new Server(QueueingSystem.q.Dequeue()));
                    }
                }
                else{
                    var finsihingServer = QueueingSystem.servers.Find(x => x.inServeJob.DepartureTime == QueueingSystem.nextdepartur);
                    QueueingSystem.CurrentTime = finsihingServer.inServeJob.DepartureTime;

                    if (QueueingSystem.Uc == 1)
                        QueueingSystem.nextdepartur = QueueingSystem.SimulationTime;

                    QueueingSystem.completions.Add(finsihingServer.inServeJob);
                    QueueingSystem.servers.Remove(finsihingServer);
                    QueueingSystem.Uc--;

                    if (QueueingSystem.q.Count > 0)
                    {
                        QueueingSystem.Uc++;
                        QueueingSystem.servers.Add(new Server(QueueingSystem.q.Dequeue()));
                    }
                }
            }

            double TotalWaitingTime = 0;
            double TotalServiceTime = 0;
            double TotalQueueLenth = 0;
            double TotalInterarrival = 0;
            double AvaregeIdle = 0;
            double lambda = 0;
            double mue = 0;

            int WaiteJobs = 0;

            Console.WriteLine($"Total Simulation Time: {QueueingSystem.SimulationTime}");
            Console.WriteLine($"Number of Servers: {QueueingSystem.c}");
            Console.WriteLine($"Bounded Queue Enabled: {QueueingSystem.BoundedQueue}");

            if (QueueingSystem.BoundedQueue)
                Console.WriteLine($"Queue capacity: {QueueingSystem.Maximumlength}");

            Console.WriteLine("|====================================================================|");
            Console.WriteLine("| job |Inter T |Arival T|Servic T|Serv begin|Wait T|Serv End|Total  T|");
            Console.WriteLine("|-----|--------|--------|--------|----------|------|--------|--------|");
            int jobs = 1;
            foreach (var item in QueueingSystem.completions.OrderBy(x => x.ArrivalTime))
            {
                Console.WriteLine($"| {jobs++.ToString("000")} | {item.InterArrivalTime.ToString("00.000")} | {item.ArrivalTime.ToString("00.000")} | {item.serviceTime.ToString("00.000")} |" +
                    $"  {item.starttingService.ToString("00.000")}  |{item.WaitingTime.ToString("00.000")}| {item.serviceEnd.ToString("00.000")} | {item.TotalTime.ToString("00.000")} |");

                if(item.WaitingTime > 0)
                {
                    TotalWaitingTime += item.WaitingTime;
                    WaiteJobs++;
                }
                TotalServiceTime += item.serviceTime;
                TotalQueueLenth += item.TotalTime;
                TotalInterarrival += item.InterArrivalTime;
            }

            AvaregeIdle = QueueingSystem.CurrentTime - (TotalServiceTime/QueueingSystem.c);
            lambda = jobs / TotalInterarrival;
            mue = jobs / TotalServiceTime;

            double Pout = AvaregeIdle / QueueingSystem.CurrentTime;


            Console.WriteLine("\n");
            Console.WriteLine($"the average waiting time: {TotalWaitingTime/ jobs} sec");
            Console.WriteLine($"the average waiting time of those who wait: {TotalWaitingTime/WaiteJobs} sec");
            Console.WriteLine($"the state probabilities : {Pout} %");
            Console.WriteLine($"the utilization: {(TotalServiceTime/QueueingSystem.CurrentTime)/QueueingSystem.c} %");
            Console.WriteLine($"the mean queue length: {TotalQueueLenth / jobs}");
            Console.WriteLine($"the throughput: {jobs / QueueingSystem.CurrentTime} job/sec");
            Console.WriteLine($"the Response time: {(TotalQueueLenth / jobs)/(jobs / QueueingSystem.CurrentTime)} sec");





        }
    }

    public static class QueueingSystem
    {
        public static double SimulationTime = 50;
        public static double CurrentTime = 0;
        public static double nextdepartur = SimulationTime;
        public static int c = 3;
        public static int Uc = 0;
        public static bool BoundedQueue = false;
        public static int Maximumlength = 10;
        public static Queue<job> q = new Queue<job>();
        public static List<Server> servers = new List<Server>();
        public static List<job> completions = new List<job>();
        
        public static Random rnd = new Random((int)DateTime.Now.Ticks);


        public static double uniform(int min, int max)
        {
            return (double)(rnd.Next(min * 100, max * 100) / (double)100);
        }
        public static double func()
        {
            return (double)(2.0 * (double)Math.Sqrt(Math.Sqrt(uniform(0,2))));
        }

       
    }

    public class job
    {
        public double InterArrivalTime { get; set; }
        public double ArrivalTime { get; set; }
        public double DepartureTime { get; set; }
        public double WaitingTime { get; set; }
        public double starttingService { get; set; }
        public double serviceTime { get; set; }
        public double serviceEnd { get; set; }
        public double TotalTime { get; set; }

        public job()
        {
            InterArrivalTime = QueueingSystem.uniform(0, 2);
            ArrivalTime = QueueingSystem.CurrentTime + InterArrivalTime;
        }



    }
    public class Server
    {
        public job inServeJob { get; set; }

        public Server(job job)
        {
            inServeJob = job;
            inServeJob.serviceTime = QueueingSystem.func();
            inServeJob.DepartureTime = QueueingSystem.CurrentTime + inServeJob.serviceTime;
            inServeJob.WaitingTime = QueueingSystem.CurrentTime - inServeJob.ArrivalTime;
            inServeJob.starttingService = QueueingSystem.CurrentTime;
            inServeJob.serviceEnd = inServeJob.starttingService + inServeJob.serviceTime;
            inServeJob.TotalTime = inServeJob.serviceEnd - inServeJob.ArrivalTime;
        }
    }
}
