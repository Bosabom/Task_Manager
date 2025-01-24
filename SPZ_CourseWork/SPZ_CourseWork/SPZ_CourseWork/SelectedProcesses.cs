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
namespace SPZ_CourseWork
{
    public partial class SelectedProcesses : Form
    {
        List<Process> processes_for_analyze;
        public ProcessModule modules_of_selected_processes;
        bool can_process_be_removed;
        public SelectedProcesses(List<Process> chosen_processes,bool access_for_changings)
        {
            processes_for_analyze = chosen_processes;
            can_process_be_removed = access_for_changings;
            InitializeComponent();

        }
        //удаление выбранного процесса из списка
        private void button1_Click(object sender, EventArgs e)
        {
            try{Process selected_process_by_user = processes_for_analyze.Where((x) => x.Id.ToString() == listView1.SelectedItems[0].SubItems[1].Text).ToList()[0];
                processes_for_analyze.Remove(selected_process_by_user);
                Update_List();}
            catch (Exception ex){MessageBox.Show("Что-то пошло не так...");}  
        }
        private void Update_List()
        {
            listView1.Items.Clear();
            string[] info_with_description;
            string[] info_without_description;
            foreach (Process proc in processes_for_analyze)
            {
                try
                {
                    modules_of_selected_processes = proc.MainModule;
                    info_with_description = new string[] { proc.ProcessName.ToString(),//имя процесса
                    proc.Id.ToString(),//PID
                    proc.MainModule.FileVersionInfo.FileDescription.ToString(),
                    modules_of_selected_processes.ModuleName.ToString()}; //описание процесса
                    listView1.Items.Add(new ListViewItem(info_with_description));
                }
                catch
                {
                    info_without_description = new string[] { proc.ProcessName.ToString(), proc.Id.ToString(), "",""};
                    listView1.Items.Add(new ListViewItem(info_without_description));
                }
            }
            Text = "Количество выбранных процессов: " + processes_for_analyze.Count.ToString();
        }
        private void SelectedProcesses_Load(object sender, EventArgs e)
        {
            if (can_process_be_removed == false)
                button1.Enabled = false;
            else
                button1.Enabled = true;
            Update_List();
        }
    }
}
