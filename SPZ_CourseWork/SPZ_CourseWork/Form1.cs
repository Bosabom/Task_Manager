using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using System.Threading;
namespace SPZ_CourseWork
{
    public partial class Form1 : Form
    {
        TaskManager task_manager;
        Thread analyze_thread;
        public Form1()
        {
            task_manager = new TaskManager();
            analyze_thread = new Thread(task_manager.Analysis_Of_Processes);
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            DisplayAllProcessesOnViewList();
        }
        //обновление списка запущенных процессов по нажатию кнопки
        private void button1_Click(object sender, EventArgs e)
        {   
            DisplayAllProcessesOnViewList();
        }
        private void DisplayAllProcessesOnViewList()
        {
            task_manager.Get_Current_Processes();
            listView1.Items.Clear();//очистка данных в ListView
            string[] info_with_module_name;
            string[] info_without_module_name_and_description;
            foreach (Process proc in task_manager.current_processes)
            {
                try
                {
                    task_manager.modules_of_processes = proc.MainModule;//получение исполняемого файла конкретного процесса
                    info_with_module_name = new string[] { proc.ProcessName.ToString(),//имя процесса
                        proc.Id.ToString(),//PID
                        proc.MainModule.FileVersionInfo.FileDescription.ToString(),//описание процесса
                        proc.Responding.ToString(),//статус
                        task_manager.modules_of_processes.ModuleName.ToString() };//исполняемый файл
                    listView1.Items.Add(new ListViewItem(info_with_module_name));
                }
                catch
                {
                    info_without_module_name_and_description = new string[] { proc.ProcessName.ToString(), proc.Id.ToString(),"", proc.Responding.ToString(),""};
                    listView1.Items.Add(new ListViewItem(info_without_module_name_and_description));           
                }
            }
            Text = "Количество активных процессов " + task_manager.current_processes.Count.ToString();
        }
        //поиск по названию процесса
        private void DisplayAllProcessesOnViewList(List<Process> certain_processes, string _processname)
        {
            task_manager.Get_Current_Processes();
            listView1.Items.Clear();//очистка данных в ListView
            string[] info_with_module_name;
            string[] info_without_module_name_and_description;
            foreach (Process pr in certain_processes)
            {
                if (pr != null)//проверка существует ли процесс вообще
                {
                    try
                    {
                        task_manager.modules_of_processes = pr.MainModule;//получение исполняемого файла конкретного процесса
                        info_with_module_name = new string[] { pr.ProcessName.ToString(),//имя процессв
                            pr.Id.ToString(),//PID
                            pr.MainModule.FileVersionInfo.FileDescription.ToString() ,//описание
                            pr.Responding.ToString(),//статус
                            task_manager.modules_of_processes.ModuleName.ToString() };//исполняемый файл
                        listView1.Items.Add(new ListViewItem(info_with_module_name));
                    }
                    catch
                    {
                        info_without_module_name_and_description = new string[] { pr.ProcessName.ToString(), pr.Id.ToString(), "", pr.Responding.ToString(), "" };
                        listView1.Items.Add(new ListViewItem(info_without_module_name_and_description));
                    }
                }
            }
            Text = $"Количество процессов '{_processname}' " + certain_processes.Count.ToString();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            task_manager.Get_Current_Processes();
            //временный список процессов для поиска процесса с определенным именем
            List<Process> searched_processes = task_manager.current_processes.Where((x) => x.ProcessName.ToLower().Contains(textBox1.Text.ToLower())).ToList<Process>();
            DisplayAllProcessesOnViewList(searched_processes, textBox1.Text);//отображение искомых процессов
        }
        private void button3_Click(object sender, EventArgs e)
        {
                if (listView1.SelectedItems[0] != null)
                {
                    Process selected_process_by_user = task_manager.current_processes.Where((x) => x.Id.ToString() == listView1.SelectedItems[0].SubItems[1].Text).ToList()[0];
                    task_manager.selected_processes.Add(selected_process_by_user);
                    MessageBox.Show($"Процесс '{selected_process_by_user.ProcessName}' был успешно выбран и добавлен в список!", "Уведомление");
                }
                else
                    MessageBox.Show("Сначала выберите процесс в списке!", "Уведомление"); 
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (task_manager.selected_processes.Count != 0)
            {
                var form2 = new SelectedProcesses(task_manager.selected_processes,task_manager.acess_for_selected_processes_changing);
                form2.Show();
            }
            else
                MessageBox.Show("Список выбранных процессов пуст! Выберите процессы из списка для дальнейшей работы.","Предупреждение");
        }
        private void button5_Click(object sender, EventArgs e)//запуск потока
        {
            if (task_manager.selected_processes.Count != 0)
            {
                analyze_thread.Start();
                timer1.Start();
                button5.Enabled = false;//блокировка кнопки для начала
                button7.Enabled = false;//блокировка кнопки для возобновления
                button3.Enabled = false;//блокировка кнопки добавить новый процесс в список
                task_manager.acess_for_selected_processes_changing = false;//сигнал для блокировки кнопки "Удалить" в форме со списком выбранных процессов
                MessageBox.Show("Анализ начался!", "Уведомление");
            }
            else
                MessageBox.Show("Нет элементов в выбранном вами списке процессов!", "Ошибка");
        }
        private void timer1_Tick(object sender, EventArgs e)//выведение MessageBox в случае открытия/закрытия приложения
        {
            if (task_manager.Is_Process_Has_Been_Started == true)
            {
                task_manager.Is_Process_Has_Been_Started = false;
                MessageBox.Show($"Приложение {task_manager.Name_Of_Process_That_Has_Been_Opened} было запущено!", "Произошло событие!");
            }
            else if(task_manager.Is_Process_Has_Been_Closed==true)
            {
                task_manager.Is_Process_Has_Been_Closed = false;
                MessageBox.Show($"Приложение {task_manager.Name_Of_Proccess_That_Has_Been_Closed} было завершено!", "Произошло событие!");
            }
        }
        private void button6_Click(object sender, EventArgs e) //остановка потока
        {
            button3.Enabled = true;//разблокировка кнопки добавить новый процесс в список
            button7.Enabled = true;//разблокировка кнопки для возобновления
            button6.Enabled = false;//блокировка кнопки для приостановки
            task_manager.acess_for_selected_processes_changing = true;//сигнал для разблокировки кнопки "Удалить" в форме со списком выбранных процессов
            task_manager._pause_thread.Reset();
            MessageBox.Show("Анализ был приостановлен!","Уведомление");
        }
        private void button7_Click(object sender, EventArgs e)//возобновление потока
        {
            try
            { 
                button3.Enabled = false;//блокировка кнопки добавить новый процесс в список
                button6.Enabled = true;//разблокировка кнопки для приостановки
                button7.Enabled = false;//блокировка кнопки для возобновления
                task_manager.acess_for_selected_processes_changing = false;//сигнал для блокировки кнопки "Удалить" в форме со списком выбранных процессов
                task_manager._pause_thread.Set();
                MessageBox.Show("Возобновился анализ!", "Сообщение");
            }   
            catch(Exception ex)
            {
                MessageBox.Show("Возможно анализ еще идет или он был не запущен..", "Предупреждение");
            }   
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (analyze_thread.IsAlive)
            {
                task_manager._shut_down_thread.Set();
                task_manager._pause_thread.Set();
                timer1.Stop();
                //analyze_thread.Join();
                analyze_thread.Abort();
                MessageBox.Show("Всего доброго!", "Прощание");
            }
        }
    }
}
