using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32.TaskScheduler;

namespace Server
{
    class StartupManager
    {
        public static void AddToStartup()
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Description = "Executa ResourceMonitor ao iniciar o sistema";
            taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;

            taskDefinition.Triggers.Add(new LogonTrigger());

            taskDefinition.Actions.Add(new ExecAction("\"" + Assembly.GetExecutingAssembly().Location + "\"", null, null));

            string taskName = "ResourceMonitorServer";
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, taskDefinition);
        }

        public static void RemoveFromStartup()
        {
            TaskService taskService = new TaskService();

            taskService.RootFolder.DeleteTask("ResourceMonitorServer");
        }
    }
}
