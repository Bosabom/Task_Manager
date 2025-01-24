using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using System.Threading;
namespace SPZ_CourseWork
{
    public class TaskManager
    {
        public List<Process> current_processes = new List<Process>();//список текущих(активных) процессов
        public List<Process> previous_processes = new List<Process>();//список предыдущих процессов
        public List<Process> selected_processes = new List<Process>();//список выбранных процессов
       

        public string Name_Of_Process_That_Has_Been_Opened;
        public string Name_Of_Proccess_That_Has_Been_Closed;

        public bool Is_Process_Has_Been_Started=false;
        public bool Is_Process_Has_Been_Closed=false;

        public bool acess_for_selected_processes_changing=true;

        public ProcessModule modules_of_processes;
        public ProcessModule selected_processes_modules;

        public ManualResetEvent _shut_down_thread = new ManualResetEvent(false);
        public ManualResetEvent _pause_thread = new ManualResetEvent(true);

        
        public TaskManager()
        {
          //  current_processes = new List<Process>();
            //previous_processes = new List<Process>();
            previous_processes = Process.GetProcesses().ToList<Process>();
            //selected_processes = new List<Process>();
        }
        public void Get_Current_Processes()
        {
            previous_processes = current_processes;
            current_processes = Process.GetProcesses().ToList<Process>();//получение активных процессов и сохранение их в список processes
        }
        //функция для потока
       public void Analysis_Of_Processes()
        {
            while (true)//thread_is_going == true)
            {
                _pause_thread.WaitOne(Timeout.Infinite);
                if (_shut_down_thread.WaitOne(0))
                { break; }
                    Get_Current_Processes();
                if (previous_processes.Count < current_processes.Count) {
                    New_Process_Is_Opened();
                }
                 else if (previous_processes.Count > current_processes.Count) {
                    Process_Has_Been_Closed();
                }
                //else
                //    Thread.Sleep(3000);
            }
        }
        private void New_Process_Is_Opened()
        {
            //создание запроса события которое сигнализирует о создании нового процесса
            WqlEventQuery query =
                new WqlEventQuery("__InstanceCreationEvent",//запрашиваемое имя класса событий
                new TimeSpan(0, 0, 1),//определение времени ожидания события в 1 с
                "TargetInstance isa \"Win32_Process\"");//условие WHERE,применяемое к событиям указанного класса
            //инициализация детектора события и подписка на события которые соответствуют запросу 
            ManagementEventWatcher watcher =
                new ManagementEventWatcher(query);
            //задание времени когда детектор прекратит свою работу
           watcher.Options.Timeout = System.TimeSpan.MaxValue;
            //создание обьекта базового класса для обьекта класса ManagementEventWatcher и блокировка детектора для ожидания следующего события
            ManagementBaseObject e = watcher.WaitForNextEvent();
            //Получение информации от события,а именно имя процесса которое было создано
            string name_of_process_that_has_been_created = ((ManagementBaseObject)e
                ["TargetInstance"])["Name"].ToString().ToLower();
            watcher.Stop();//отмена подписки на событие
   
            foreach (Process pr in selected_processes)
            {
                try
                {
                    selected_processes_modules = pr.MainModule;
                    if (name_of_process_that_has_been_created == selected_processes_modules.ModuleName.ToString().ToLower())
                    {
                        Name_Of_Process_That_Has_Been_Opened = pr.MainModule.FileVersionInfo.FileDescription.ToString();
                        Is_Process_Has_Been_Started = true;
                        //break;
                    }
                }
                catch (Exception ex){}
            }
        }
        private void Process_Has_Been_Closed()
        {
            foreach(Process pr in previous_processes)
            {
                try
                {
                    pr.EnableRaisingEvents = true;
                    pr.Exited += new EventHandler(Closing_Process_Event);

                }
                catch (Exception e) { } //when (e is Win32Exception || e is FileNotFoundException) { }//может быть отказано в доступе
            }
        }
        private void Closing_Process_Event(object sender, EventArgs e)
        {
            Process Exited_Process = (Process)sender;
            foreach (Process proc in selected_processes)
            {
                try
                {
                    if (proc.ProcessName.ToString().ToLower() == Exited_Process.ProcessName.ToString().ToLower())
                    {
                        Name_Of_Proccess_That_Has_Been_Closed = proc.MainModule.FileVersionInfo.FileDescription.ToString();
                        Is_Process_Has_Been_Closed = true;

                    }
                }
                catch (Exception ex) { }
            }
        }
    }
}
//private Process Detect_New_Processes(List<Process> current_proc)
//{




//    //Get_Current_Processes();

//    //foreach (Process started_process in selected_processes)
//    //{
//    //    if (previous_processes.Contains(started_process) == false && current_processes.Contains(started_process) == true)//процесс отсутствует в предыдущем списке но есть в нынешнем
//    //    {
//    //        this.previous_processes = current_proc;
//    //        return started_process;
//    //    }
//    //}
//    //return null;
//}
//private Process Detect_Closed_Processes(List<Process> current_proc)
//{
//    Get_Current_Processes();


//    foreach (Process closed_process in selected_processes)
//    {
//        if (previous_processes.Contains(closed_process) == false && current_processes.Contains(closed_process) == false)//процесс присутствует в предыдущем списке но его нет в нынешнем
//        {
//            closed_process.WaitForExit();
//            this.previous_processes = current_proc;
//            return closed_process;

//        }
//    }

//    return process_that_has_been_closed;
//}
// foreach(Process pr in selected_processes)
// {
//     try
//     {
//         if (current_processes.Contains(pr) == false)
//         {
//             Name_Of_Proccess_That_Has_Been_Closed = pr.MainModule.FileVersionInfo.FileDescription.ToString();
//             Is_Process_Has_Been_Closed = true;
//         }
//     }
//     catch (Exception ex) { }
// }
//previous_processes = current_processes;//